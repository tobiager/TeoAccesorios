using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;
using TeoAccesorios.Desktop.UI.Common;

namespace TeoAccesorios.Desktop
{
    public class EstadisticasForm : Form
    {
        // --- Filtros ---
        private readonly DateTimePicker dpDesde = new() { Value = DateTime.Today.AddDays(-30), Width = 120 };
        private readonly DateTimePicker dpHasta = new() { Value = DateTime.Today, Width = 120 };
        private readonly ComboBox cboVendedor = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
        private readonly ComboBox cboCliente = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
        private readonly ComboBox cboCategoria = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
        private readonly ComboBox cboProvincia = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
        private readonly ComboBox cboLocalidad = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
        private readonly Button btnExportar = new() { Text = "Exportar", AutoSize = true };

        // --- Controles de comparación ---
        private readonly CheckBox chkComparar = new() { Text = "Comparar rangos", AutoSize = true };
        private readonly DateTimePicker dpDesdeComparacion = new() { Value = DateTime.Today.AddDays(-60), Width = 120, Enabled = false };
        private readonly DateTimePicker dpHastaComparacion = new() { Value = DateTime.Today.AddDays(-31), Width = 120, Enabled = false };

        // --- Contenedores de datos ---
        private readonly DataGridView _gridTopProductos = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false };
        private readonly DataGridView _gridTopClientes = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false };

        // --- Gráficos ---
        private readonly Panel _panelGraficos = new() { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle };

        // --- Datos cacheados ---
        private List<Venta> _ventasFiltradas = new();
        private List<Venta> _ventasComparacion = new();
        private readonly CultureInfo _culture = new("es-AR");

        public EstadisticasForm()
        {
            Text = "Estadísticas y Tops";
            WindowState = FormWindowState.Maximized;
            QuestPDF.Settings.License = LicenseType.Community;

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Filtros
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 60)); // Tops
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 40)); // Gráficos

            // --- Panel de Filtros ---
            var pnlFiltros = new FlowLayoutPanel { 
                Dock = DockStyle.Fill, 
                Padding = new Padding(8), 
                AutoSize = true, 
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight
            };

            // Primera fila de filtros
            pnlFiltros.Controls.AddRange(new Control[] {
                new Label { Text = "Desde:", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(0, 8, 5, 0) },
                dpDesde,
                new Label { Text = "Hasta:", AutoSize = true, Padding = new Padding(10, 8, 5, 0) },
                dpHasta,
                new Label { Text = "Vendedor:", AutoSize = true, Padding = new Padding(10, 8, 5, 0) },
                cboVendedor,
                new Label { Text = "Cliente:", AutoSize = true, Padding = new Padding(10, 8, 5, 0) },
                cboCliente,
                btnExportar
            });

            // Separador
            pnlFiltros.Controls.Add(new Panel { Width = pnlFiltros.Width, Height = 1 });

            // Segunda fila de filtros
            pnlFiltros.Controls.AddRange(new Control[] {
                new Label { Text = "Categoría:", AutoSize = true, Padding = new Padding(0, 8, 5, 0) },
                cboCategoria,
                new Label { Text = "Provincia:", AutoSize = true, Padding = new Padding(10, 8, 5, 0) },
                cboProvincia,
                new Label { Text = "Localidad:", AutoSize = true, Padding = new Padding(10, 8, 5, 0) },
                cboLocalidad
            });

            // Separador
            pnlFiltros.Controls.Add(new Panel { Width = pnlFiltros.Width, Height = 1 });

            // Tercera fila - Comparación
            pnlFiltros.Controls.AddRange(new Control[] {
                chkComparar,
                new Label { Text = "Desde (comp):", AutoSize = true, Padding = new Padding(10, 8, 5, 0) },
                dpDesdeComparacion,
                new Label { Text = "Hasta (comp):", AutoSize = true, Padding = new Padding(10, 8, 5, 0) },
                dpHastaComparacion
            });

            // --- Contenido Principal (Tops) ---
            var pnlTops = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(8) };
            pnlTops.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            pnlTops.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            // Panel Izquierdo: Top Productos
            var pnlLeft = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            pnlLeft.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pnlLeft.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            pnlLeft.Controls.Add(new Label { 
                Text = "Top Productos más vendidos", 
                Font = new Font("Segoe UI", 11, FontStyle.Bold), 
                AutoSize = true, 
                Padding = new Padding(0, 0, 0, 6) 
            }, 0, 0);
            pnlLeft.Controls.Add(_gridTopProductos, 0, 1);

            // Panel Derecho: Top Clientes
            var pnlRight = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            pnlRight.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pnlRight.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            pnlRight.Controls.Add(new Label { 
                Text = "Top Clientes (por monto)", 
                Font = new Font("Segoe UI", 11, FontStyle.Bold), 
                AutoSize = true, 
                Padding = new Padding(0, 0, 0, 6) 
            }, 0, 0);
            pnlRight.Controls.Add(_gridTopClientes, 0, 1);

            pnlTops.Controls.Add(pnlLeft, 0, 0);
            pnlTops.Controls.Add(pnlRight, 1, 0);

            // --- Sección de Gráficos ---
            var pnlGraficosContainer = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, Padding = new Padding(8) };
            pnlGraficosContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pnlGraficosContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Controles de gráficos - solo título
            var pnlControlesGraficos = new FlowLayoutPanel { 
                Dock = DockStyle.Fill, 
                AutoSize = true, 
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 0, 0, 8)
            };

            pnlControlesGraficos.Controls.Add(
                new Label { Text = "Gráficos por Rango de Fechas", Font = new Font("Segoe UI", 11, FontStyle.Bold), AutoSize = true, Padding = new Padding(0, 5, 15, 0) }
            );

            pnlGraficosContainer.Controls.Add(pnlControlesGraficos, 0, 0);
            pnlGraficosContainer.Controls.Add(_panelGraficos, 0, 1);

            root.Controls.Add(pnlFiltros, 0, 0);
            root.Controls.Add(pnlTops, 0, 1);
            root.Controls.Add(pnlGraficosContainer, 0, 2);
            Controls.Add(root);

            // --- Configuración e inicialización ---
            SetupGrids();
            SetupGraficos();
            CargarCombos();
            
            // Eventos automáticos
            dpDesde.ValueChanged += (_, __) => CargarDatos();
            dpHasta.ValueChanged += (_, __) => CargarDatos();
            cboVendedor.SelectedIndexChanged += (_, __) => CargarDatos();
            cboCliente.SelectedIndexChanged += (_, __) => CargarDatos();
            cboCategoria.SelectedIndexChanged += (_, __) => CargarDatos();
            cboProvincia.SelectedIndexChanged += (_, __) => { CargarLocalidades(); CargarDatos(); };
            cboLocalidad.SelectedIndexChanged += (_, __) => CargarDatos();
            
            // Eventos de comparación
            chkComparar.CheckedChanged += (_, __) => { ActualizarEstadoComparacion(); CargarDatos(); };
            dpDesdeComparacion.ValueChanged += (_, __) => { if (chkComparar.Checked) CargarDatos(); };
            dpHastaComparacion.ValueChanged += (_, __) => { if (chkComparar.Checked) CargarDatos(); };

            btnExportar.Click += (_, __) => Exportar();

            CargarDatos();
        }

        private void SetupGrids()
        {
            GridHelper.Estilizar(_gridTopProductos);
            _gridTopProductos.Columns.AddRange(
                new DataGridViewTextBoxColumn { HeaderText = "Producto", DataPropertyName = "Nombre", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 60 },
                new DataGridViewTextBoxColumn { HeaderText = "Cantidad", DataPropertyName = "Cantidad", Width = 80, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } },
                new DataGridViewTextBoxColumn { HeaderText = "Monto Total", DataPropertyName = "Monto", Width = 120, DefaultCellStyle = { Format = "C2", Alignment = DataGridViewContentAlignment.MiddleRight } }
            );

            GridHelper.Estilizar(_gridTopClientes);
            _gridTopClientes.Columns.AddRange(
                new DataGridViewTextBoxColumn { HeaderText = "Cliente", DataPropertyName = "Nombre", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 60 },
                new DataGridViewTextBoxColumn { HeaderText = "Compras", DataPropertyName = "Cantidad", Width = 80, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } },
                new DataGridViewTextBoxColumn { HeaderText = "Monto Total", DataPropertyName = "Monto", Width = 120, DefaultCellStyle = { Format = "C2", Alignment = DataGridViewContentAlignment.MiddleRight } }
            );
        }

        private void SetupGraficos()
        {
            _panelGraficos.BackColor = System.Drawing.Color.White;
            _panelGraficos.Paint += PanelGraficos_Paint;
        }

        private void ActualizarEstadoComparacion()
        {
            bool habilitar = chkComparar.Checked;
            dpDesdeComparacion.Enabled = habilitar;
            dpHastaComparacion.Enabled = habilitar;
        }

        private void CargarCombos()
        {
            // Vendedores con opción "Todos"
            var vendedores = Repository.ListarUsuarios()
                .Where(u => u.Activo)
                .Select(u => u.NombreUsuario)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s)
                .ToList();
            vendedores.Insert(0, "Todos");
            FormUtils.BindCombo(cboVendedor, vendedores, "Todos");

            // Clientes con opción "Todos"
            var clientes = Repository.Clientes
                .Select(c => c.Nombre)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s)
                .ToList();
            clientes.Insert(0, "Todos");
            FormUtils.BindCombo(cboCliente, clientes, "Todos");

            // Categorías con opción "Todas"
            var categorias = Repository.ListarCategorias(true)
                .Select(c => c.Nombre)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s)
                .ToList();
            categorias.Insert(0, "Todas");
            FormUtils.BindCombo(cboCategoria, categorias, "Todas");

            // Provincias con opción "Todas"
            var provincias = Repository.ListarProvincias(true).ToList();
            provincias.Insert(0, new Provincia { Id = 0, Nombre = "Todas" });
            FormUtils.BindCombo(cboProvincia, provincias, selectedValue: 0);

            CargarLocalidades();
        }

        private void CargarLocalidades()
        {
            if (cboProvincia.SelectedIndex == 0 || cboProvincia.SelectedValue == null || (int)cboProvincia.SelectedValue == 0)
    {
        // "Todas" las provincias seleccionadas
        var todasLocalidades = Repository.ListarLocalidades(null, true).ToList();
        todasLocalidades.Insert(0, new Localidad { Id = 0, Nombre = "Todas" });
        FormUtils.BindCombo(cboLocalidad, todasLocalidades, selectedValue: 0);
    }
    else if (cboProvincia.SelectedValue is int provinciaId && provinciaId > 0)
    {
        var localidades = Repository.ListarLocalidades(provinciaId, true).ToList();
        localidades.Insert(0, new Localidad { Id = 0, Nombre = "Todas" });
        FormUtils.BindCombo(cboLocalidad, localidades, selectedValue: 0);
    }
        }

        private void CargarDatos()
        {
            // Cargar datos del rango principal
            _ventasFiltradas = CargarVentasPorRango(dpDesde.Value, dpHasta.Value);

            // Cargar datos de comparación si está habilitado
            if (chkComparar.Checked)
            {
                _ventasComparacion = CargarVentasPorRango(dpDesdeComparacion.Value, dpHastaComparacion.Value);
            }
            else
            {
                _ventasComparacion.Clear();
            }

            ActualizarVistas();
            ActualizarGrafico();
        }

        private List<Venta> CargarVentasPorRango(DateTime desde, DateTime hasta)
        {
            var start = desde.Date;
            var end = hasta.Date.AddDays(1);

            var q = Repository.ListarVentas(true)
                .Where(v => v.FechaVenta >= start && v.FechaVenta < end && !v.Anulada);

            // Aplicar filtros
            if (cboVendedor.SelectedIndex > 0)
            {
                var vend = cboVendedor.SelectedItem!.ToString();
                q = q.Where(v => string.Equals(v.Vendedor, vend, StringComparison.OrdinalIgnoreCase));
            }

            if (cboCliente.SelectedIndex > 0)
            {
                var cliente = cboCliente.SelectedItem!.ToString();
                q = q.Where(v => string.Equals(v.ClienteNombre, cliente, StringComparison.OrdinalIgnoreCase));
            }

            // Filtrar por localidad/provincia
            if (cboLocalidad.SelectedIndex > 0 && cboLocalidad.SelectedValue is int localidadId)
            {
                q = q.Where(v => v.LocalidadId == localidadId);
            }
            else if (cboProvincia.SelectedIndex > 0 && cboProvincia.SelectedValue is int provinciaId)
            {
                var localidadesEnProvincia = Repository.ListarLocalidades(provinciaId, true)
                    .Select(l => l.Id)
                    .ToHashSet();
                q = q.Where(v => v.LocalidadId.HasValue && localidadesEnProvincia.Contains(v.LocalidadId.Value));
            }

            var ventas = q.ToList();

            // Filtrar por categoría (requiere inspeccionar detalles)
            if (cboCategoria.SelectedIndex > 0)
            {
                var catNombre = cboCategoria.SelectedItem!.ToString();
                var productosEnCategoria = Repository.ListarProductos(true)
                    .Where(p => string.Equals(p.CategoriaNombre, catNombre, StringComparison.OrdinalIgnoreCase))
                    .Select(p => p.Id)
                    .ToHashSet();

                ventas = ventas
                    .Where(v => v.Detalles.Any(d => productosEnCategoria.Contains(d.ProductoId)))
                    .ToList();
            }

            return ventas;
        }

        private void ActualizarVistas()
        {
            // --- Top Productos ---
            var topProductos = _ventasFiltradas
                .SelectMany(v => v.Detalles)
                .GroupBy(d => d.ProductoNombre)
                .Select(g => new
                {
                    Nombre = g.Key,
                    Cantidad = g.Sum(d => d.Cantidad),
                    Monto = g.Sum(d => d.Subtotal)
                })
                .OrderByDescending(x => x.Monto)
                .Take(15)
                .ToList();

            _gridTopProductos.DataSource = topProductos;

            // --- Top Clientes ---
            var topClientes = _ventasFiltradas
                .GroupBy(v => v.ClienteNombre)
                .Select(g => new
                {
                    Nombre = g.Key,
                    Cantidad = g.Count(),
                    Monto = g.Sum(v => v.Total)
                })
                .OrderByDescending(x => x.Monto)
                .Take(15)
                .ToList();

            _gridTopClientes.DataSource = topClientes;
        }

        private void ActualizarGrafico()
        {
            _panelGraficos.Invalidate();
        }

        private void PanelGraficos_Paint(object? sender, PaintEventArgs e)
        {
            if (_ventasFiltradas == null || !_ventasFiltradas.Any())
            {
                DrawEmptyMessage(e.Graphics);
                return;
            }

            if (chkComparar.Checked && _ventasComparacion.Any())
            {
                DrawComparacion(e.Graphics);
            }
            else
            {
                DrawVentasPorDia(e.Graphics);
            }
        }

        private void DrawEmptyMessage(Graphics g)
        {
            var rect = _panelGraficos.ClientRectangle;
            var message = "No hay datos para mostrar";
            var font = new Font("Segoe UI", 12);
            var size = g.MeasureString(message, font);
            var x = (rect.Width - size.Width) / 2;
            var y = (rect.Height - size.Height) / 2;
            
            g.DrawString(message, font, Brushes.Gray, x, y);
        }

        private void DrawVentasPorDia(Graphics g)
        {
            var rect = _panelGraficos.ClientRectangle;
            var margin = 60;
            var graphRect = new Rectangle(margin, margin, rect.Width - 2 * margin, rect.Height - 2 * margin);

            // Agrupar ventas por día
            var ventasPorDia = _ventasFiltradas
                .GroupBy(v => v.FechaVenta.Date)
                .Select(g => new { Fecha = g.Key, Total = g.Sum(v => v.Total) })
                .OrderBy(x => x.Fecha)
                .ToList();

            if (!ventasPorDia.Any()) return;

            // Título
            var title = $"Ventas por Día ({dpDesde.Value:dd/MM/yyyy} - {dpHasta.Value:dd/MM/yyyy})";
            var titleFont = new Font("Segoe UI", 14, FontStyle.Bold);
            var titleSize = g.MeasureString(title, titleFont);
            g.DrawString(title, titleFont, Brushes.Black, (rect.Width - titleSize.Width) / 2, 10);

            // Dibujar ejes
            g.DrawLine(Pens.Black, margin, rect.Height - margin, rect.Width - margin, rect.Height - margin); // X
            g.DrawLine(Pens.Black, margin, margin, margin, rect.Height - margin); // Y

            // Calcular escalas
            var maxMonto = ventasPorDia.Max(x => x.Total);
            var stepX = graphRect.Width / (float)Math.Max(1, ventasPorDia.Count - 1);

            // Dibujar puntos y líneas
            var points = new List<PointF>();
            for (int i = 0; i < ventasPorDia.Count; i++)
            {
                var x = margin + i * stepX;
                var y = rect.Height - margin - (graphRect.Height * (float)(ventasPorDia[i].Total / maxMonto));
                points.Add(new PointF(x, y));
            }

            // Dibujar línea
            if (points.Count > 1)
            {
                g.DrawLines(new Pen(System.Drawing.Color.Blue, 2), points.ToArray());
            }

            // Dibujar puntos
            foreach (var point in points)
            {
                g.FillEllipse(Brushes.Blue, point.X - 3, point.Y - 3, 6, 6);
            }

            // Etiquetas de valores en Y
            var valueFont = new Font("Segoe UI", 8);
            for (int i = 0; i <= 5; i++)
            {
                var value = maxMonto * i / 5;
                var yPos = rect.Height - margin - (graphRect.Height * i / 5);
                g.DrawString($"${value:N0}", valueFont, Brushes.Black, 5, yPos - 6);
            }
        }

        private void DrawComparacion(Graphics g)
        {
            var rect = _panelGraficos.ClientRectangle;
            var margin = 60;
            var graphRect = new Rectangle(margin, margin, rect.Width - 2 * margin, rect.Height - 2 * margin);

            // Agrupar ventas por día para ambos rangos
            var ventasRango1 = _ventasFiltradas
                .GroupBy(v => v.FechaVenta.Date)
                .Select(g => new { Fecha = g.Key, Total = g.Sum(v => v.Total) })
                .OrderBy(x => x.Fecha)
                .ToList();

            var ventasRango2 = _ventasComparacion
                .GroupBy(v => v.FechaVenta.Date)
                .Select(g => new { Fecha = g.Key, Total = g.Sum(v => v.Total) })
                .OrderBy(x => x.Fecha)
                .ToList();

            if (!ventasRango1.Any() && !ventasRango2.Any()) return;

            // Título
            var title = $"Comparación: ({dpDesde.Value:dd/MM/yyyy} - {dpHasta.Value:dd/MM/yyyy}) vs ({dpDesdeComparacion.Value:dd/MM/yyyy} - {dpHastaComparacion.Value:dd/MM/yyyy})";
            var titleFont = new Font("Segoe UI", 12, FontStyle.Bold);
            var titleSize = g.MeasureString(title, titleFont);
            g.DrawString(title, titleFont, Brushes.Black, (rect.Width - titleSize.Width) / 2, 10);

            // Dibujar ejes
            g.DrawLine(Pens.Black, margin, rect.Height - margin, rect.Width - margin, rect.Height - margin); // X
            g.DrawLine(Pens.Black, margin, margin, margin, rect.Height - margin); // Y

            // Calcular escalas
            var maxCount = Math.Max(ventasRango1.Count, ventasRango2.Count);
            var maxMonto = Math.Max(
                ventasRango1.Any() ? ventasRango1.Max(x => x.Total) : 0,
                ventasRango2.Any() ? ventasRango2.Max(x => x.Total) : 0
            );

            if (maxCount > 0)
            {
                var stepX = graphRect.Width / (float)Math.Max(1, maxCount - 1);

                // Dibujar primer rango (azul)
                if (ventasRango1.Any())
                {
                    var points1 = new List<PointF>();
                    for (int i = 0; i < ventasRango1.Count; i++)
                    {
                        var x = margin + i * stepX;
                        var y = rect.Height - margin - (graphRect.Height * (float)(ventasRango1[i].Total / maxMonto));
                        points1.Add(new PointF(x, y));
                    }

                    if (points1.Count > 1)
                        g.DrawLines(new Pen(System.Drawing.Color.Blue, 2), points1.ToArray());

                    foreach (var point in points1)
                        g.FillEllipse(Brushes.Blue, point.X - 3, point.Y - 3, 6, 6);
                }

                // Dibujar segundo rango (rojo)
                if (ventasRango2.Any())
                {
                    var points2 = new List<PointF>();
                    for (int i = 0; i < ventasRango2.Count; i++)
                    {
                        var x = margin + i * stepX;
                        var y = rect.Height - margin - (graphRect.Height * (float)(ventasRango2[i].Total / maxMonto));
                        points2.Add(new PointF(x, y));
                    }

                    if (points2.Count > 1)
                        g.DrawLines(new Pen(System.Drawing.Color.Red, 2), points2.ToArray());

                    foreach (var point in points2)
                        g.FillEllipse(Brushes.Red, point.X - 3, point.Y - 3, 6, 6);
                }

                // Leyenda
                var legendFont = new Font("Segoe UI", 10);
                g.DrawString("Rango 1", legendFont, Brushes.Blue, rect.Width - 120, 40);
                g.DrawString("Rango 2", legendFont, Brushes.Red, rect.Width - 120, 60);

                // Etiquetas de valores en Y
                var valueFont = new Font("Segoe UI", 8);
                for (int i = 0; i <= 5; i++)
                {
                    var value = maxMonto * i / 5;
                    var yPos = rect.Height - margin - (graphRect.Height * i / 5);
                    g.DrawString($"${value:N0}", valueFont, Brushes.Black, 5, yPos - 6);
                }
            }
        }

        private (DateTime start, DateTime end) GetRange()
        {
            var s = dpDesde.Value.Date;
            var e = dpHasta.Value.Date.AddDays(1);
            return (s, e);
        }

        private string GetPeriodoTexto()
        {
            var (start, endExcl) = GetRange();
            var endIncl = endExcl.AddDays(-1);
            return $"Período: {start:dd/MM/yyyy} - {endIncl:dd/MM/yyyy}";
        }

        #region Exportación

        private void Exportar()
        {
            if (!_ventasFiltradas.Any())
            {
                MessageBox.Show("No hay datos para exportar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx|PDF Document (*.pdf)|*.pdf",
                FileName = $"Estadisticas_{DateTime.Now:yyyyMMdd}.xlsx"
            };

            if (sfd.ShowDialog(this) != DialogResult.OK) return;

            if (Path.GetExtension(sfd.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                ExportarPdf(sfd.FileName);
            }
            else
            {
                ExportarExcel(sfd.FileName);
            }
        }

        private void ExportarExcel(string filePath)
        {
            using var wb = new XLWorkbook();

            // Hoja 1: Top Productos
            var wsProd = wb.AddWorksheet("Top Productos");
            var topProductos = (IEnumerable<dynamic>)_gridTopProductos.DataSource;
            if (topProductos != null)
            {
                wsProd.Cell(1, 1).InsertTable(topProductos);
                wsProd.Column(3).Style.NumberFormat.Format = "$ #,##0.00";
                wsProd.Columns().AdjustToContents();
            }

            // Hoja 2: Top Clientes
            var wsCli = wb.AddWorksheet("Top Clientes");
            var topClientes = (IEnumerable<dynamic>)_gridTopClientes.DataSource;
            if (topClientes != null)
            {
                wsCli.Cell(1, 1).InsertTable(topClientes);
                wsCli.Column(3).Style.NumberFormat.Format = "$ #,##0.00";
                wsCli.Columns().AdjustToContents();
            }

            // Hoja 3: Resumen de Filtros
            var wsResumen = wb.AddWorksheet("Resumen");
            wsResumen.Cell(1, 1).Value = "Resumen de Estadísticas";
            wsResumen.Cell(1, 1).Style.Font.Bold = true;
            wsResumen.Cell(2, 1).Value = GetPeriodoTexto();
            wsResumen.Cell(3, 1).Value = $"Total de ventas analizadas: {_ventasFiltradas.Count}";
            wsResumen.Cell(4, 1).Value = $"Monto total: ${_ventasFiltradas.Sum(v => v.Total):N2}";

            if (chkComparar.Checked)
            {
                wsResumen.Cell(6, 1).Value = "Comparación habilitada:";
                wsResumen.Cell(6, 1).Style.Font.Bold = true;
                wsResumen.Cell(7, 1).Value = $"Rango 2: {dpDesdeComparacion.Value:dd/MM/yyyy} - {dpHastaComparacion.Value:dd/MM/yyyy}";
                wsResumen.Cell(8, 1).Value = $"Ventas en rango 2: {_ventasComparacion.Count}";
                wsResumen.Cell(9, 1).Value = $"Monto rango 2: ${_ventasComparacion.Sum(v => v.Total):N2}";
            }

            wb.SaveAs(filePath);
            MessageBox.Show("Excel generado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExportarPdf(string filePath)
        {
            var topProductos = (IEnumerable<dynamic>)_gridTopProductos.DataSource ?? new List<dynamic>();
            var topClientes = (IEnumerable<dynamic>)_gridTopClientes.DataSource ?? new List<dynamic>();

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Reporte de Estadísticas").FontSize(16).SemiBold();
                        col.Item().Text(GetPeriodoTexto());
                        col.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}");
                        
                        var filtrosTexto = $"Filtros: Vendedor={cboVendedor.SelectedItem}, Cliente={cboCliente.SelectedItem}, " +
                                         $"Categoría={cboCategoria.SelectedItem}, Provincia={cboProvincia.SelectedItem}, " +
                                         $"Localidad={cboLocalidad.SelectedItem}";
                        col.Item().Text(filtrosTexto);
                        
                        if (chkComparar.Checked)
                        {
                            col.Item().Text($"Comparando con: {dpDesdeComparacion.Value:dd/MM/yyyy} - {dpHastaComparacion.Value:dd/MM/yyyy}");
                        }
                        
                        col.Spacing(15);
                    });

                    page.Content().Column(col =>
                    {
                        // Tabla Top Productos
                        col.Item().Text("Top Productos más vendidos").Bold().FontSize(12);
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols => { cols.RelativeColumn(3); cols.ConstantColumn(60); cols.ConstantColumn(80); });
                            table.Header(h =>
                            {
                                h.Cell().Element(CellHeader).Text("Producto");
                                h.Cell().Element(CellHeader).AlignCenter().Text("Cantidad");
                                h.Cell().Element(CellHeader).AlignRight().Text("Monto");
                            });
                            foreach (var item in topProductos)
                            {
                                var nombre = (string)item.Nombre;
                                var cantidad = (int)item.Cantidad;
                                var monto = (decimal)item.Monto;

                                table.Cell().Element(CellBody).Text(nombre);
                                table.Cell().Element(CellBody).AlignCenter().Text(cantidad.ToString());
                                table.Cell().Element(CellBody).AlignRight().Text(monto.ToString("C2", _culture));
                            }
                        });

                        col.Spacing(20);

                        // Tabla Top Clientes
                        col.Item().Text("Top Clientes (por monto)").Bold().FontSize(12);
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols => { cols.RelativeColumn(3); cols.ConstantColumn(60); cols.ConstantColumn(80); });
                            table.Header(h =>
                            {
                                h.Cell().Element(CellHeader).Text("Cliente");
                                h.Cell().Element(CellHeader).AlignCenter().Text("Compras");
                                h.Cell().Element(CellHeader).AlignRight().Text("Monto");
                            });
                            foreach (var item in topClientes)
                            {
                                var nombre = (string)item.Nombre;
                                var cantidad = (int)item.Cantidad;
                                var monto = (decimal)item.Monto;

                                table.Cell().Element(CellBody).Text(nombre);
                                table.Cell().Element(CellBody).AlignCenter().Text(cantidad.ToString());
                                table.Cell().Element(CellBody).AlignRight().Text(monto.ToString("C2", _culture));
                            }
                        });
                    });

                    page.Footer().AlignRight().Text(x => { x.Span("Página "); x.CurrentPageNumber(); x.Span(" / "); x.TotalPages(); });
                });
            });

            doc.GeneratePdf(filePath);
            MessageBox.Show("PDF generado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // --- Helpers de Estilo para PDF ---
        private static IContainer CellHeader(IContainer container) =>
            container.DefaultTextStyle(x => x.SemiBold())
                     .PaddingVertical(5)
                     .Background(Colors.Grey.Lighten3)
                     .BorderBottom(1)
                     .BorderColor(Colors.Grey.Lighten1);

        private static IContainer CellBody(IContainer container) =>
            container.BorderBottom(1)
                     .BorderColor(Colors.Grey.Lighten2)
                     .PaddingVertical(5);

        #endregion
    }
}