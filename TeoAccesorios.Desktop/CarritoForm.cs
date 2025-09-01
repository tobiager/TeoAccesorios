using System.Linq;
using System.Windows.Forms;

namespace TeoAccesorios.Desktop
{
    public class CarritoForm : Form
    {
        public CarritoForm()
        {
            Text = "Carrito / Checkout (Demo)";
            Width = 800; Height = 520; StartPosition = FormStartPosition.CenterParent;

            var pedido = MockData.Pedidos.First();
            var grid = new DataGridView{ Dock=DockStyle.Fill, ReadOnly=true, AutoGenerateColumns=true, DataSource = pedido.Items };
            var panel = new FlowLayoutPanel{ Dock=DockStyle.Bottom, Height=60, FlowDirection=FlowDirection.RightToLeft, Padding = new Padding(8) };
            var btnPagar = new Button{ Text="Confirmar (demo)" };
            var btnVolver = new Button{ Text="Volver" };
            btnVolver.Click += (_,__) => Close();
            btnPagar.Click += (_,__) => MessageBox.Show("¡Gracias por tu compra! (demo)", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            panel.Controls.Add(btnPagar);
            panel.Controls.Add(btnVolver);
            var total = new Label{ Text = $"Total: $ {pedido.Total:N0}", Dock=DockStyle.Top, Height=28, TextAlign=System.Drawing.ContentAlignment.MiddleRight };

            Controls.Add(grid);
            Controls.Add(panel);
            Controls.Add(total);
        }
    }
}
