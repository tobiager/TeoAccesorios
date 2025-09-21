using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class VentaDetalleForm : Form
    {
        // Grid de líneas de venta
        private readonly DataGridView grid = new()
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        // Mantengo la venta y el cliente (lo recibo desde el form de lista)
        private readonly Venta _venta;
        private readonly Cliente? _cliente;

        public VentaDetalleForm(Venta venta, Cliente? cliente = null)
        {
            _venta = venta;
            _cliente = cliente; 

            // Ventana
            Text = $"Venta #{venta.Id}";
            Width = 840; Height = 620;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            KeyPreview = true; BackColor = Color.White; Padding = new Padding(10);
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };

           
            typeof(DataGridView).InvokeMember(
                "DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null, grid, new object[] { true });

            
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(8)
            };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));     
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); 
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));     
            Controls.Add(root);

            // Header
            root.Controls.Add(BuildHeader(), 0, 0);

            // Grid
            ConfigurarColumnasTolerantes(_venta);
            EstilosGrid();
            grid.DataSource = _venta.Detalles;
            try { GridHelper.Estilizar(grid); } catch { }
            try { GridHelperLock.WireDataBindingLock(grid); } catch { }
            root.Controls.Add(grid, 0, 1);

            // Footer (total)
            var pnlTotal = new Panel { Dock = DockStyle.Bottom, Height = 54, Padding = new Padding(12, 8, 12, 8), BackColor = Color.White };
            pnlTotal.Controls.Add(new Label { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(230, 230, 230) });

            var totalCalc = _venta.Total > 0
                ? _venta.Total
                : _venta.Detalles?.Sum(d => GetDecimal(d, "Subtotal", "SubTotal", "Importe", "TotalLinea")) ?? 0m;

            pnlTotal.Controls.Add(new Label
            {
                Dock = DockStyle.Right,
                Width = 300,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI Semibold", 13f, FontStyle.Bold),
                Text = $"Total  $ {totalCalc:N0}"
            });
            pnlTotal.Controls.Add(new Label { Dock = DockStyle.Left, AutoSize = true, ForeColor = Color.Gray, Text = "Esc para cerrar", Padding = new Padding(0, 10, 0, 0) });

            root.Controls.Add(pnlTotal, 0, 2);
        }

        // Header con tarjetas (fecha, cliente, dirección, vendedor, localidad, provincia)
        private Control BuildHeader()
        {
            // Dirección desde Venta.*
            var direccion = GetStringPath(_venta, "DireccionEnvio", "DirecciónEnvio", "Direccion", "Dirección");

            // Localidad/Provincia: las saco del Cliente que recibo
            var localidad = _cliente?.Localidad ?? "";
            var provincia = _cliente?.Provincia ?? "";

            var wrap = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 3,
                RowCount = 3,
                Padding = new Padding(0, 0, 0, 6)
            };
            wrap.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            wrap.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            wrap.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34f));

            // Título + chips
            var title = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Margin = new Padding(0, 0, 0, 6) };
            title.Controls.Add(new Label { AutoSize = true, Text = $"Venta #{_venta.Id}", Font = new Font("Segoe UI Semibold", 12f, FontStyle.Bold), ForeColor = Color.FromArgb(28, 37, 54), Padding = new Padding(0, 0, 6, 0) });
            if (!string.IsNullOrWhiteSpace(_venta.Canal))
                title.Controls.Add(Badge(_venta.Canal, Color.FromArgb(230, 243, 255), Color.FromArgb(26, 95, 180)));

            var anuladaProp = _venta.GetType().GetProperty("Anulada", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (anuladaProp is not null && (anuladaProp.GetValue(_venta) as bool? ?? false))
                title.Controls.Add(Badge("ANULADA", Color.FromArgb(255, 236, 236), Color.FromArgb(183, 28, 28)));

            wrap.Controls.Add(title, 0, 0);
            wrap.SetColumnSpan(title, 3);

            // Tarjetas 3×2
            wrap.Controls.Add(InfoCard("\uE8A5  Fecha", _venta.Fecha.ToString("dd/MM/yyyy HH:mm")), 0, 1);
            wrap.Controls.Add(InfoCard("\uE77B  Cliente", string.IsNullOrWhiteSpace(_venta.ClienteNombre) ? "-" : _venta.ClienteNombre), 1, 1);
            wrap.Controls.Add(InfoCard("\uE13D  Dirección", string.IsNullOrWhiteSpace(direccion) ? "-" : direccion), 2, 1);

            wrap.Controls.Add(InfoCard("\uE76E  Vendedor", string.IsNullOrWhiteSpace(_venta.Vendedor) ? "-" : _venta.Vendedor), 0, 2);
            wrap.Controls.Add(InfoCard("\uE909  Localidad", string.IsNullOrWhiteSpace(localidad) ? "-" : localidad), 1, 2);
            wrap.Controls.Add(InfoCard("\uE77C  Provincia", string.IsNullOrWhiteSpace(provincia) ? "-" : provincia), 2, 2);

            return wrap;
        }

        // Tarjeta con título + botón de copiar
        private Control InfoCard(string titleWithIcon, string value)
        {
            var card = new Panel { Margin = new Padding(0, 0, 8, 8), Padding = new Padding(12), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            card.Paint += (s, e) => { using var p = new Pen(Color.FromArgb(235, 235, 235)); e.Graphics.DrawLine(p, 0, card.Height - 1, card.Width, card.Height - 1); };

            var title = new Label { AutoSize = true, Text = titleWithIcon, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(60, 72, 88) };
            var txt = new Label { AutoSize = false, Dock = DockStyle.Top, Height = 24, Text = value ?? "-", Font = new Font("Segoe UI", 10f), ForeColor = Color.FromArgb(24, 28, 32), AutoEllipsis = true };
            var tip = new ToolTip(); txt.MouseEnter += (s, e) => tip.SetToolTip(txt, value ?? "-");

            var btnCopy = new Label
            {
                AutoSize = false,
                Width = 22,
                Height = 22,
                Text = "\uE8C8",
                Font = new Font("Segoe MDL2 Assets", 10f),
                ForeColor = Color.FromArgb(90, 110, 130),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnCopy.Click += (s, e) => { try { Clipboard.SetText(value ?? ""); } catch { } };

            var top = new TableLayoutPanel { Dock = DockStyle.Top, Height = 22, ColumnCount = 2 };
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            top.Controls.Add(title, 0, 0);
            top.Controls.Add(btnCopy, 1, 0);

            card.Controls.Add(txt);
            card.Controls.Add(top);
            card.Height = 64;
            return card;
        }

        private Control Badge(string text, Color back, Color fore) =>
            new Label { AutoSize = true, Text = text, BackColor = back, ForeColor = fore, Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold), Padding = new Padding(8, 3, 8, 3), Margin = new Padding(0, 0, 8, 0) };

        // Estética del grid
        private void EstilosGrid()
        {
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(32, 86, 179);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 10f);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(230, 239, 255);
            grid.DefaultCellStyle.SelectionForeColor = Color.Black;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        }

        // Configuro columnas con tolerancia a nombres distintos en el detalle
        private void ConfigurarColumnasTolerantes(Venta v)
        {
            grid.Columns.Clear();
            var itemType = v.Detalles?.GetType().GetGenericArguments().FirstOrDefault()
                           ?? v.Detalles?.FirstOrDefault()?.GetType();
            string P(params string[] cands) => ResolveProp(itemType, cands);

            grid.Columns.Add(new DataGridViewTextBoxColumn
            { DataPropertyName = P("ProductoNom", "ProductoNombre", "NombreProducto", "Producto", "Descripcion"), HeaderText = "Producto", FillWeight = 260, MinimumWidth = 160 });

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = P("Cantidad", "Qty", "Unidades"),
                HeaderText = "Cantidad",
                Width = 90,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter, Format = "N0" }
            });

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = P("PrecioUnitario", "PrecioUnitari", "Precio", "UnitPrice"),
                HeaderText = "Precio Unitario",
                Width = 140,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight, Format = "N0" }
            });

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = P("Subtotal", "SubTotal", "Importe", "TotalLinea"),
                HeaderText = "Subtotal",
                Width = 140,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight, Format = "N0", Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold) }
            });

            HideIfExists(P("IdDetalle", "Id", "DetalleId", "LineaId"));
            HideIfExists(P("VentaId", "IdVenta"));
            HideIfExists(P("ProductoId", "IdProducto"));
        }

        // Helpers
        private static string ResolveProp(Type t, params string[] candidates)
        {
            if (t == null || candidates == null || candidates.Length == 0) return candidates?.FirstOrDefault() ?? string.Empty;
            var names = t.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var match = candidates.FirstOrDefault(c => names.Contains(c));
            return match ?? candidates.First();
        }

        private void HideIfExists(string name)
        {
            if (string.IsNullOrEmpty(name)) return;
            var col = grid.Columns.Cast<DataGridViewColumn>().FirstOrDefault(c =>
                string.Equals(c.DataPropertyName, name, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
            if (col != null) col.Visible = false;
        }

        private static decimal GetDecimal(object obj, params string[] props)
        {
            if (obj == null) return 0m;
            var t = obj.GetType();
            foreach (var p in props)
            {
                var pi = t.GetProperty(p, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (pi == null) continue;
                var val = pi.GetValue(obj);
                if (val == null) continue;
                try { return Convert.ToDecimal(val); } catch { }
            }
            return 0m;
        }

        private static string GetString(object obj, params string[] props)
        {
            if (obj == null) return null;
            var t = obj.GetType();
            foreach (var p in props)
            {
                var pi = t.GetProperty(p, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (pi == null) continue;
                var val = pi.GetValue(obj);
                if (val == null) continue;
                try { return Convert.ToString(val); } catch { }
            }
            return null;
        }

        // Permite leer propiedades anidadas por nombre (ej: "DireccionEnvio")
        private static string GetStringPath(object root, params string[] paths)
        {
            if (root == null || paths == null) return null;
            foreach (var path in paths)
            {
                if (string.IsNullOrWhiteSpace(path)) continue;
                object current = root;
                foreach (var part in path.Split('.'))
                {
                    if (current == null) break;
                    var pi = current.GetType().GetProperty(part, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                    if (pi == null) { current = null; break; }
                    current = pi.GetValue(current);
                }
                if (current != null) { try { return Convert.ToString(current); } catch { } }
            }
            return null;
        }
    }
}
