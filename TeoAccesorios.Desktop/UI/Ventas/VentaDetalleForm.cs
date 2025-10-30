using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;
using System.Drawing.Printing;
using System.Collections;
using System.IO; 

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
        };

        private readonly Venta _venta;
        private readonly Cliente? _cliente;

        public VentaDetalleForm(Venta venta, Cliente? cliente = null)
        {
            _venta = venta;
            _cliente = cliente;

            // Ventana
            Text = $"Venta #{venta.Id}";
            Width = 840; Height = 620; // Se ajustará en AplicarAnchoPreferido
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            KeyPreview = true; BackColor = Color.White; Padding = new Padding(10);
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };

            // Suaviza el scroll del grid
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
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));     // header
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // grid
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));     // footer
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
            var pnlTotal = new Panel { Dock = DockStyle.Bottom, Height = 64, Padding = new Padding(12, 8, 12, 8), BackColor = Color.White };
            pnlTotal.Controls.Add(new Label { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(230, 230, 230) });

            var totalCalc = _venta.Total > 0
                ? _venta.Total
                : _venta.Detalles?.Sum(d => GetDecimal(d, "Subtotal", "SubTotal", "Importe", "TotalLinea")) ?? 0m;

            // panel derecho que contiene boton imprimir y total
            var rightPanel = new Panel { Dock = DockStyle.Right, Width = 380, BackColor = Color.White };

            var totalLabel = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI Semibold", 13f, FontStyle.Bold),
                Text = $"Total  $ {totalCalc:N0}"
            };

            var btnPrint = new Button
            {
                AutoSize = true,
                Text = "Imprimir",
                Dock = DockStyle.Right,
                Padding = new Padding(8, 6, 8, 6),
                BackColor = Color.FromArgb(32, 86, 179),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            };
            btnPrint.FlatAppearance.BorderSize = 0;
            btnPrint.Click += (s, e) => { try { ImprimirFactura(); } catch { } };

            // agregar controls al panel derecho (btn a la derecha del total)
            rightPanel.Controls.Add(totalLabel);
            rightPanel.Controls.Add(btnPrint);

            pnlTotal.Controls.Add(rightPanel);

            pnlTotal.Controls.Add(new Label
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                ForeColor = Color.Gray,
                Text = "Esc para cerrar",
                Padding = new Padding(0, 10, 0, 0)
            });

            root.Controls.Add(pnlTotal, 0, 2);

            // Ensancha el form y ajusta la grilla para que las columnas no se achiquen.
            // Se llama en el Load para asegurar que el handle del form esté creado.
            this.Load += (_, __) => AplicarAnchoPreferido();
        }

        // Header con tarjetas
        private Control BuildHeader()
        {
            // Dirección desde Venta.* (soporta varios nombres)
            var direccion = GetStringPath(_venta,
                "DireccionEnvio", "DirecciónEnvio", "DireccionVenta", "Direccion", "Dirección");

            // Localidad / Provincia
            int? localidadId = _venta.LocalidadId;
            if (!localidadId.HasValue && _cliente?.LocalidadId != null)
                localidadId = _cliente.LocalidadId;

            string localidad = "-", provincia = "-";
            if (localidadId.HasValue)
            {
                try
                {
                    var loc = Repository.ListarLocalidades(null, true)
                                        .FirstOrDefault(l => l.Id == localidadId.Value);
                    if (loc != null)
                    {
                        localidad = loc.Nombre;
                        provincia = loc.ProvinciaNombre;
                    }
                }
                catch { /* repository aún no listo: mantener "-" */ }
            }

            // Otros campos tolerantes
            var fechaTxt = GetDateString(_venta, "Fecha", "FechaVenta", "FechaHora", "FechaAlta", "CreatedAt") ?? "-";
            var clienteNom = GetString(_venta, "ClienteNombre", "Cliente", "NombreCliente")
                             ?? _cliente?.Nombre ?? "-";
            var vendedor = GetString(_venta, "Vendedor", "Usuario", "UsuarioNombre") ?? "-";
            var canal = GetString(_venta, "Canal", "Medio") ?? "";

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
            var title = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = new Padding(0, 0, 0, 6)
            };
            title.Controls.Add(new Label
            {
                AutoSize = true,
                Text = $"Venta #{_venta.Id}",
                Font = new Font("Segoe UI Semibold", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(28, 37, 54),
                Padding = new Padding(0, 0, 6, 0)
            });
            if (!string.IsNullOrWhiteSpace(canal))
                title.Controls.Add(Badge(canal, Color.FromArgb(230, 243, 255), Color.FromArgb(26, 95, 180)));

            var anuladaProp = _venta.GetType().GetProperty("Anulada",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (anuladaProp is not null && (anuladaProp.GetValue(_venta) as bool? ?? false))
                title.Controls.Add(Badge("ANULADA", Color.FromArgb(255, 236, 236), Color.FromArgb(183, 28, 28)));

            wrap.Controls.Add(title, 0, 0);
            wrap.SetColumnSpan(title, 3);

            // Tarjetas 3×2
            wrap.Controls.Add(InfoCard("Fecha", fechaTxt, "\uE787"), 0, 1);
            wrap.Controls.Add(InfoCard("Cliente", string.IsNullOrWhiteSpace(clienteNom) ? "-" : clienteNom, "\uE77B"), 1, 1);
            wrap.Controls.Add(InfoCard("Dirección", string.IsNullOrWhiteSpace(direccion) ? "-" : direccion, "\uE707"), 2, 1);

            wrap.Controls.Add(InfoCard("Vendedor", string.IsNullOrWhiteSpace(vendedor) ? "-" : vendedor, "\uE7EF"), 0, 2);
            wrap.Controls.Add(InfoCard("Localidad", string.IsNullOrWhiteSpace(localidad) ? "-" : localidad, "\uE80F"), 1, 2);
            wrap.Controls.Add(InfoCard("Provincia", string.IsNullOrWhiteSpace(provincia) ? "-" : provincia, "\uE909"), 2, 2);

            return wrap;
        }

        // Tarjeta con icono + título + botón de copiar
        private Control InfoCard(string title, string value, string mdl2Glyph)
        {
            var card = new Panel { Margin = new Padding(0, 0, 8, 8), Padding = new Padding(12), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            card.Paint += (s, e) => { using var p = new Pen(Color.FromArgb(235, 235, 235)); e.Graphics.DrawLine(p, 0, card.Height - 1, card.Width, card.Height - 1); };

            // Encabezado
            var icon = new Label
            {
                AutoSize = false,
                Width = 20,
                Height = 20,
                Text = mdl2Glyph,
                Font = new Font("Segoe MDL2 Assets", 12f, FontStyle.Regular),
                ForeColor = Color.FromArgb(60, 72, 88),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 0, 4, 0)
            };

            var titleLbl = new Label
            {
                AutoSize = true,
                Text = title,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 72, 88),
                Margin = new Padding(0, 2, 0, 0)
            };

            var btnCopy = new Label
            {
                AutoSize = false,
                Width = 22,
                Height = 22,
                Text = "\uE8C8", // Copy
                Font = new Font("Segoe MDL2 Assets", 10f),
                ForeColor = Color.FromArgb(90, 110, 130),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnCopy.Click += (s, e) => { try { Clipboard.SetText(value ?? ""); } catch { } };

            var header = new TableLayoutPanel { Dock = DockStyle.Top, Height = 22, ColumnCount = 3 };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            header.Controls.Add(icon, 0, 0);
            header.Controls.Add(titleLbl, 1, 0);
            header.Controls.Add(btnCopy, 2, 0);

            var txt = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 24,
                Text = value ?? "-",
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(24, 28, 32),
                AutoEllipsis = true
            };
            var tip = new ToolTip(); txt.MouseEnter += (s, e) => tip.SetToolTip(txt, value ?? "-");

            card.Controls.Add(txt);
            card.Controls.Add(header);
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
            {
                DataPropertyName = P("ProductoNom", "ProductoNombre", "NombreProducto", "Producto", "Descripcion"),
                HeaderText = "Producto",
                FillWeight = 260,
                MinimumWidth = 160
            });

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

        // ===== Helpers =====

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

        private static string? GetString(object obj, params string[] props)
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

        // Lee propiedades anidadas por nombre (ej: "DireccionEnvio")
        private static string? GetStringPath(object root, params string[] paths)
        {
            if (root == null || paths == null) return null;
            foreach (var path in paths)
            {
                if (string.IsNullOrWhiteSpace(path)) continue;
                object? current = root;
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

        // Intenta obtener una fecha desde varios posibles nombres de propiedad
        private static string? GetDateString(object obj, params string[] props)
        {
            if (obj == null) return null;
            var t = obj.GetType();
            foreach (var p in props)
            {
                var pi = t.GetProperty(p, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (pi == null) continue;
                var val = pi.GetValue(obj);
                if (val == null) continue;

                if (val is DateTime dt) return dt.ToString("dd/MM/yyyy HH:mm");
                if (val is DateTimeOffset dto) return dto.LocalDateTime.ToString("dd/MM/yyyy HH:mm");
                // si ya viene string
                if (val is string s && DateTime.TryParse(s, out var parsed))
                    return parsed.ToString("dd/MM/yyyy HH:mm");
            }
            return null;
        }

        // ===== Ajustes de Ancho =====

        // Imprime una factura simple a partir de los datos de la venta
        private void ImprimirFactura()
        {
            // Detectar si la venta está anulada. Si la propiedad existe la usamos; si no, asumimos false.
            bool isAnulada = false;
            try
            {
                var prop = _venta.GetType().GetProperty("Anulada", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop != null) isAnulada = (prop.GetValue(_venta) as bool?) ?? false;
            }
            catch { isAnulada = false; }

            // Datos de la empresa / emprendimiento (ajustar según corresponda)
            string companyName = "Teo Accesorios";
            string companyAddress = "Av. Ejemplo 123, Ciudad";
            string companyPhone = "Tel: (011) 1234-5678";
            string companyEmail = "contacto@teoaccesorios.com";
            string companyCuit = "CUIT: 20-12345678-9";

            // Cargar logo desde recursos embebidos
            Image? logo = null;
            try
            {
                // Primero intentar desde Properties.Resources donde está logo.png
                logo = Properties.Resources.logo;
                
                // Si no está disponible, usar el método anterior como respaldo
                if (logo == null)
                {
                    var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.png");
                    if (File.Exists(logoPath)) logo = Image.FromFile(logoPath);
                    else
                    {
                        var asm = Assembly.GetExecutingAssembly();
                        var resName = asm.GetManifestResourceNames().FirstOrDefault(n => n.ToLower().Contains("logo"));
                        if (resName != null)
                        {
                            using var s = asm.GetManifestResourceStream(resName);
                            if (s != null) logo = Image.FromStream(s);
                        }
                    }
                }
            }
            catch { /* no detener impresión si no se puede cargar logo */ }

            var pd = new PrintDocument();
            pd.DefaultPageSettings.Margins = new Margins(40, 40, 40, 40);

            // NOTE: no mostramos mensaje de "impresión enviada" aquí.
            // El flujo de impresión se disparará desde el botón de impresora del preview.

            pd.PrintPage += (s, e) =>
            {
                var g = e.Graphics;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                int x = e.MarginBounds.Left;
                int y = e.MarginBounds.Top;

                var titleFont = new Font("Segoe UI", 16f, FontStyle.Bold); 
                var companyFont = new Font("Segoe UI Semibold", 13f); 
                var normal = new Font("Segoe UI", 9f);
                var small = new Font("Segoe UI", 8f);
                var bold = new Font("Segoe UI", 9f, FontStyle.Bold);

                // Header: logo más grande + datos empresa reorganizados
                int logoWidth = 0;
                int headerHeight = 100; // Aumentamos el espacio del header para el logo más grande
                
                if (logo != null)
                {
                    int desiredH = 80; 
                    int desiredW = logo.Width * desiredH / logo.Height;
                    g.DrawImage(logo, x, y, desiredW, desiredH);
                    logoWidth = desiredW + 12; 
                }

                // Empresa - reorganizada para aprovechar mejor el espacio
                float infoX = x + logoWidth;
                float companyInfoWidth = (e.MarginBounds.Width * 0.5f) - logoWidth; 
                
                g.DrawString(companyName, companyFont, Brushes.Black, infoX, y);
                g.DrawString(companyAddress, normal, Brushes.Black, infoX, y + 20);
                g.DrawString($"{companyPhone}", normal, Brushes.Black, infoX, y + 38);
                g.DrawString($"{companyEmail}", normal, Brushes.Black, infoX, y + 54);
                g.DrawString(companyCuit, small, Brushes.Black, infoX, y + 72);

                // Título factura / nota de crédito y metadata (derecha)
                var sfRight = new StringFormat { Alignment = StringAlignment.Far };
                float rightSectionX = e.MarginBounds.Left + (e.MarginBounds.Width * 0.55f); 
                float rightSectionWidth = e.MarginBounds.Width * 0.45f; 
                
                var titleRect = new RectangleF(rightSectionX, y, rightSectionWidth, 24);
                // Mostrar diferente título y color si es nota de crédito (anulada)
                var titleBrush = isAnulada ? Brushes.DarkRed : Brushes.Black;
                var titleText = isAnulada ? "NOTA DE CRÉDITO" : "FACTURA";
                g.DrawString(titleText, titleFont, titleBrush, titleRect, sfRight);

                var fecha = GetDateString(_venta, "Fecha", "FechaVenta", "FechaHora", "FechaAlta", "CreatedAt") ?? "-";
                var clienteNom = GetString(_venta, "ClienteNombre", "Cliente", "NombreCliente") ?? _cliente?.Nombre ?? "-";

                // Mostrar referencia: si es nota de crédito dejamos "Nota de crédito de venta #..."
                var referencia = isAnulada ? $"NOTA - Venta #{_venta.Id}" : $"Venta #{_venta.Id}";
                g.DrawString(referencia, bold, titleBrush, new RectangleF(rightSectionX, y + 26, rightSectionWidth, 16), sfRight);
                g.DrawString($"Fecha: {fecha}", normal, Brushes.Black, new RectangleF(rightSectionX, y + 44, rightSectionWidth, 14), sfRight);
                g.DrawString($"Cliente: {clienteNom}", normal, Brushes.Black, new RectangleF(rightSectionX, y + 62, rightSectionWidth, 14), sfRight);

                y += headerHeight; // Usamos la nueva altura del header

                // Separador
                g.DrawLine(Pens.Gray, e.MarginBounds.Left, y, e.MarginBounds.Right, y);
                y += 8;

                // Encabezados tabla - redistribuimos el espacio de manera más eficiente
                int tableWidth = e.MarginBounds.Width;
                int xProd = e.MarginBounds.Left;
                int prodWidth = (int)(tableWidth * 0.45f); // 45% para producto
                int xQty = xProd + prodWidth;
                int qtyWidth = (int)(tableWidth * 0.12f); // 12% para cantidad
                int xPrecio = xQty + qtyWidth;
                int precioWidth = (int)(tableWidth * 0.20f); // 20% para precio
                int xSub = xPrecio + precioWidth;
                int subWidth = (int)(tableWidth * 0.23f); // 23% para subtotal

                g.DrawString("Producto", bold, Brushes.Black, xProd, y);
                g.DrawString("Cant.", bold, Brushes.Black, new RectangleF(xQty, y, qtyWidth, 16), new StringFormat { Alignment = StringAlignment.Center });
                g.DrawString("Precio", bold, Brushes.Black, new RectangleF(xPrecio, y, precioWidth, 16), new StringFormat { Alignment = StringAlignment.Center });
                g.DrawString("Subtotal", bold, Brushes.Black, new RectangleF(xSub, y, subWidth, 16), new StringFormat { Alignment = StringAlignment.Far });
                y += 18;

                // Líneas de detalle
                IEnumerable detallesEnum = _venta.Detalles ?? Enumerable.Empty<object>();
                foreach (object d in detallesEnum)
                {
                    if (y > e.MarginBounds.Bottom - 120) { e.HasMorePages = true; return; }

                    var prod = GetString(d, "ProductoNom", "ProductoNombre", "NombreProducto", "Producto", "Descripcion") ?? "-";
                    var qty = GetDecimal(d, "Cantidad", "Qty", "Unidades");
                    var precio = GetDecimal(d, "PrecioUnitario", "PrecioUnitari", "Precio", "UnitPrice");
                    var sub = GetDecimal(d, "Subtotal", "SubTotal", "Importe", "TotalLinea");

                    // Producto: permitir wrapping si es largo y medir la altura necesaria
                    var measured = g.MeasureString(prod, normal, prodWidth - 8);
                    var prodHeight = Math.Max(measured.Height, normal.GetHeight(g));

                    // Dibujar producto (puede ocupar varias líneas)
                    var prodRect = new RectangleF(xProd, y, prodWidth - 8, prodHeight);
                    g.DrawString(prod, normal, Brushes.Black, prodRect, new StringFormat { Alignment = StringAlignment.Near });

                    // Línea base para textos
                    float baseLineY = y + prodHeight - normal.GetHeight(g);

                    // Cantidad - mostrar negativa si es nota de crédito
                    var qtyDisplay = isAnulada ? $"-{qty:N0}" : $"{qty:N0}";
                    g.DrawString(qtyDisplay, normal, Brushes.Black, new RectangleF(xQty, baseLineY, qtyWidth, normal.GetHeight(g)), new StringFormat { Alignment = StringAlignment.Center });

                    // Precio - habitualmente se muestra positivo, pero si prefieres negativo puedes cambiarlo.
                    var precioDisplay = isAnulada ? $"-$ {Math.Abs(precio):N0}" : $"$ {precio:N0}";
                    g.DrawString(precioDisplay, normal, Brushes.Black, new RectangleF(xPrecio, baseLineY, precioWidth, normal.GetHeight(g)), new StringFormat { Alignment = StringAlignment.Center });

                    // Subtotal - mostrar con signo negativo en nota de crédito
                    var subDisplay = isAnulada ? $"-$ {Math.Abs(sub):N0}" : $"$ {sub:N0}";
                    g.DrawString(subDisplay, normal, Brushes.Black, new RectangleF(xSub, baseLineY, subWidth, normal.GetHeight(g)), new StringFormat { Alignment = StringAlignment.Far });

                    // Avanzar Y por la altura usada por el producto + espacio
                    y += (int)prodHeight + 6;
                }

                // Separador antes de totales
                y += 6;
                g.DrawLine(Pens.Gray, e.MarginBounds.Left, y, e.MarginBounds.Right, y);
                y += 8;

                // Totales (alineados a la derecha) - usando más espacio
                var total = _venta.Total > 0 ? _venta.Total : _venta.Detalles?.Sum(d => GetDecimal(d, "Subtotal", "SubTotal", "Importe", "TotalLinea")) ?? 0m;
                // Si la venta está anulada invertimos el signo para la impresión
                var totalToPrint = isAnulada ? -Math.Abs(total) : total;
                var propina = 0m; 
                var subtotalCalc = totalToPrint - propina;

                float rightColX = e.MarginBounds.Right - 240; // Más espacio para totales
                float totalColWidth = 240;

                g.DrawString("Subtotal:", bold, Brushes.Black, rightColX, y);
                var subtotalDisplay = isAnulada ? $"-$ {Math.Abs(subtotalCalc):N0}" : $"$ {subtotalCalc:N0}";
                g.DrawString(subtotalDisplay, bold, Brushes.Black, new RectangleF(rightColX + 80, y, totalColWidth - 80, 16), new StringFormat { Alignment = StringAlignment.Far });
                y += 18;

                if (propina > 0)
                {
                    g.DrawString("Propina:", normal, Brushes.Black, rightColX, y);
                    var propinaDisplay = isAnulada ? $"-$ {Math.Abs(propina):N0}" : $"$ {propina:N0}";
                    g.DrawString(propinaDisplay, normal, Brushes.Black, new RectangleF(rightColX + 80, y, totalColWidth - 80, 14), new StringFormat { Alignment = StringAlignment.Far });
                    y += 16;
                }

                var totalFont = new Font("Segoe UI", 12f, FontStyle.Bold); // Fuente más grande para el total
                g.DrawString("TOTAL:", totalFont, isAnulada ? Brushes.DarkRed : Brushes.Black, rightColX, y);
                var totalDisplay = isAnulada ? $"-$ {Math.Abs(totalToPrint):N0}" : $"$ {totalToPrint:N0}";
                g.DrawString(totalDisplay, totalFont, isAnulada ? Brushes.DarkRed : Brushes.Black, new RectangleF(rightColX + 80, y, totalColWidth - 80, 18), new StringFormat { Alignment = StringAlignment.Far });
                y += 30;

                // Pie opcional con notas y datos legales - mejor uso del espacio
                var note = isAnulada ? "Nota de crédito emitida por anulación. Conservá este comprobante como constancia." : "Gracias por su compra. Conservá este comprobante como constancia.";
                g.DrawString(note, small, Brushes.Gray, e.MarginBounds.Left, y);
                y += 16;
                g.DrawString($"Dirección: {companyAddress} | {companyPhone} | {companyEmail}", small, Brushes.Gray, e.MarginBounds.Left, y);
            };

            using var preview = new PrintPreviewDialog { Document = pd, Width = 900, Height = 700, StartPosition = FormStartPosition.CenterParent };

            // Al mostrarse la vista previa, reemplazamos el botón de impresora por uno propio
            preview.Shown += (s, e) =>
            {
                try
                {
                    foreach (Control c in preview.Controls)
                    {
                        if (c is ToolStrip ts)
                        {
                            // Buscar el item de imprimir por propiedades comunes (tooltip/nombre/texto)
                            var found = ts.Items.Cast<ToolStripItem>().FirstOrDefault(it =>
                                (it.ToolTipText != null && (it.ToolTipText.ToLower().Contains("print") || it.ToolTipText.ToLower().Contains("imprimir"))) ||
                                (it.Name != null && it.Name.ToLower().Contains("print")) ||
                                (it.Text != null && it.Text.ToLower().Contains("imprimir") || it.Text.ToLower().Contains("print"))
                            );

                            if (found != null)
                            {
                                int idx = ts.Items.IndexOf(found);
                                // ocultar botón original
                                found.Visible = false;

                                // crear botón propio (misma imagen/tooltip) y manejar el Click
                                var myPrint = new ToolStripButton
                                {
                                    Image = found.Image,
                                    DisplayStyle = ToolStripItemDisplayStyle.Image,
                                    ToolTipText = found.ToolTipText ?? "Imprimir"
                                };

                                myPrint.Click += (_, __) =>
                                {
                                    // Al apretar el icono: validar impresoras y pedir confirmación/selección
                                    try
                                    {
                                        var installed = PrinterSettings.InstalledPrinters;
                                        if (installed == null || installed.Count == 0)
                                        {
                                            MessageBox.Show("No se detectaron impresoras instaladas. Conecta una impresora o configura una impresora virtual (PDF) para poder imprimir.", "Imprimir", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                            return;
                                        }
                                    }
                                    catch
                                    {
                                        MessageBox.Show("No fue posible comprobar las impresoras instaladas.", "Imprimir", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                        return;
                                    }

                                    using var pdialog = new PrintDialog { Document = pd };
                                    if (pdialog.ShowDialog(this) == DialogResult.OK)
                                    {
                                        try
                                        {
                                            pd.Print();
                                            MessageBox.Show("Impresión enviada correctamente.", "Imprimir", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                        catch (Exception exPrint)
                                        {
                                            MessageBox.Show($"Error durante la impresión: {exPrint.Message}", "Imprimir", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }
                                    }
                                };

                                ts.Items.Insert(idx, myPrint);
                                break;
                            }
                        }
                    }
                }
                catch
                {
                    // No fallar la vista previa si algo falla al intentar customizar la barra.
                }
            };

            try
            {
                preview.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar la vista previa o la impresión: {ex.Message}", "Imprimir", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private const int ExtraWidth = 160;
 
        private void AplicarAnchoPreferido()
        {
            // 1. Ensanchar el diálogo
            this.Width += ExtraWidth;
          

            // 2. Configurar la grilla para que no auto-ajuste las columnas y aproveche el espacio
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
           

            // 3. Forzar ancho mínimo de columnas monetarias
            SetColumnWidth("PrecioUnitario", 130);
            SetColumnWidth("Subtotal", 120);

            // 4. Asegurar que los anchos se reapliquen si el DataSource cambia
            grid.DataBindingComplete -= OnGridDataBindingComplete; // Evitar suscripciones múltiples
            grid.DataBindingComplete += OnGridDataBindingComplete;
        }

        private void OnGridDataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            SetColumnWidth("PrecioUnitario", 130);
            SetColumnWidth("Subtotal", 120);
        }

        private void SetColumnWidth(string columnName, int width)
        {
            var col = grid.Columns.Cast<DataGridViewColumn>().FirstOrDefault(c => c.DataPropertyName == columnName);
            if (col == null) return;
            col.MinimumWidth = width;
            col.Width = Math.Max(col.Width, width);
        }
    }
}
