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
        //  Grilla + filtros 
        private readonly DataGridView grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
        private readonly TextBox txtBuscar = new TextBox { PlaceholderText = "Buscar por nombre...", Width = 220 };
        private readonly ComboBox cboCategoria = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200 };
        private readonly ComboBox cboSubcategoria = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200 };
        private readonly CheckBox chkInactivos = new CheckBox { Text = "Ver inactivos" };
        private readonly BindingSource bs = new BindingSource();

        //  Selector de columnas 
        private readonly Button btnColumnas = new Button { Text = "⚙ Columnas ▾" };
        private readonly ContextMenuStrip cmsColumnas = new ContextMenuStrip();
        private bool _buildingMenu = false;   // evita reentradas al sincronizar checks

        //  Persistencia de preferencias 
        private readonly string prefsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TeoAccesorios", "grid_productos_cols.json");

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
            top.Controls.Add(chkInactivos);
            var btnFiltrar = new Button { Text = "Filtrar" };
            top.Controls.Add(btnFiltrar);

            // Botón de columnas (context menu)
            btnColumnas.Click += (s, e) =>
            {
                ConstruirMenuColumnas();
                cmsColumnas.Show(btnColumnas, new System.Drawing.Point(0, btnColumnas.Height));
            };
            top.Controls.Add(btnColumnas);

            // ABM solo Admin
            if (Sesion.Rol == RolUsuario.Admin)
            {
                var btnNuevo = new Button { Text = "Nuevo" };
                var btnEditar = new Button { Text = "Editar" };
                var btnEliminar = new Button { Text = "Eliminar" };
                var btnRestaurar = new Button { Text = "Restaurar" };
                top.Controls.AddRange(new Control[] { btnNuevo, btnEditar, btnEliminar, btnRestaurar });

                btnNuevo.Click += (s, e) =>
                {
                    var pr = new Producto();
                    using var f = new ProductoEditForm(pr);
                    if (f.ShowDialog(this) == DialogResult.OK)
                    {
                        pr.Id = Repository.InsertarProducto(pr);
                        LoadData();
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
                            Repository.ActualizarProducto(tmp);
                            LoadData();
                        }
                    }
                };

                btnEliminar.Click += (s, e) =>
                {
                    if (grid.CurrentRow?.DataBoundItem is Producto sel)
                    {
                        Repository.EliminarProducto(sel.Id);
                        LoadData();
                    }
                };

                btnRestaurar.Click += (s, e) =>
                {
                    if (grid.CurrentRow?.DataBoundItem is Producto sel)
                    {
                        sel.Activo = true;
                        Repository.ActualizarProducto(sel);
                        LoadData();
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

            // Estilo grilla
            GridHelper.Estilizar(grid);
            GridHelperLock.SoloLectura(grid);
            GridHelperLock.WireDataBindingLock(grid);

            // Filtros
            btnFiltrar.Click += (s, e) => LoadData();
            chkInactivos.CheckedChanged += (s, e) => LoadData();
            txtBuscar.TextChanged += (s, e) => LoadData();
            cboCategoria.SelectedIndexChanged += (s, e) => { CargarSubcategorias(); LoadData(); };
            cboSubcategoria.SelectedIndexChanged += (s, e) => LoadData();

            
            grid.DataBindingComplete += (s, e) =>
            {
                AplicarPreferenciasGuardadas(); 
              
                ConstruirMenuColumnas();
            };

            // Cargar combos
            cboCategoria.Items.Clear();
            cboCategoria.Items.Add(new ComboItem { Text = "Todas las categorías", Value = null });
            foreach (var c in Repository.ListarCategorias())
                cboCategoria.Items.Add(new ComboItem { Text = $"{c.Id} - {c.Nombre}", Value = c.Id });
            cboCategoria.SelectedIndex = 0;

            CargarSubcategorias();
            LoadData();

            // Guardar prefs al cerrar
            FormClosing += (s, e) => GuardarPreferencias();
        }

        //  Subcargas 
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
            var data = Repository.ListarProductos(chkInactivos.Checked).AsEnumerable();

            var q = txtBuscar.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(q))
                data = data.Where(p => (p.Nombre ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0);

            int? catId = (cboCategoria.SelectedItem as ComboItem)?.Value;
            if (catId != null) data = data.Where(p => p.CategoriaId == catId.Value);

            int? subId = (cboSubcategoria.SelectedItem as ComboItem)?.Value;
            if (subId != null) data = data.Where(p => p.SubcategoriaId == subId.Value);

            bs.DataSource = data.ToList();
            grid.DataSource = bs;
        }

        //  Selector de columnas
        private void ConstruirMenuColumnas()
        {
            _buildingMenu = true;
            cmsColumnas.Items.Clear();

            // Presets
            var miRapida = new ToolStripMenuItem("Vista rápida") { Tag = "preset" };
            var miPredeterminada = new ToolStripMenuItem("Usar predeterminada") { Tag = "preset" };
            var miMostrarTodo = new ToolStripMenuItem("Mostrar todo") { Tag = "preset" };
            var miOcultarTodo = new ToolStripMenuItem("Ocultar todo") { Tag = "preset" };
            miRapida.Click += (s, e) => AplicarVistaRapida();
            miPredeterminada.Click += (s, e) => AplicarVistaPredeterminada();
            miMostrarTodo.Click += (s, e) => SetAllColumnsVisible(true);
            miOcultarTodo.Click += (s, e) => SetAllColumnsVisible(false);

            cmsColumnas.Items.Add(miRapida);
            cmsColumnas.Items.Add(miPredeterminada);
            cmsColumnas.Items.Add(miMostrarTodo);
            cmsColumnas.Items.Add(miOcultarTodo);
            cmsColumnas.Items.Add(new ToolStripSeparator());

            // Lista de columnas con check
            foreach (DataGridViewColumn col in grid.Columns.Cast<DataGridViewColumn>().OrderBy(c => c.DisplayIndex))
            {
                if (string.IsNullOrEmpty(col.Name)) continue;
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
                };
                cmsColumnas.Items.Add(item);
            }

            _buildingMenu = false;
        }

        private void SetAllColumnsVisible(bool visible)
        {
            foreach (DataGridViewColumn col in grid.Columns)
                col.Visible = visible;

            // Sincronizo menú si está abierto
            foreach (ToolStripItem tsi in cmsColumnas.Items)
            {
                if (tsi is ToolStripMenuItem mi && mi.Tag is ColTag)
                    mi.Checked = visible;
            }
        }

        // Preset operativa diaria
        private void AplicarVistaRapida()
        {
            var visibles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Nombre", "Precio", "Stock", "Activo", "CategoriaNombre"
            };
            AplicarSetVisibles(visibles);
        }

        //  Predeterminada 
        private void AplicarVistaPredeterminada()
        {
            var visibles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Nombre", "Precio", "Stock", "CategoriaNombre"
            };
            AplicarSetVisibles(visibles);
        }

        private void AplicarSetVisibles(HashSet<string> visibles)
        {
            foreach (DataGridViewColumn col in grid.Columns)
                col.Visible = visibles.Contains(col.Name);

            // Reflejo en el menú si está abierto
            foreach (ToolStripItem tsi in cmsColumnas.Items)
            {
                if (tsi is ToolStripMenuItem mi && mi.Tag is ColTag ct)
                    mi.Checked = visibles.Contains(ct.ColumnName);
            }
        }

      
        private void AplicarPreferenciasGuardadas()
        {
            try
            {
                if (!File.Exists(prefsPath))
                {
                    // Primera vez → aplicar predeterminada
                    AplicarVistaPredeterminada();
                    return;
                }

                var json = File.ReadAllText(prefsPath);
                var pref = JsonSerializer.Deserialize<GridPrefs>(json);

                if (pref?.VisibleColumns == null || pref.VisibleColumns.Count == 0)
                {
                    AplicarVistaPredeterminada();
                    return;
                }

                var set = new HashSet<string>(pref.VisibleColumns, StringComparer.OrdinalIgnoreCase);
                foreach (DataGridViewColumn col in grid.Columns)
                    col.Visible = set.Contains(col.Name);
            }
            catch
            {
                // Ante error, uso predeterminada
                AplicarVistaPredeterminada();
            }
        }

        private void GuardarPreferencias()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(prefsPath)!);
                var visibles = grid.Columns
                    .Cast<DataGridViewColumn>()
                    .Where(c => c.Visible && !string.IsNullOrEmpty(c.Name))
                    .Select(c => c.Name)
                    .ToList();

                var pref = new GridPrefs { VisibleColumns = visibles };
                var json = JsonSerializer.Serialize(pref, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(prefsPath, json);
            }
            catch
            {
                // ignorar errores de escritura
            }
        }

        //  Helpers 
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
