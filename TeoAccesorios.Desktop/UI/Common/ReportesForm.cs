using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;


using DrawingColor = System.Drawing.Color;
using PdfDocument = QuestPDF.Fluent.Document;

namespace TeoAccesorios.Desktop
{
    public class ReportesForm : Form
    {
        private readonly DateTimePicker dpDesde = new() { Value = DateTime.Today.AddDays(-7), Width = 120 };
        private readonly DateTimePicker dpHasta = new() { Value = DateTime.Today, Width = 120 };

        private readonly ComboBox cboVendedor = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
        private readonly ComboBox cboCliente = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
        private readonly CheckBox chkExcluirAnuladas = new() { Text = "Excluir anuladas", Checked = true, AutoSize = true };

        private readonly Button btnExport = new() { Text = "Exportar" };

        private readonly Label kpiIngresos = new();
        private readonly Label kpiVentas = new();
        private readonly Label kpiClientes = new();
        private readonly Label kpiProductos = new();

        private readonly DataGridView grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };

        private List<Models.Venta> _ventasFiltradas = new();

        public ReportesForm()
        {
            Text = "Reportes";

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var filtros = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(8), AutoSize = false, Height = 56 };
            filtros.Controls.Add(new Label { Text = "Desde:", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(0, 8, 0, 0) });
            filtros.Controls.Add(dpDesde);
            filtros.Controls.Add(new Label { Text = "Hasta:", AutoSize = true, Padding = new Padding(6, 8, 0, 0) });
            filtros.Controls.Add(dpHasta);
            filtros.Controls.Add(new Label { Text = "Vendedor:", AutoSize = true, Padding = new Padding(10, 8, 0, 0) }); filtros.Controls.Add(cboVendedor);
            filtros.Controls.Add(new Label { Text = "Cliente:", AutoSize = true, Padding = new Padding(10, 8, 0, 0) }); filtros.Controls.Add(cboCliente);
            filtros.Controls.Add(chkExcluirAnuladas);
            filtros.Controls.Add(btnExport);

            // Combos
            cboVendedor.Items.Add("Todos");
            IEnumerable<string> vendedores;
            try
            {
                vendedores = Repository.ListarUsuarios().Where(u => u.Activo).Select(u => u.NombreUsuario).Distinct();
            }
            catch
            {
                vendedores = Repository.ListarVentas(true).Select(v => v.Vendedor).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct();
            }
            foreach (var nombre in vendedores) cboVendedor.Items.Add(nombre);
            cboVendedor.SelectedIndex = 0;

            cboCliente.Items.Add("Todos");
            foreach (var c in Repository.Clientes) cboCliente.Items.Add(c.Nombre);
            cboCliente.SelectedIndex = 0;

            var kpis = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, Padding = new Padding(12) };
            for (int i = 0; i < 4; i++) kpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            kpis.Controls.Add(Card("Ingresos totales", kpiIngresos), 0, 0);
            kpis.Controls.Add(Card("Cantidad de ventas", kpiVentas), 1, 0);
            kpis.Controls.Add(Card("Clientes (únicos)", kpiClientes), 2, 0);
            kpis.Controls.Add(Card("Productos vendidos", kpiProductos), 3, 0);

            //  Doble clic: abrir detalle de la venta
            grid.CellDoubleClick += (_, e) =>
            {
                if (e.RowIndex < 0) return;

                var cell = grid.Rows[e.RowIndex].Cells["IdVenta"];
                if (cell == null) return;

                if (int.TryParse(cell.Value?.ToString(), out int id))
                {
                    
                    var venta = _ventasFiltradas.FirstOrDefault(x => x.Id == id)
                                ?? Repository.ListarVentas(true).FirstOrDefault(x => x.Id == id);

                    if (venta == null) return;

                    var cliente = Repository.Clientes.FirstOrDefault(c => c.Id == venta.ClienteId);
                    new VentaDetalleForm(venta, cliente).ShowDialog(this);
                }
            };

            root.Controls.Add(filtros, 0, 0);
            root.Controls.Add(kpis, 0, 1);
            root.Controls.Add(grid, 0, 2);
            Controls.Add(root);

            // Eventos para aplicar filtros automáticamente
            dpDesde.ValueChanged += (_, __) => LoadData();
            dpHasta.ValueChanged += (_, __) => LoadData();
            cboVendedor.SelectedIndexChanged += (_, __) => LoadData();
            cboCliente.SelectedIndexChanged += (_, __) => LoadData();
            chkExcluirAnuladas.CheckedChanged += (_, __) => LoadData();

            btnExport.Click += (_, __) => Exportar();

            GridHelper.Estilizar(grid);
            GridHelperLock.SoloLectura(grid);
            GridHelperLock.WireDataBindingLock(grid);
            LoadData();

            QuestPDF.Settings.License = LicenseType.Community;
        }

        private Control Card(string titulo, Label valueLabel)
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = DrawingColor.FromArgb(18, 22, 35), Padding = new Padding(10) };
            var t = new Label { Text = titulo, ForeColor = DrawingColor.Gainsboro, Dock = DockStyle.Top, Height = 22 };
            valueLabel.Text = "-";
            valueLabel.ForeColor = DrawingColor.White;
            valueLabel.Dock = DockStyle.Fill;
            valueLabel.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            valueLabel.TextAlign = ContentAlignment.MiddleLeft;
            panel.Controls.Add(valueLabel); panel.Controls.Add(t);
            return panel;
        }

        private (DateTime start, DateTime end) GetRange()
        {
            var s = dpDesde.Value.Date;
            var e = dpHasta.Value.Date.AddDays(1);                    
            return (s, e);
        }

        // Texto amigable del período a mostrar en el reporte
        private string GetPeriodoTexto()
        {
            var (start, endExcl) = GetRange();
            var endIncl = endExcl.AddDays(-1); 
            return $"Rango: {start:dd/MM/yyyy} - {endIncl:dd/MM/yyyy}";
        }

        private void LoadData()
        {
            var (start, end) = GetRange();

            var q = Repository.ListarVentas(true)
                .Where(v => v.FechaVenta >= start && v.FechaVenta < end);

            if (chkExcluirAnuladas.Checked)
                q = q.Where(v => !v.Anulada);

            if (cboVendedor.SelectedIndex > 0)
            {
                var vend = cboVendedor.SelectedItem!.ToString()!;
                q = q.Where(v => string.Equals(v.Vendedor, vend, StringComparison.OrdinalIgnoreCase));
            }
            if (cboCliente.SelectedIndex > 0)
            {
                var cli = cboCliente.SelectedItem!.ToString()!;
                q = q.Where(v => string.Equals(v.ClienteNombre, cli, StringComparison.OrdinalIgnoreCase));
            }

            var detallesCount = q.SelectMany(v => v.Detalles).Sum(d => d.Cantidad);

            var ingresos = q.Sum(v => v.Total);
            var ventas = q.Count();
            var clientesUnicos = q.Select(v => v.ClienteId).Distinct().Count();

            kpiIngresos.Text = "$ " + ingresos.ToString("N0");
            kpiVentas.Text = ventas.ToString();
            kpiClientes.Text = clientesUnicos.ToString();
            kpiProductos.Text = detallesCount.ToString();

            _ventasFiltradas = q.OrderByDescending(v => v.FechaVenta).ToList();

            grid.DataSource = _ventasFiltradas
                .Select(v => new
                {
                    Vendedor = v.Vendedor,
                    Cliente = v.ClienteNombre,
                    IdVenta = v.Id,
                    Fecha = v.FechaVenta.ToString("dd/MM/yyyy HH:mm"),
                    Total = v.Total.ToString("N0")
                })
                .ToList();
        }

        //  EXPORTAR 
        private void Exportar()
        {
            using var dlg = new Form
            {
                Text = "Exportar",
                StartPosition = FormStartPosition.CenterParent,
                Width = 360,
                Height = 170,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var p = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12) };
            var b5 = new Button { Text = "Excel (.xlsx) + resumen", Width = 180, Height = 34 };
            var b6 = new Button { Text = "PDF + resumen", Width = 180, Height = 34 };

            b5.Click += (_, __) => { dlg.Close(); ExportExcelConResumen(); };
            b6.Click += (_, __) => { dlg.Close(); ExportPdfConResumen(); };

            p.Controls.AddRange(new Control[] { b5, b6 });
            dlg.Controls.Add(p);
            dlg.ShowDialog(this);
        }

        //  Excel (.xlsx) con resumen arriba y fila TOTAL al final 
        private void ExportExcelConResumen()
        {
            if (_ventasFiltradas == null || _ventasFiltradas.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = "reporte.xlsx"
            };
            if (sfd.ShowDialog(this) != DialogResult.OK) return;

            var periodo = GetPeriodoTexto();
            var cantidad = _ventasFiltradas.Count;
            var total = _ventasFiltradas.Sum(v => v.Total);

            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Ventas");

            // Resumen arriba
            ws.Cell(1, 1).Value = "Reporte de ventas";
            ws.Cell(1, 1).Style.Font.Bold = true;

            ws.Cell(2, 1).Value = "Período:";
            ws.Cell(2, 2).Value = periodo;

            ws.Cell(3, 1).Value = "Generado:";
            ws.Cell(3, 2).Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            ws.Cell(4, 1).Value = "Cantidad de ventas:";
            ws.Cell(4, 2).Value = cantidad;

            ws.Cell(5, 1).Value = "Total ventas:";
            ws.Cell(5, 2).Value = total;
            ws.Cell(5, 2).Style.NumberFormat.Format = "$ #,##0";

            // Encabezados (a partir de fila 7)
            var row = 7;
            ws.Cell(row, 1).Value = "Vendedor";
            ws.Cell(row, 2).Value = "Cliente";
            ws.Cell(row, 3).Value = "IdVenta";
            ws.Cell(row, 4).Value = "Fecha";
            ws.Cell(row, 5).Value = "Total";
            ws.Range(row, 1, row, 5).Style.Font.Bold = true;
            ws.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.FromHtml("#E9EEF7");
            row++;

            foreach (var v in _ventasFiltradas)
            {
                ws.Cell(row, 1).Value = v.Vendedor;
                ws.Cell(row, 2).Value = v.ClienteNombre;
                ws.Cell(row, 3).Value = v.Id;
                ws.Cell(row, 4).Value = v.FechaVenta;
                ws.Cell(row, 4).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                ws.Cell(row, 5).Value = v.Total;
                ws.Cell(row, 5).Style.NumberFormat.Format = "$ #,##0";
                row++;
            }

            // Fila TOTAL
            ws.Cell(row, 1).Value = "TOTAL";
            ws.Range(row, 1, row, 4).Merge();
            ws.Range(row, 1, row, 4).Style.Font.Bold = true;
            ws.Cell(row, 5).FormulaA1 = $"SUM(E8:E{row - 1})";
            ws.Cell(row, 5).Style.NumberFormat.Format = "$ #,##0";
            ws.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.FromHtml("#F6F6F6");
            ws.Range(7, 1, row, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(7, 1, row, 5).Style.Border.InsideBorder = XLBorderStyleValues.Dotted;

            ws.Columns().AdjustToContents();

            wb.SaveAs(sfd.FileName);
            MessageBox.Show("Excel generado correctamente.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // PDF con resumen arriba 
        private void ExportPdfConResumen()
        {
            if (_ventasFiltradas == null || _ventasFiltradas.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "PDF (*.pdf)|*.pdf",
                FileName = "reporte.pdf"
            };
            if (sfd.ShowDialog(this) != DialogResult.OK) return;

            var periodo = GetPeriodoTexto();
            var cantidad = _ventasFiltradas.Count;
            var total = _ventasFiltradas.Sum(v => v.Total);

            var ventas = _ventasFiltradas
                .Select(v => new
                {
                    v.Vendedor,
                    v.ClienteNombre,
                    v.Id,
                    Fecha = v.FechaVenta.ToString("dd/MM/yyyy HH:mm"),
                    v.Total
                })
                .ToList();

            var doc = PdfDocument.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Header().Column(col =>
                    {
                        col.Item().Text("Reporte de ventas").FontSize(16).SemiBold();
                        col.Item().Text(text => { text.Span("Período: ").SemiBold(); text.Span(periodo); });
                        col.Item().Text(text => { text.Span("Generado: ").SemiBold(); text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")); });
                        col.Item().Text(text => { text.Span("Cantidad de ventas: ").SemiBold(); text.Span(cantidad.ToString()); });
                        col.Item().Text(text => { text.Span("Total: ").SemiBold(); text.Span("$ " + total.ToString("N0")); });
                    });

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(2);   
                            cols.RelativeColumn(2);   
                            cols.RelativeColumn(1);   
                            cols.RelativeColumn(2);   
                            cols.RelativeColumn(1);   
                        });

                        table.Header(h =>
                        {
                            h.Cell().Element(CellHeader).Text("Vendedor");
                            h.Cell().Element(CellHeader).Text("Cliente");
                            h.Cell().Element(CellHeader).Text("IdVenta");
                            h.Cell().Element(CellHeader).Text("Fecha");
                            h.Cell().Element(CellHeader).Text("Total");

                            static IContainer CellHeader(IContainer container) =>
                                container.DefaultTextStyle(x => x.SemiBold())
                                         .PaddingVertical(6)
                                         .Background("#E9EEF7")
                                         .BorderBottom(1)
                                         .BorderColor("#B8C2D3");
                        });

                        foreach (var r in ventas)
                        {
                            table.Cell().Element(CellBody).Text(r.Vendedor ?? "");
                            table.Cell().Element(CellBody).Text(r.ClienteNombre ?? "");
                            table.Cell().Element(CellBody).Text(r.Id.ToString());
                            table.Cell().Element(CellBody).Text(r.Fecha);
                            table.Cell().Element(CellBody).AlignRight().Text("$ " + r.Total.ToString("N0"));

                            static IContainer CellBody(IContainer container) =>
                                container.PaddingVertical(3).BorderBottom(0.5f).BorderColor("#EEEEEE");
                        }
                    });

                    page.Footer().AlignRight().Text(x =>
                    {
                        x.Span("Página ").SemiBold();
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            });

            using var stream = new FileStream(sfd.FileName, FileMode.Create);
            doc.GeneratePdf(stream);

            MessageBox.Show("PDF generado correctamente.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
