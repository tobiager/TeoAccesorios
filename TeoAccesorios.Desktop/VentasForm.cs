using System;
using System.Linq;
using System.Windows.Forms;

namespace TeoAccesorios.Desktop
{
    public class VentasForm : Form
    {
        private DataGridView grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
        private CheckBox chkAnuladas = new CheckBox { Text = "Ver anuladas" };

        public VentasForm()
        {
            Text = "Ver Ventas";
            Width = 900;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(8), AutoSize = false };
            var btnNueva = new Button { Text = "Nueva" };
            var btnAnular = new Button { Text = "Anular" };
            var btnRestaurar = new Button { Text = "Restaurar" };

            top.Controls.Add(btnAnular);
            top.Controls.Add(btnRestaurar);
            top.Controls.Add(chkAnuladas);
            top.Controls.Add(btnNueva);

            Controls.Add(grid);
            Controls.Add(top);

            btnNueva.Click += (s, e) =>
            {
                using (var f = new NuevaVentaForm())
                {
                    if (f.ShowDialog(this) == DialogResult.OK) LoadData();
                }
            };

            btnAnular.Click += (s, e) =>
            {
                if (grid.CurrentRow != null && grid.CurrentRow.DataBoundItem is Models.Venta v)
                {
                    if (Sesion.Rol == RolUsuario.Vendedor &&
                        (!string.Equals(v.Vendedor, Sesion.Usuario, StringComparison.OrdinalIgnoreCase) || v.Fecha.Date != DateTime.Today))
                    {
                        MessageBox.Show("Sólo podés anular ventas tuyas del día.", "Permiso");
                        return;
                    }
                    v.Anulada = true;
                    LoadData();
                }
            };

            btnRestaurar.Click += (s, e) =>
            {
                if (grid.CurrentRow != null && grid.CurrentRow.DataBoundItem is Models.Venta v)
                {
                    if (Sesion.Rol == RolUsuario.Vendedor &&
                        (!string.Equals(v.Vendedor, Sesion.Usuario, StringComparison.OrdinalIgnoreCase) || v.Fecha.Date != DateTime.Today))
                    {
                        MessageBox.Show("Sólo podés restaurar ventas tuyas del día.", "Permiso");
                        return;
                    }
                    v.Anulada = false;
                    LoadData();
                }
            };

            chkAnuladas.CheckedChanged += (s, e) => LoadData();

            grid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && grid.Rows[e.RowIndex].DataBoundItem is Models.Venta v)
                {
                    new VentaDetalleForm(v).ShowDialog(this);
                }
            };

            LoadData();
        }

        private void LoadData()
        {
            var data = MockData.Ventas
                .Where(v => chkAnuladas.Checked ? true : !v.Anulada)
                .OrderByDescending(v => v.Fecha)
                .ToList();

            if (Sesion.Rol == RolUsuario.Vendedor)
                data = data.Where(v => string.Equals(v.Vendedor, Sesion.Usuario, StringComparison.OrdinalIgnoreCase)).ToList();

            grid.DataSource = data.Select(v => new
            {
                v.Id,
                Fecha = v.Fecha.ToString("dd/MM/yyyy HH:mm"),
                v.Vendedor,
                Canal = v.Canal,
                v.ClienteId,
                v.ClienteNombre,
                DireccionEnvio = v.DireccionEnvio,
                v.Anulada,
                Total = v.Total.ToString("N0")
            }).ToList();
        }
    }
}
