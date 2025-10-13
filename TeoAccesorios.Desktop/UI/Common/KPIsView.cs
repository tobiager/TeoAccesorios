using System;
using System.Data;
using System.Drawing;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

using WFSortOrder = System.Windows.Forms.SortOrder;

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

            // Top productos
            var topBox = new GroupBox { Text = "Top productos vendidos (últimas 50 ventas)", Dock = DockStyle.Fill };
            topBox.ForeColor = Color.White;
            topBox.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            topGrid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
            topBox.Controls.Add(topGrid);

            // Panel inferior
            var bottom = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            // Últimas ventas
            var ultBox = new GroupBox { Text = "Últimas ventas", Dock = DockStyle.Fill };
            ultBox.ForeColor = Color.White;
            ultBox.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            ultGrid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
            ultBox.Controls.Add(ultGrid);

            // Bajo stock
            var stockBox = new GroupBox { Text = "Bajo stock", Dock = DockStyle.Fill };
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

            // Aplicar ordenamiento con glifo blanco
            // Se llama después de ThemeGrid para asegurar que los estilos base están aplicados.
            // El ordenamiento se aplicará correctamente cuando se carguen los datos y las columnas.
            WireWhiteSortGlyph(topGrid);
            WireWhiteSortGlyph(ultGrid);
            WireWhiteSortGlyph(stockGrid);

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

            // Top productos (últimas 50 ventas)
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
                ORDER BY Cantidad DESC;",
                Array.Empty<SqlParameter>());
            topGrid.DataSource = dtTop;

            if (topGrid.Columns.Contains("id_producto"))
                topGrid.Columns["id_producto"].HeaderText = "Id";

            // Últimas ventas
            var dtUlt = Db.Query(@"
                SELECT TOP (12)
                       v.Id AS Id,
                       FORMAT(v.Fecha, 'dd/MM HH:mm') AS Fecha,
                       COALESCE(NULLIF(LTRIM(RTRIM(c.Nombre)), ''), LTRIM(RTRIM(CAST(v.ClienteId AS nvarchar(20))))) AS Cliente,
                       v.Vendedor AS Usuario,
                       SUM(d.Cantidad * d.PrecioUnitario) AS Total
                FROM dbo.Ventas v
                LEFT JOIN dbo.Clientes c   ON c.Id = v.ClienteId
                LEFT JOIN dbo.DetalleVenta d ON d.VentaId = v.Id
                WHERE ISNULL(v.Anulada,0)=0
                GROUP BY v.Id, v.Fecha, c.Nombre, v.ClienteId, v.Vendedor
                ORDER BY v.Fecha DESC;",
             Array.Empty<SqlParameter>());
            ultGrid.DataSource = dtUlt;

            // Bajo stock
            var dtStock = Db.Query(@"
                SELECT p.nombre AS Producto, s.descripcion AS Subcategoria, c.nombre AS Categoria, p.stock, p.stockMinimo
                FROM producto p
                JOIN subcategoria s ON s.id_subcategoria = p.id_subcategoria
                JOIN categoria c ON c.id_categoria = s.id_categoria
                WHERE p.activo = 1 AND p.stock <= p.stockMinimo
                ORDER BY p.stock ASC;",
                Array.Empty<SqlParameter>());
            stockGrid.DataSource = dtStock;
        }

        private void WireWhiteSortGlyph(DataGridView g)
        {
            if (g == null) return;

            // usar nuestros estilos (texto ya es blanco)
            g.EnableHeadersVisualStyles = false;

            // forzar sort programático (así no sale el glyph por defecto)
            g.ColumnAdded += (_, ev) => { if (ev.Column.SortMode != DataGridViewColumnSortMode.NotSortable) ev.Column.SortMode = DataGridViewColumnSortMode.Programmatic; };
            foreach (DataGridViewColumn c in g.Columns)
                if (c.SortMode != DataGridViewColumnSortMode.NotSortable)
                    c.SortMode = DataGridViewColumnSortMode.Programmatic;

            // click en header: aplicar orden y guardar estado en Tag
            g.ColumnHeaderMouseClick += (_, e) =>
            {
                if (e.ColumnIndex < 0) return;
                var col = g.Columns[e.ColumnIndex];

                var current = col.HeaderCell.Tag is WFSortOrder t ? t : WFSortOrder.None;
                var next = current == WFSortOrder.Ascending ? WFSortOrder.Descending : WFSortOrder.Ascending;

                // ordenar (BindingSource si existe; si no, DataGridView.Sort)
                if (g.DataSource is BindingSource bs)
                {
                    var prop = string.IsNullOrEmpty(col.DataPropertyName) ? col.Name : col.DataPropertyName;
                    bs.Sort = $"{prop} {(next == WFSortOrder.Ascending ? "ASC" : "DESC")}";
                }
                else
                {
                    g.Sort(col, next == WFSortOrder.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending);
                }

                // resetear otros headers y setear este
                foreach (DataGridViewColumn c in g.Columns) if (c != col) c.HeaderCell.Tag = WFSortOrder.None;
                col.HeaderCell.Tag = next;

                g.Invalidate(); // repintar headers
            };

            // pintar header + glyph blanco
            g.CellPainting += (_, e) =>
            {
                if (e.RowIndex == -1 && e.ColumnIndex >= 0)
                {
                    e.Paint(e.ClipBounds, DataGridViewPaintParts.All); // pintá todo (sin glyph por defecto porque SortMode es Programmatic)

                    var col = g.Columns[e.ColumnIndex];
                    var order = col.HeaderCell.Tag is WFSortOrder ord ? ord : WFSortOrder.None;
                    if (order != WFSortOrder.None)
                    {
                        DrawWhiteSortTriangle(e.Graphics!, e.CellBounds, order);
                    }

                    e.Handled = true;
                }
            };
        }

        private static void DrawWhiteSortTriangle(Graphics g, Rectangle cell, WFSortOrder order)
        {
            // triángulo pequeño a la derecha del header
            int w = 10, h = 6;
            int paddingRight = 10;
            int centerX = cell.Right - paddingRight - w / 2;
            int centerY = cell.Top + cell.Height / 2;

            Point[] pts = (order == WFSortOrder.Ascending)
                ? new[] {
                    new Point(centerX - w/2, centerY + h/2),
                    new Point(centerX + w/2, centerY + h/2),
                    new Point(centerX,       centerY - h/2),
                }
                : new[] {
                    new Point(centerX - w/2, centerY - h/2),
                    new Point(centerX + w/2, centerY - h/2),
                    new Point(centerX,       centerY + h/2),
                };

            using (var brush = new SolidBrush(Color.White))
                g.FillPolygon(brush, pts);
        }
    }
}
