using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;

namespace TeoAccesorios.Desktop
{
    public class KPIsView : UserControl
    {
        public KPIsView()
        {
            var main = new TableLayoutPanel{ Dock=DockStyle.Fill, ColumnCount=1, RowCount=3, Padding=new Padding(12) };
            main.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
            main.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            main.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            var stats = new TableLayoutPanel{ Dock=DockStyle.Fill, ColumnCount=4 };
            for(int i=0;i<4;i++) stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            Control Card(string label, string value){
                var panel = new Panel{ Dock=DockStyle.Fill, BackColor = Color.FromArgb(18,22,35), Padding = new Padding(12) };
                var l1 = new Label{ Text=label, ForeColor=Color.Gainsboro, Dock=DockStyle.Top, Height=24 };
                var l2 = new Label{ Text=value, ForeColor=Color.White, Dock=DockStyle.Fill, Font=new Font("Segoe UI", 20, FontStyle.Bold), TextAlign=ContentAlignment.MiddleLeft };
                panel.Controls.Add(l2); panel.Controls.Add(l1);
                return panel;
            }
            var ventasHoy = MockData.Ventas.Where(v => v.Fecha.Date == DateTime.Today && !v.Anulada).ToList();
            var ingresosHoy = ventasHoy.Sum(v => v.Total);
            stats.Controls.Add(Card("Ventas hoy", ventasHoy.Count.ToString()), 0, 0);
            stats.Controls.Add(Card("Ingresos hoy", "$ " + ingresosHoy.ToString("N0")), 1, 0);
            stats.Controls.Add(Card("Clientes", MockData.Clientes.Count.ToString()), 2, 0);
            stats.Controls.Add(Card("Productos", MockData.Productos.Count.ToString()), 3, 0);

            var topBox = new GroupBox{ Text="Top productos vendidos (últimas 50 ventas)", Dock=DockStyle.Fill };
            var topGrid = new DataGridView{ Dock=DockStyle.Fill, ReadOnly=true, AutoGenerateColumns=true };
            var top = MockData.Ventas.Where(v=>!v.Anulada)
                        .OrderByDescending(v => v.Fecha)
                        .Take(50)
                        .SelectMany(v => v.Detalles)
                        .GroupBy(d => d.ProductoNombre)
                        .Select(g => new { Producto = g.Key, Cantidad = g.Sum(x => x.Cantidad), Recaudado = g.Sum(x => x.Subtotal) })
                        .OrderByDescending(x => x.Cantidad)
                        .ToList();
            topGrid.DataSource = top;
            topBox.Controls.Add(topGrid);

            var bottom = new TableLayoutPanel{ Dock=DockStyle.Fill, ColumnCount=2 };
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            var ultBox = new GroupBox{ Text="Últimas ventas", Dock=DockStyle.Fill };
            var ultGrid = new DataGridView{ Dock=DockStyle.Fill, ReadOnly=true, AutoGenerateColumns=true };
            ultGrid.DataSource = MockData.Ventas.Where(v=>!v.Anulada).OrderByDescending(v => v.Fecha).Take(12)
                .Select(v => new { v.Id, Fecha = v.Fecha.ToString("dd/MM HH:mm"), v.ClienteNombre, v.Vendedor, Total = v.Total.ToString("N0") })
                .ToList();
            ultBox.Controls.Add(ultGrid);

            var stockBox = new GroupBox{ Text="Bajo stock", Dock=DockStyle.Fill };
            var stockGrid = new DataGridView{ Dock=DockStyle.Fill, ReadOnly=true, AutoGenerateColumns=true };
            stockGrid.DataSource = MockData.Productos
                .Where(p => p.Stock <= p.StockMinimo && p.Activo)
                .Select(p => new { p.Nombre, p.CategoriaNombre, p.Stock, p.StockMinimo })
                .ToList();
            stockBox.Controls.Add(stockGrid);

            bottom.Controls.Add(ultBox, 0, 0);
            bottom.Controls.Add(stockBox, 1, 0);

            main.Controls.Add(stats, 0, 0);
            main.Controls.Add(topBox, 0, 1);
            main.Controls.Add(bottom, 0, 2);
            Controls.Add(main);
        }
    }
}
