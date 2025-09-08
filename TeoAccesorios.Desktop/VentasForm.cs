using System;
using System.Linq;
using System.Windows.Forms;

namespace TeoAccesorios.Desktop
{
    public class VentasForm : Form
    {
        private readonly DataGridView grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
        private readonly CheckBox chkAnuladas = new() { Text = "Ver anuladas" };

        public VentasForm()
        {
            Text = "Ver Ventas";
            Width = 900;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(8) };
            var btnNueva = new Button { Text = "Nueva" };
            var btnAnular = new Button { Text = "Anular" };
            var btnRestaurar = new Button { Text = "Restaurar" };

            top.Controls.AddRange(new Control[] { btnAnular, btnRestaurar, chkAnuladas, btnNueva });

            Controls.Add(grid);
            Controls.Add(top);

            btnNueva.Click += (_, __) =>
            {
                using var f = new NuevaVentaForm();
                if (f.ShowDialog(this) == DialogResult.OK) LoadData();
            };

            btnAnular.Click += (_, __) =>
            {
                if (grid.CurrentRow?.DataBoundItem is Models.Venta v)
                {
                    // Solo vendedor puede anular sus ventas del día
                    if (Sesion.Rol == RolUsuario.Vendedor &&
                        (!string.Equals(v.Vendedor, Sesion.Usuario, StringComparison.OrdinalIgnoreCase)
                         || v.FechaVenta.Date != DateTime.Today))
                    {
                        MessageBox.Show("Sólo podés anular ventas tuyas del día.", "Permiso");
                        return;
                    }

                    Repository.SetVentaAnulada(v.Id, true);
                    LoadData();
                }
            };

            btnRestaurar.Click += (_, __) =>
            {
                if (grid.CurrentRow?.DataBoundItem is Models.Venta v)
                {
                    if (Sesion.Rol == RolUsuario.Vendedor &&
                        (!string.Equals(v.Vendedor, Sesion.Usuario, StringComparison.OrdinalIgnoreCase)
                         || v.FechaVenta.Date != DateTime.Today))
                    {
                        MessageBox.Show("Sólo podés restaurar ventas tuyas del día.", "Permiso");
                        return;
                    }

                    Repository.SetVentaAnulada(v.Id, false);
                    LoadData();
                }
            };

            chkAnuladas.CheckedChanged += (_, __) => LoadData();

            grid.CellDoubleClick += (_, e) =>
            {
                if (e.RowIndex >= 0 && grid.Rows[e.RowIndex].DataBoundItem is Models.Venta v)
                    new VentaDetalleForm(v).ShowDialog(this);
            };

            LoadData();
        }

        private void LoadData()
        {
            // Cargar desde BD (con o sin anuladas)
            var data = Repository.ListarVentas(chkAnuladas.Checked);

            // Si el rol es vendedor, filtra por vendedor actual
            if (Sesion.Rol == RolUsuario.Vendedor)
            {
                data = data
                    .Where(v => string.Equals(v.Vendedor, Sesion.Usuario, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Proyección a la grilla
            grid.DataSource = data.Select(v => new
            {
                v.Id,
                Fecha = v.FechaVenta.ToString("dd/MM/yyyy HH:mm"),
                v.Vendedor,
                v.Canal,
                v.ClienteId,
                v.ClienteNombre,
                DireccionEnvio = v.DireccionEnvio,
                v.Anulada,
                Total = v.Total.ToString("N0")
            }).ToList();
        }
    }
}
