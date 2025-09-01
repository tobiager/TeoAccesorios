using System.Linq;
using System.Windows.Forms;

namespace TeoAccesorios.Desktop
{
    public class PedidosForm : Form
    {
        private DataGridView grid = new DataGridView{ Dock=DockStyle.Fill, ReadOnly=true, AutoGenerateColumns=true };
        public PedidosForm()
        {
            Text = "Pedidos";
            Width = 900; Height = 500; StartPosition = FormStartPosition.CenterParent;
            Controls.Add(grid);
            LoadData();
            grid.CellDoubleClick += (s,e)=>{
                if(e.RowIndex >= 0){
                    var pedido = grid.Rows[e.RowIndex].DataBoundItem as Models.Pedido;
                    if(pedido!=null) new PedidoDetalleForm(pedido).ShowDialog(this);
                }
            };
        }
        private void LoadData()
        {
            var data = MockData.Pedidos.OrderByDescending(p => p.Fecha).ToList();
            grid.DataSource = data;
        }
    }
}
