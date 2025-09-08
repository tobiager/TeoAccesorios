using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

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
            stats.Controls.Add(Card("Clientes", out lblClientes), 2, 0);
            stats.Controls.Add(Card("Productos", out lblProductos), 3, 0);
            var btnRefrescar = new Button { Text = "Refrescar", Dock = DockStyle.Fill, Height = 36, BackColor = Color.FromArgb(14, 165, 233), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnRefrescar.FlatAppearance.BorderSize = 0;
            btnRefrescar.Click += (_, __) => LoadFromDb();
            stats.Controls.Add(btnRefrescar, 4, 0);

            var topBox = new GroupBox { Text = "Top productos vendidos (últimas 50 ventas)", Dock = DockStyle.Fill };
            topGrid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
            topBox.Controls.Add(topGrid);

            var bottom = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            var ultBox = new GroupBox { Text = "Últimas ventas", Dock = DockStyle.Fill };
            ultGrid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
            ultBox.Controls.Add(ultGrid);

            var stockBox = new GroupBox { Text = "Bajo stock", Dock = DockStyle.Fill };
            stockGrid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
            stockBox.Controls.Add(stockGrid);

            bottom.Controls.Add(ultBox, 0, 0);
            bottom.Controls.Add(stockBox, 1, 0);

            main.Controls.Add(stats, 0, 0);
            main.Controls.Add(topBox, 0, 1);
            main.Controls.Add(bottom, 0, 2);
            Controls.Add(main);

            LoadFromDb();
        }

        private void LoadFromDb()
        {
            // KPIs rápidos
            var ventasHoy = Db.Scalar<int>(
                "SELECT COUNT(*) FROM cabeceraventa WHERE CAST(fechaVenta AS date) = CAST(GETDATE() AS date)");
            var ingresosHoy = Db.Scalar<decimal>(@"
        SELECT COALESCE(SUM(d.cantidad * d.precioUnitario),0)
        FROM cabeceraventa v
        JOIN detalleventa_ext d ON d.id_venta = v.id_venta
        WHERE CAST(v.fechaVenta AS date) = CAST(GETDATE() AS date)");
            var totalClientes = Db.Scalar<int>("SELECT COUNT(*) FROM cliente");
            var totalProductos = Db.Scalar<int>("SELECT COUNT(*) FROM producto");

            lblVentasHoy.Text = ventasHoy.ToString();
            lblIngresosHoy.Text = "$ " + Math.Round(ingresosHoy, 0).ToString("N0");
            lblClientes.Text = totalClientes.ToString();
            lblProductos.Text = totalProductos.ToString();

            // Top productos en las últimas 50 ventas
            var dtTop = Db.Query(@"
        WITH ult AS (
            SELECT TOP (50) v.id_venta
            FROM cabeceraventa v
            ORDER BY v.fechaVenta DESC
        )
        SELECT d.id_producto,
               p.nombre AS Producto,
               SUM(d.cantidad) AS Cantidad,
               SUM(d.cantidad * d.precioUnitario) AS Recaudado
        FROM ult u
        JOIN detalleventa_ext d ON d.id_venta = u.id_venta
        JOIN producto p ON p.id_producto = d.id_producto
        GROUP BY d.id_producto, p.nombre
        ORDER BY Cantidad DESC;");
            topGrid.DataSource = dtTop;

            // Últimas ventas: total calculado desde detalleventa_ext, sin unir a usuario
            var dtUlt = Db.Query(@"
        SELECT TOP (12)
               v.id_venta AS Id,
               FORMAT(v.fechaVenta, 'dd/MM HH:mm') AS Fecha,
               ISNULL(c.nombre,'') + COALESCE(' ' + NULLIF(c.apellido,''),'') AS Cliente,
               v.vendedor AS Usuario,
               SUM(d.cantidad * d.precioUnitario) AS Total
        FROM cabeceraventa v
        LEFT JOIN cliente c ON c.id_cliente = v.id_cliente
        JOIN detalleventa_ext d ON d.id_venta = v.id_venta
        GROUP BY v.id_venta, v.fechaVenta, c.nombre, c.apellido, v.vendedor
        ORDER BY v.fechaVenta DESC;");
            ultGrid.DataSource = dtUlt;

            // Bajo stock (igual que antes)
            var dtStock = Db.Query(@"
        SELECT p.nombre AS Producto, s.descripcion AS Subcategoria, c.nombre AS Categoria, p.stock, p.stockMinimo
        FROM producto p
        JOIN subcategoria s ON s.id_subcategoria = p.id_subcategoria
        JOIN categoria c ON c.id_categoria = s.id_categoria
        WHERE p.activo = 1 AND p.stock <= p.stockMinimo
        ORDER BY p.stock ASC;");
            stockGrid.DataSource = dtStock;
        }

    }
}
