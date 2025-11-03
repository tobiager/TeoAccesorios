using System;
using System.Data;
using System.Drawing;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace TeoAccesorios.Desktop
{
    public class KPIsView : UserControl
    {
        Label lblVentasHoy, lblIngresosHoy, lblClientes, lblProductos;
        DataGridView topGrid, ultGrid, stockGrid;

        public KPIsView()
        {
            var main = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(12) };
            main.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
            main.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            main.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            var stats = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 5 };
            for (int i = 0; i < 5; i++) stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));

            Control Card(string label, out Label valueLabel)
            {
                var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(18, 22, 35), Padding = new Padding(12) };
                var l1 = new Label { Text = label, ForeColor = Color.Gainsboro, Dock = DockStyle.Top, Height = 24 };
                valueLabel = new Label { Text = "-", ForeColor = Color.White, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 20, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
                panel.Controls.Add(valueLabel); panel.Controls.Add(l1);
                return panel;
            }

            stats.Controls.Add(Card("Ventas hoy", out lblVentasHoy), 0, 0);
            stats.Controls.Add(Card("Ingresos hoy", out lblIngresosHoy), 1, 0);
            stats.Controls.Add(Card("Clientes activos", out lblClientes), 2, 0);
            stats.Controls.Add(Card("Productos activos", out lblProductos), 3, 0);
            var btnRefrescar = new Button { Text = "Refrescar", Dock = DockStyle.Fill, Height = 36, BackColor = Color.FromArgb(14, 165, 233), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnRefrescar.FlatAppearance.BorderSize = 0;
            btnRefrescar.Click += (_, __) => LoadFromDb();
            stats.Controls.Add(btnRefrescar, 4, 0);

            // Top productos
            var topBox = new GroupBox { Text = "Top productos vendidos (últimas 50 ventas activas)", Dock = DockStyle.Fill };
            topBox.ForeColor = Color.White;
            topBox.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            topGrid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
            topBox.Controls.Add(topGrid);

            // Panel inferior
            var bottom = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            // Últimas ventas
            var ultBox = new GroupBox { Text = "Últimas ventas activas", Dock = DockStyle.Fill };
            ultBox.ForeColor = Color.White;
            ultBox.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            ultGrid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
            ultBox.Controls.Add(ultGrid);

            // Bajo stock
            var stockBox = new GroupBox { Text = "Productos activos con bajo stock", Dock = DockStyle.Fill };
            stockBox.ForeColor = Color.White;
            stockBox.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            stockGrid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
            stockBox.Controls.Add(stockGrid);

            bottom.Controls.Add(ultBox, 0, 0);
            bottom.Controls.Add(stockBox, 1, 0);

            main.Controls.Add(stats, 0, 0);
            main.Controls.Add(topBox, 0, 1);
            main.Controls.Add(bottom, 0, 2);
            Controls.Add(main);

           
            GridHelper.Estilizar(topGrid);
            GridHelper.Estilizar(ultGrid);
            GridHelper.Estilizar(stockGrid);

            ThemeGrid(topGrid);
            ThemeGrid(ultGrid);
            ThemeGrid(stockGrid);

            // Usar GridHelper para el manejo del glifo blanco y alternancia asc/desc.
            // GridHelper.WireWhiteSortGlyph preserva estado entre rebinds y hace toggle automáticamente.
            GridHelper.WireWhiteSortGlyph(topGrid);
            GridHelper.WireWhiteSortGlyph(ultGrid);
            GridHelper.WireWhiteSortGlyph(stockGrid);

            GridHelperLock.Apply(topGrid);
            GridHelperLock.Apply(ultGrid);
            GridHelperLock.Apply(stockGrid);
            LoadFromDb();
        }

        
        private void ThemeGrid(DataGridView g)
        {
            g.EnableHeadersVisualStyles = false;

            
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235); 
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            
            g.DefaultCellStyle.BackColor = Color.White;
            g.DefaultCellStyle.ForeColor = Color.Black;
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(14, 165, 233);
            g.DefaultCellStyle.SelectionForeColor = Color.White;

            
            g.BackgroundColor = Color.White;
            g.BorderStyle = BorderStyle.None;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.RowHeadersVisible = false;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void LoadFromDb()
        {
            // Usar el Repository para obtener solo datos activos
            var ventasActivas = Repository.ListarVentas(incluirAnuladas: false); // Solo ventas no anuladas
            var clientesActivos = Repository.ListarClientes(incluirInactivos: false); // Solo clientes activos
            var productosActivos = Repository.ListarProductos(incluirInactivos: false); // Solo productos activos
            
            var hoy = DateTime.Today;
            
            // KPIs basados en datos activos
            var ventasHoy = ventasActivas.Where(v => v.FechaVenta.Date == hoy).Count();
            var ingresosHoy = ventasActivas.Where(v => v.FechaVenta.Date == hoy).Sum(v => v.Total);
            var totalClientesActivos = clientesActivos.Count;
            var totalProductosActivos = productosActivos.Count;

            lblVentasHoy.Text = ventasHoy.ToString();
            lblIngresosHoy.Text = "$ " + Math.Round(ingresosHoy, 0).ToString("N0");
            lblClientes.Text = totalClientesActivos.ToString();
            lblProductos.Text = totalProductosActivos.ToString();

            // Top productos de las últimas 50 ventas activas (no anuladas)
            var ultimasVentas = ventasActivas
                .OrderByDescending(v => v.FechaVenta)
                .Take(50)
                .ToList();

            var topProductos = ultimasVentas
                .SelectMany(v => v.Detalles)
                .Where(d => productosActivos.Any(p => p.Id == d.ProductoId)) // Solo productos activos
                .GroupBy(d => new { d.ProductoId, d.ProductoNombre })
                .Select(g => new
                {
                    Id = g.Key.ProductoId,
                    Producto = g.Key.ProductoNombre,
                    Cantidad = g.Sum(d => d.Cantidad),
                    Recaudado = g.Sum(d => d.Subtotal)
                })
                .OrderByDescending(x => x.Cantidad)
                .ToList();

            // Bindear como BindingSource para mayor control; la ordenación se aplicará de forma segura en el handler.
            topGrid.DataSource = new BindingSource { DataSource = topProductos };

            // Ajustar encabezados
            if (topGrid.Columns.Contains("Id"))
                topGrid.Columns["Id"].HeaderText = "Id";

            // Últimas ventas activas (no anuladas)
            var ultimasVentasDetalle = ventasActivas
                .OrderByDescending(v => v.FechaVenta)
                .Take(12)
                .Select(v => new
                {
                    Id = v.Id,
                    Fecha = v.FechaVenta.ToString("dd/MM HH:mm"),
                    Cliente = v.ClienteNombre,
                    Usuario = v.Vendedor,
                    Total = v.Total
                })
                .ToList();

            ultGrid.DataSource = new BindingSource { DataSource = ultimasVentasDetalle };

            // Productos activos con bajo stock
            var productosStock = productosActivos
                .Where(p => p.Stock <= p.StockMinimo)
                .OrderBy(p => p.Stock)
                .Select(p => new
                {
                    Producto = p.Nombre,
                    Categoria = p.CategoriaNombre,
                    Subcategoria = p.SubcategoriaNombre,
                    Stock = p.Stock,
                    StockMinimo = p.StockMinimo
                })
                .ToList();

            stockGrid.DataSource = new BindingSource { DataSource = productosStock };
        }
    }
}
