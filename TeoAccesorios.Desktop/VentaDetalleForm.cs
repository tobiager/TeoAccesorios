using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class VentaDetalleForm : Form
    {
        public VentaDetalleForm(Venta v)
        {
            Text = $"Venta #{v.Id}";
            Width = 720; Height = 520; StartPosition = FormStartPosition.CenterParent;

            var info = new Label{ Text = $"Fecha: {v.Fecha:dd/MM/yyyy HH:mm}  —  Cliente: {v.ClienteNombre}  —  Vendedor: {v.Vendedor}\nEnvío: {v.DireccionEnvio}", Dock = DockStyle.Top, Height=50 };
            var grid = new DataGridView{ Dock=DockStyle.Fill, ReadOnly=true, AutoGenerateColumns=true, DataSource=v.Detalles };
            var total = new Label{ Text = $"Total: $ {v.Total:N0}", Dock = DockStyle.Bottom, Height=32, TextAlign=System.Drawing.ContentAlignment.MiddleRight };

            Controls.Add(grid);
            Controls.Add(total);
            Controls.Add(info);
            
            GridHelper.Estilizar(grid);
            GridHelperLock.WireDataBindingLock(grid);

        }
    }
}
