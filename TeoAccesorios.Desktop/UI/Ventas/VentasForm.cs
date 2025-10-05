using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class VentasForm : Form
    {
        private readonly DataGridView grid = new()
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AutoGenerateColumns = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false
        };

        private readonly CheckBox chkAnuladas = new() { Text = "Ver anuladas", AutoSize = true };
        private readonly CheckBox chkRango = new() { Text = "Rango fechas", AutoSize = true };
        private readonly DateTimePicker dpDesde = new() { Width = 130, Value = DateTime.Today.AddDays(-7) };
        private readonly DateTimePicker dpHasta = new() { Width = 130, Value = DateTime.Today };

        private readonly ComboBox cboVendedor = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
        private readonly ComboBox cboCliente = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };

        private readonly TextBox txtBuscar = new() { Width = 220, PlaceholderText = "Buscar (Id, cliente, vendedor...)" };

        private readonly Button btnNueva = new() { Text = "Nueva" };
        private readonly Button btnAnular = new() { Text = "Anular" };
        private readonly Button btnRestaurar = new() { Text = "Restaurar" };
        private readonly Button btnLimpiar = new() { Text = "Limpiar filtros" };

        // ⚙ Selector de columnas
        private readonly Button btnColumnas = new() { Text = "⚙", Width = 32, Height = 26, Padding = new Padding(0) };
        private readonly ContextMenuStrip cmsColumnas = new();
        private bool _buildingMenu = false;

        // ===== Preferencias por USUARIO + VISTA =====
        private const string VIEW_KEY = "Ventas";
        private string PrefsPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TeoAccesorios",
            $"grid_{VIEW_KEY}_{Sanitize(GetUserKey())}.json"
        );

        private List<Models.Venta> _ventasSource = new();
        private List<Models.Venta> _ventasFiltradas = new();
        private bool _colsConfigured = false;

        public VentasForm()
        {
            Text = "Ver Ventas";
            Width = 1050;
            Height = 650;
            StartPosition = FormStartPosition.CenterParent;

            var actions = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42, Padding = new Padding(8) };
            actions.Controls.AddRange(new Control[] { btnAnular, btnRestaurar, chkAnuladas, btnNueva });

            var filtros = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 56, Padding = new Padding(8) };
            filtros.Controls.Add(new Label { Text = "Vendedor:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
            filtros.Controls.Add(cboVendedor);
            filtros.Controls.Add(new Label { Text = "Cliente:", AutoSize = true, Padding = new Padding(10, 8, 0, 0) });
            filtros.Controls.Add(cboCliente);
            filtros.Controls.Add(chkRango);
            filtros.Controls.Add(new Label { Text = "Desde:", AutoSize = true, Padding = new Padding(10, 8, 0, 0) });
            filtros.Controls.Add(dpDesde);
            filtros.Controls.Add(new Label { Text = "Hasta:", AutoSize = true, Padding = new Padding(6, 8, 0, 0) });
            filtros.Controls.Add(dpHasta);
            filtros.Controls.Add(new Label { Text = "Buscar:", AutoSize = true, Padding = new Padding(10, 8, 0, 0) });
            filtros.Controls.Add(txtBuscar);
            filtros.Controls.Add(btnLimpiar);
            filtros.Controls.Add(btnColumnas); 

            Controls.Add(grid);
            Controls.Add(filtros);
            Controls.Add(actions);

            // Estilo grilla (si tenés estos helpers)
            GridHelper.Estilizar(grid);
            GridHelperLock.SoloLectura(grid);
            GridHelperLock.WireDataBindingLock(grid);

            // Acciones
            btnNueva.Click += (_, __) =>
            {
                using var f = new NuevaVentaForm();
                if (f.ShowDialog(this) == DialogResult.OK) LoadData();
            };

            btnAnular.Click += (_, __) =>
            {
                if (!TryGetSelectedVenta(out var v)) return;
                if (Sesion.Rol == RolUsuario.Vendedor &&
                    (!string.Equals(v.Vendedor, Sesion.Usuario, StringComparison.OrdinalIgnoreCase)
                     || v.FechaVenta.Date != DateTime.Today))
                {
                    MessageBox.Show("Sólo podés anular ventas tuyas del día.", "Permiso");
                    return;
                }
                Repository.SetVentaAnulada(v.Id, true);
                LoadData();
            };

            btnRestaurar.Click += (_, __) =>
            {
                if (!TryGetSelectedVenta(out var v)) return;
                if (Sesion.Rol == RolUsuario.Vendedor &&
                    (!string.Equals(v.Vendedor, Sesion.Usuario, StringComparison.OrdinalIgnoreCase)
                     || v.FechaVenta.Date != DateTime.Today))
                {
                    MessageBox.Show("Sólo podés restaurar ventas tuyas del día.", "Permiso");
                    return;
                }
                Repository.SetVentaAnulada(v.Id, false);
                LoadData();
            };

            chkAnuladas.CheckedChanged += (_, __) => LoadData();
            chkRango.CheckedChanged += (_, __) => ApplyFilters();
            dpDesde.ValueChanged += (_, __) => ApplyFilters();
            dpHasta.ValueChanged += (_, __) => ApplyFilters();
            cboVendedor.SelectedIndexChanged += (_, __) => ApplyFilters();
            cboCliente.SelectedIndexChanged += (_, __) => ApplyFilters();
            txtBuscar.TextChanged += (_, __) => ApplyFilters();

            btnLimpiar.Click += (_, __) =>
            {
                cboVendedor.SelectedIndex = 0;
                cboCliente.SelectedIndex = 0;
                chkRango.Checked = false;
                dpDesde.Value = DateTime.Today.AddDays(-7);
                dpHasta.Value = DateTime.Today;
                txtBuscar.Clear();
            };

            grid.CellDoubleClick += (_, e) =>
            {
                if (e.RowIndex < 0) return;
                if (!TryGetVentaFromRow(grid.Rows[e.RowIndex], out var v)) return;
                var cliente = FindClienteById(v.ClienteId);
                using var f = new VentaDetalleForm(v, cliente);
                f.ShowDialog(this);
            };

            grid.DataBindingComplete += (_, __) =>
            {
                if (!_colsConfigured)
                {
                    ConfigureColumns();
                    _colsConfigured = true;
                }
                AplicarPreferenciasGuardadas();
                ConstruirMenuColumnas();
            };

            btnColumnas.Click += (s, e) =>
            {
                ConstruirMenuColumnas();
                cmsColumnas.Show(btnColumnas, new System.Drawing.Point(0, btnColumnas.Height));
            };

            FormClosing += (s, e) => GuardarPreferencias();

            LoadCombos();
            LoadData();
        }

        // ===== Datos y filtros =====
        private void LoadCombos()
        {
            var vendedores = Repository.ListarUsuarios()
                .Where(u => u.Activo)
                .Select(u => u.NombreUsuario)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s)
                .ToList();

            cboVendedor.Items.Clear();
            cboVendedor.Items.Add("Todos");
            foreach (var v in vendedores) cboVendedor.Items.Add(v);
            cboVendedor.SelectedIndex = 0;

            var clientes = Repository.Clientes
                .Select(c => c.Nombre)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s)
                .ToList();

            cboCliente.Items.Clear();
            cboCliente.Items.Add("Todos");
            foreach (var c in clientes) cboCliente.Items.Add(c);
            cboCliente.SelectedIndex = 0;
        }

        private void LoadData()
        {
            _ventasSource = Repository.ListarVentas(chkAnuladas.Checked) ?? new List<Models.Venta>();

            if (Sesion.Rol == RolUsuario.Vendedor)
            {
                _ventasSource = _ventasSource
                    .Where(v => string.Equals(v.Vendedor, Sesion.Usuario, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            ApplyFilters();
            _colsConfigured = false;
        }

        private void ApplyFilters()
        {
            IEnumerable<Models.Venta> q = _ventasSource;

            if (chkRango.Checked)
            {
                var desde = dpDesde.Value.Date;
                var hasta = dpHasta.Value.Date.AddDays(1);
                q = q.Where(v => v.FechaVenta >= desde && v.FechaVenta < hasta);
            }

            if (cboVendedor.SelectedIndex > 0)
            {
                var vend = cboVendedor.SelectedItem!.ToString()!;
                q = q.Where(v => string.Equals(v.Vendedor, vend, StringComparison.OrdinalIgnoreCase));
            }

            if (cboCliente.SelectedIndex > 0)
            {
                var cli = cboCliente.SelectedItem!.ToString()!;
                q = q.Where(v => string.Equals(v.ClienteNombre, cli, StringComparison.OrdinalIgnoreCase));
            }

            var term = txtBuscar.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(term))
            {
                term = term.ToLowerInvariant();
                q = q.Where(v =>
                    v.Id.ToString().Contains(term) ||
                    (v.ClienteNombre ?? "").ToLowerInvariant().Contains(term) ||
                    (v.Vendedor ?? "").ToLowerInvariant().Contains(term) ||
                    (v.Canal ?? "").ToLowerInvariant().Contains(term) ||
                    (v.DireccionEnvio ?? "").ToLowerInvariant().Contains(term));
            }

            _ventasFiltradas = q.OrderByDescending(v => v.FechaVenta).ToList();

            grid.DataSource = _ventasFiltradas.Select(v => new
            {
                v.Id,
                Fecha = v.FechaVenta.ToString("dd/MM/yyyy HH:mm"),
                v.Vendedor,
                v.Canal,
                v.ClienteId,
                ClienteNombre = v.ClienteNombre,
                DireccionEnvio = v.DireccionEnvio,
                v.Anulada,
                Total = v.Total.ToString("N0")
            }).ToList();
        }

        // ===== Cols y ruedita =====
        private void ConfigureColumns()
        {
            DataGridViewColumn FindCol(string key) =>
                grid.Columns
                    .Cast<DataGridViewColumn>()
                    .FirstOrDefault(c =>
                        string.Equals(c.DataPropertyName, key, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(c.Name, key, StringComparison.OrdinalIgnoreCase));

            try
            {
                var cId = FindCol("Id"); if (cId != null) cId.Width = 60;
                var cFecha = FindCol("Fecha"); if (cFecha != null) cFecha.Width = 140;
                var cCli = FindCol("ClienteNombre"); if (cCli != null) cCli.HeaderText = "Cliente";
                var cDir = FindCol("DireccionEnvio"); if (cDir != null) cDir.HeaderText = "Dirección envío";
            }
            catch { /* cosmético */ }
        }

        private void ConstruirMenuColumnas()
        {
            _buildingMenu = true;
            cmsColumnas.Items.Clear();

            // Presets
            var miRapida = new ToolStripMenuItem("Vista rápida") { Tag = "preset" };
            var miPredet = new ToolStripMenuItem("Usar predeterminada") { Tag = "preset" };
            var miMostrarTodo = new ToolStripMenuItem("Mostrar todo") { Tag = "preset" };
            var miOcultarTodo = new ToolStripMenuItem("Ocultar todo") { Tag = "preset" };

            miRapida.Click += (s, e) => { AplicarVistaRapida(); GuardarPreferencias(); };
            miPredet.Click += (s, e) => { AplicarVistaPredeterminada(); GuardarPreferencias(); };
            miMostrarTodo.Click += (s, e) => { SetAllColumnsVisible(true); GuardarPreferencias(); };
            miOcultarTodo.Click += (s, e) => { SetAllColumnsVisible(false); GuardarPreferencias(); };

            cmsColumnas.Items.Add(miRapida);
            cmsColumnas.Items.Add(miPredet);
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
                    Tag = col.Name
                };

                item.CheckedChanged += (s, e) =>
                {
                    if (_buildingMenu) return;
                    var it = (ToolStripMenuItem)s;
                    var column = grid.Columns[(string)it.Tag];
                    if (column != null) column.Visible = it.Checked;
                    GuardarPreferencias();
                };

                cmsColumnas.Items.Add(item);
            }

            _buildingMenu = false;
        }

        private void SetAllColumnsVisible(bool visible)
        {
            foreach (DataGridViewColumn col in grid.Columns)
                col.Visible = visible;

            foreach (ToolStripItem tsi in cmsColumnas.Items)
                if (tsi is ToolStripMenuItem mi && mi.Tag is string)
                    mi.Checked = visible;
        }

        // Preset rápida
        private void AplicarVistaRapida()
        {
            var visibles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "Id", "Fecha", "ClienteNombre", "Total", "Anulada" };
            AplicarSetVisibles(visibles);
        }

        // Predeterminada (fallback)
        private void AplicarVistaPredeterminada()
        {
            var visibles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "Fecha", "Vendedor", "Canal", "ClienteNombre", "DireccionEnvio", "Total" };
            AplicarSetVisibles(visibles);
        }

        private void AplicarSetVisibles(HashSet<string> visibles)
        {
            foreach (DataGridViewColumn col in grid.Columns)
                col.Visible = visibles.Contains(col.Name);

            foreach (ToolStripItem tsi in cmsColumnas.Items)
                if (tsi is ToolStripMenuItem mi && mi.Tag is string name)
                    mi.Checked = visibles.Contains(name);
        }

        // ===== Persistencia columnas visibles =====
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
            catch { /* ignorar */ }
        }

        private class GridPrefs
        {
            public List<string> VisibleColumns { get; set; } = new();
        }

        // ===== Ventas helpers =====
        private bool TryGetSelectedVenta(out Models.Venta venta)
        {
            venta = null!;
            var row = grid.CurrentRow;
            if (row == null) return false;
            return TryGetVentaFromRow(row, out venta);
        }

        private bool TryGetVentaFromRow(DataGridViewRow row, out Models.Venta venta)
        {
            venta = null!;
            var col = grid.Columns.Cast<DataGridViewColumn>()
                .FirstOrDefault(c =>
                    string.Equals(c.DataPropertyName, "Id", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.Name, "Id", StringComparison.OrdinalIgnoreCase));
            if (col == null) return false;

            var val = row.Cells[col.Index].Value;
            if (val == null) return false;
            if (!int.TryParse(val.ToString(), out int id)) return false;

            venta = _ventasFiltradas.FirstOrDefault(v => v.Id == id)!;
            return venta != null;
        }

        private Cliente? FindClienteById(int clienteId)
        {
            try
            {
                var m = typeof(Repository).GetMethod("GetClienteById");
                if (m != null) return (Cliente?)m.Invoke(null, new object[] { clienteId });
            }
            catch { /* fallback */ }

            return Repository.Clientes?.FirstOrDefault(c => c.Id == clienteId);
        }

        // ===== Helpers generales =====
        private static string GetUserKey()
        {
            var user = !string.IsNullOrWhiteSpace(Sesion.Usuario) ? Sesion.Usuario : Environment.UserName;
            var rol = Sesion.Rol.ToString(); // enum no-nullable
            return $"{user}|{rol}";
        }

        private static string Sanitize(string s)
        {
            foreach (var ch in Path.GetInvalidFileNameChars())
                s = s.Replace(ch, '_');
            return s;
        }
    }
}
