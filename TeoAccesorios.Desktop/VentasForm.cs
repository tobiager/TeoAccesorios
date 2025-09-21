using System;
using System.Collections.Generic;
using System.Linq;
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

            Controls.Add(grid);
            Controls.Add(filtros);
            Controls.Add(actions);

            GridHelper.Estilizar(grid);
            GridHelperLock.SoloLectura(grid);
            GridHelperLock.WireDataBindingLock(grid);

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
                if (_colsConfigured) return;
                ConfigureColumns();
                _colsConfigured = true;
            };

            LoadCombos();
            LoadData();
        }

        // Datos 
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

        // UI helpers
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

        //  Helper chico para conseguir el cliente por Id
        private Cliente? FindClienteById(int clienteId)
        {
            
            try
            {
               
                
                var m = typeof(Repository).GetMethod("GetClienteById");
                if (m != null) return (Cliente?)m.Invoke(null, new object[] { clienteId });
            }
            catch { /* fallback abajo */ }

           
            return Repository.Clientes?.FirstOrDefault(c => c.Id == clienteId);
        }
    }
}
