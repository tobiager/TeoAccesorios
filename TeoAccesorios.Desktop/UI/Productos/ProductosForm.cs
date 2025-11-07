using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class ProductosForm : Form
    {
        // ====== UI: grilla + filtros ======
        private readonly DataGridView grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
        private readonly TextBox txtBuscar = new() { PlaceholderText = "Buscar por nombre...", Width = 220 };
        private readonly ComboBox cboCategoria = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200 };
        private readonly ComboBox cboSubcategoria = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200 };
        private readonly BindingSource bs = new();

        // ====== Selector de columnas ======
        private readonly Button btnColumnas = new() { Text = "⚙ Columnas ▾" };
        private readonly ContextMenuStrip cmsColumnas = new();
        private bool _buildingMenu = false;

        // ====== Persistencia por USUARIO + VISTA ======
        private const string VIEW_KEY = "Productos"; // clave lógica de esta pantalla
        private string PrefsPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TeoAccesorios",
            $"grid_{VIEW_KEY}_{Sanitize(GetUserKey())}.json"
        );

        public ProductosForm()
        {
            Text = "Productos";
            Width = 1200;
            Height = 720;
            StartPosition = FormStartPosition.CenterParent;

            // Barra superior (filtros + ABM + columnas)
            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(8), AutoSize = false, WrapContents = false };
            top.Controls.Add(txtBuscar);
            top.Controls.Add(new Label { Text = "Categoría", AutoSize = true, Padding = new Padding(8, 8, 4, 0) });
            top.Controls.Add(cboCategoria);
            top.Controls.Add(new Label { Text = "Subcategoría", AutoSize = true, Padding = new Padding(8, 8, 4, 0) });
            top.Controls.Add(cboSubcategoria);

            // Botón de columnas (context menu)
            btnColumnas.Click += (s, e) =>
            {
                ConstruirMenuColumnas();
                cmsColumnas.Show(btnColumnas, new System.Drawing.Point(0, btnColumnas.Height));
            };
            top.Controls.Add(btnColumnas);

            // ABM para Admin y Gerente (Gerente tiene todos los permisos)
            if (Sesion.Rol == RolUsuario.Admin || Sesion.Rol == RolUsuario.Gerente)
            {
                var btnNuevo = new Button { Text = "Nuevo", AutoSize = true };
                var btnEditar = new Button { Text = "Editar", AutoSize = true };
                var btnEliminar = new Button { Text = "Eliminar", AutoSize = true };
                var btnVerInactivos = new Button { Text = "Ver Productos Inactivos", AutoSize = true };
                top.Controls.AddRange(new Control[] { btnNuevo, btnEditar, btnEliminar, btnVerInactivos });

                btnVerInactivos.Click += (s, e) =>
                {
                    using (var f = new ProductosInactivosForm())
                    {
                        f.ShowDialog(this);
                    }
                    LoadData();
                };

                btnNuevo.Click += (s, e) =>
                {
                    var pr = new Producto();
                    using var f = new ProductoEditForm(pr);
                    if (f.ShowDialog(this) == DialogResult.OK)
                    {
                        // Confirmación antes de insertar
                        var confirm = MessageBox.Show(
                            $"¿Confirmás agregar el producto \"{pr.Nombre}\"?",
                            "Confirmar alta",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                        if (confirm == DialogResult.Yes)
                        {
                            pr.Id = Repository.InsertarProducto(pr);
                            LoadData();
                        }
                    }
                };

                btnEditar.Click += (s, e) =>
                {
                    if (grid.CurrentRow?.DataBoundItem is Producto sel)
                    {
                        var tmp = new Producto
                        {
                            Id = sel.Id,
                            Nombre = sel.Nombre,
                            Descripcion = sel.Descripcion,
                            Precio = sel.Precio,
                            Stock = sel.Stock,
                            StockMinimo = sel.StockMinimo,
                            CategoriaId = sel.CategoriaId,
                            SubcategoriaId = sel.SubcategoriaId,
                            Activo = sel.Activo
                        };
                        using var f = new ProductoEditForm(tmp);
                        if (f.ShowDialog(this) == DialogResult.OK)
                        {
                            // Confirmación antes de actualizar
                            var confirm = MessageBox.Show(
                                $"¿Confirmás guardar los cambios en el producto \"{tmp.Nombre}\"?",
                                "Confirmar edición",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);

                            if (confirm == DialogResult.Yes)
                            {
                                Repository.ActualizarProducto(tmp);
                                LoadData();
                            }
                        }
                    }
                };

                btnEliminar.Click += (s, e) =>
                {
                    if (grid.CurrentRow?.DataBoundItem is Producto sel)
                    {
                        var confirm = MessageBox.Show(
                            $"¿Estás seguro que deseas eliminar (desactivar) el producto \"{sel.Nombre}\"?\nEsta acción puede revertirse desde 'Ver Productos Inactivos'.",
                            "Confirmar eliminación",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);

                        if (confirm == DialogResult.Yes)
                        {
                            Repository.EliminarProducto(sel.Id);
                            LoadData();
                        }
                    }
                };
            }
            else
            {
                grid.ReadOnly = true;
            }

            // Layout
            Controls.Add(grid);
            Controls.Add(top);

            // Estética y locks (si tenés estos helpers en tu proyecto)
            GridHelper.Estilizar(grid);
            GridHelperLock.Apply(grid);

            // Filtros
            txtBuscar.TextChanged += (s, e) => LoadData();
            cboCategoria.SelectedIndexChanged += (s, e) => { CargarSubcategorias(); LoadData(); };
            cboSubcategoria.SelectedIndexChanged += (s, e) => LoadData();

            // Aplicar prefs cuando la grilla termina de enlazar datos
            grid.DataBindingComplete += (s, e) =>
            {
                AplicarPreferenciasGuardadas();   // por usuario + vista
                ConstruirMenuColumnas();          // sincroniza checks
            };

            // Cargar combos y datos
            cboCategoria.Items.Clear();
            cboCategoria.Items.Add(new ComboItem { Text = "Todas las categorías", Value = null });
            foreach (var c in Repository.ListarCategorias())
                cboCategoria.Items.Add(new ComboItem { Text = $"{c.Id} - {c.Nombre}", Value = c.Id });
            cboCategoria.SelectedIndex = 0;

            CargarSubcategorias();
            LoadData();

            // Guardar prefs al cerrar (por si quedó algo sin persistir)
            FormClosing += (s, e) => GuardarPreferencias();
        }

        // ====== Subcargas ======
        private void CargarSubcategorias()
        {
            cboSubcategoria.Items.Clear();
            cboSubcategoria.Items.Add(new ComboItem { Text = "Todas las subcategorías", Value = null });

            int? catId = (cboCategoria.SelectedItem as ComboItem)?.Value;
            var subs = Repository.ListarSubcategorias(catId);
            foreach (var s in subs)
                cboSubcategoria.Items.Add(new ComboItem { Text = $"{s.Id} - {s.Nombre}", Value = s.Id });

            cboSubcategoria.SelectedIndex = 0;
        }

        private void LoadData()
        {
            var data = Repository.ListarProductos(false).AsEnumerable();

            var q = txtBuscar.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(q))
                data = data.Where(p => (p.Nombre ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0);

            int? catId = (cboCategoria.SelectedItem as ComboItem)?.Value;
            if (catId != null) data = data.Where(p => p.CategoriaId == catId.Value);

            int? subId = (cboSubcategoria.SelectedItem as ComboItem)?.Value;
            if (subId != null) data = data.Where(p => p.SubcategoriaId == subId.Value);

            bs.DataSource = data.ToList();
            grid.DataSource = bs;

            // Eliminar la columna "Activo" si existe, sin importar cómo se generó.
            var colActivo = grid.Columns
                .Cast<DataGridViewColumn>()
                .FirstOrDefault(c =>
                    string.Equals(c.HeaderText, "Activo", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.Name, "Activo", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.DataPropertyName, "Activo", StringComparison.OrdinalIgnoreCase));

            if (colActivo != null)
                grid.Columns.Remove(colActivo);
        }

        // ====== Columnas: menú + presets ======
        private void ConstruirMenuColumnas()
        {
            _buildingMenu = true;
            cmsColumnas.Items.Clear();

            var miRapida = new ToolStripMenuItem("Vista rápida") { Tag = "preset" };
            var miPredeterminada = new ToolStripMenuItem("Usar predeterminada") { Tag = "preset" };
            var miMostrarTodo = new ToolStripMenuItem("Mostrar todo") { Tag = "preset" };
            var miOcultarTodo = new ToolStripMenuItem("Ocultar todo") { Tag = "preset" };

            miRapida.Click += (s, e) => { AplicarVistaRapida(); GuardarPreferencias(); };
            miPredeterminada.Click += (s, e) => { AplicarVistaPredeterminada(); GuardarPreferencias(); };
            miMostrarTodo.Click += (s, e) => { SetAllColumnsVisible(true); GuardarPreferencias(); };
            miOcultarTodo.Click += (s, e) => { SetAllColumnsVisible(false); GuardarPreferencias(); };

            cmsColumnas.Items.Add(miRapida);
            cmsColumnas.Items.Add(miPredeterminada);
            cmsColumnas.Items.Add(miMostrarTodo);
            cmsColumnas.Items.Add(miOcultarTodo);
            cmsColumnas.Items.Add(new ToolStripSeparator());

            // Lista de columnas con check
            foreach (DataGridViewColumn col in grid.Columns.Cast<DataGridViewColumn>().OrderBy(c => c.DisplayIndex))
            {
                if (string.IsNullOrEmpty(col.Name)) continue;

                // Excluir explícitamente la columna "Activo" del menú de la ruedita.
                if (string.Equals(col.Name, "Activo", StringComparison.OrdinalIgnoreCase)) continue;

                var display = string.IsNullOrWhiteSpace(col.HeaderText) ? col.Name : col.HeaderText;

                var item = new ToolStripMenuItem(display)
                {
                    Checked = col.Visible,
                    CheckOnClick = true,
                    Tag = new ColTag { ColumnName = col.Name }
                };
                item.CheckedChanged += (s, e) =>
                {
                    if (_buildingMenu) return;
                    var it = (ToolStripMenuItem)s;
                    var tag = (ColTag)it.Tag!;
                    var column = grid.Columns[tag.ColumnName];
                    if (column != null) column.Visible = it.Checked;
                    GuardarPreferencias(); // persistir al vuelo
                };
                cmsColumnas.Items.Add(item);
            }

            _buildingMenu = false;
        }

        private void SetAllColumnsVisible(bool visible)
        {
            foreach (DataGridViewColumn col in grid.Columns)
                col.Visible = visible;

            // sincronizo checks si el menú está abierto
            foreach (ToolStripItem tsi in cmsColumnas.Items)
                if (tsi is ToolStripMenuItem mi && mi.Tag is ColTag)
                    mi.Checked = visible;
        }

        // Preset de operación diaria
        private void AplicarVistaRapida()
        {
            var visibles = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Nombre", "Precio", "Stock", "CategoriaNombre", "SubcategoriaNombre" };
            AplicarSetVisibles(visibles);
        }

        // Fallback si no hay prefs guardadas
        private void AplicarVistaPredeterminada()
        {
            var visibles = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Nombre", "Precio", "Stock", "CategoriaNombre", "SubcategoriaNombre" };
            AplicarSetVisibles(visibles);
        }

        private void AplicarSetVisibles(HashSet<string> visibles)
        {
            foreach (DataGridViewColumn col in grid.Columns)
                if (!string.IsNullOrEmpty(col.Name))
                    col.Visible = visibles.Contains(col.Name);

            // reflejo en menú (si está abierto)
            foreach (ToolStripItem tsi in cmsColumnas.Items)
                if (tsi is ToolStripMenuItem mi && mi.Tag is ColTag ct)
                    mi.Checked = visibles.Contains(ct.ColumnName);
        }

        // ====== Preferencias (leer/guardar) ======
        private void AplicarPreferenciasGuardadas()
        {
            try
            {
                if (!File.Exists(PrefsPath))
                {
                    AplicarVistaPredeterminada();
                    return;
                }

                var json = File.ReadAllText(PrefsPath);
                var pref = JsonSerializer.Deserialize<GridPrefs>(json);

                if (pref?.VisibleColumns == null || pref.VisibleColumns.Count == 0)
                {
                    AplicarVistaPredeterminada();
                    return;
                }

                var set = new HashSet<string>(pref.VisibleColumns, StringComparer.OrdinalIgnoreCase);
                foreach (DataGridViewColumn col in grid.Columns)
                    if (!string.IsNullOrEmpty(col.Name))
                        col.Visible = set.Contains(col.Name);
            }
            catch
            {
                AplicarVistaPredeterminada();
            }
        }

        private void GuardarPreferencias()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(PrefsPath)!);

                var visibles = grid.Columns
                    .Cast<DataGridViewColumn>()
                    .Where(c => c.Visible && !string.IsNullOrEmpty(c.Name))
                    .Select(c => c.Name)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var pref = new GridPrefs { VisibleColumns = visibles };
                var json = JsonSerializer.Serialize(pref, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(PrefsPath, json);
            }
            catch
            {
                // ignorar errores de escritura
            }
        }

        // ====== Helpers ======
        private static string GetUserKey()
        {
            // Si el usuario viniera nulo/vacío, caigo al del sistema
            var user = !string.IsNullOrWhiteSpace(Sesion.Usuario)
                ? Sesion.Usuario
                : Environment.UserName;

            // Rol es enum no-nullable
            var rol = Sesion.Rol.ToString();

            return $"{user}|{rol}";
        }

        private static string? Safe(Func<string?> getter)
        {
            try { return getter(); } catch { return null; }
        }

        // Para generar un nombre de archivo válido
        private static string Sanitize(string s)
        {
            foreach (var ch in Path.GetInvalidFileNameChars())
                s = s.Replace(ch, '_');
            return s;
        }

        private class ComboItem
        {
            public string Text { get; set; } = "";
            public int? Value { get; set; }
            public override string ToString() => Text;
        }

        private class ColTag
        {
            public string ColumnName { get; set; } = "";
        }

        private class GridPrefs
        {
            public List<string> VisibleColumns { get; set; } = new();
        }
    }
}
