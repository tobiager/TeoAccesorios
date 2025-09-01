using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class PedidoDetalleForm : Form
    {
        public PedidoDetalleForm(Pedido pedido)
        {
            Text = $"Pedido #{pedido.Id}";
            Width = 700; Height = 500; StartPosition = FormStartPosition.CenterParent;

            var lbl = new Label{ Text = $"Cliente: {pedido.ClienteNombre} — Fecha: {pedido.Fecha:dd/MM/yyyy} — Estado: {pedido.Estado}", Dock=DockStyle.Top, Height=40 };
            var grid = new DataGridView{ Dock=DockStyle.Fill, ReadOnly=true, AutoGenerateColumns=true, DataSource = pedido.Items };
            var total = new Label{ Text = $"Total: $ {pedido.Total:N0}", Dock=DockStyle.Bottom, Height=32, TextAlign=System.Drawing.ContentAlignment.MiddleRight };
            Controls.Add(grid);
            Controls.Add(total);
            Controls.Add(lbl);
        }
    }
}
