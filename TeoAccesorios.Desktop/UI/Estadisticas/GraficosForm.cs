using System.Drawing;
using Microsoft.Web.WebView2.Core;
using ClosedXML.Excel;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;
using TeoAccesorios.Desktop.UI.Common;
using Color = System.Drawing.Color;

namespace TeoAccesorios.Desktop.UI.Estadisticas
{
    public class GraficosForm : Form
    {
        // --- Controles de UI ---
        private readonly ComboBox _cboTipoGrafico = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 250 };
        private readonly ComboBox _cboGranularidad = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180, Visible = false };
        private readonly ComboBox _cboEstiloGrafico = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 250 };
        private readonly DateTimePicker _dpDesde1 = new() { 
            Value = DateTime.Today.AddDays(-30), 
            Width = 110,
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "dd/MM/yy"
        };
        private readonly DateTimePicker _dpHasta1 = new() { 
            Value = DateTime.Today, 
            Width = 110,
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "dd/MM/yy"
        };
        private readonly CheckBox _chkComparar = new() { Text = "Comparar con otro período", AutoSize = true };
        private readonly DateTimePicker _dpDesde2 = new() { 
            Value = DateTime.Today.AddDays(-60), 
            Width = 110,
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "dd/MM/yy"
        };
        private readonly DateTimePicker _dpHasta2 = new() { 
            Value = DateTime.Today.AddDays(-31), 
            Width = 110,
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "dd/MM/yy"
        };
        private readonly ComboBox _cboVendedor = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 250 };
        private readonly ComboBox _cboCategoria = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 250 };
        private readonly ComboBox _cboProvincia = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 250 };
        private readonly Button _btnExportar = new() { Text = "📄 Exportar", AutoSize = true, BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Height = 35 };
        private readonly WebView2 _webView = new() { Dock = DockStyle.Fill };

        // --- Panel de fechas del segundo período ---
        private FlowLayoutPanel _panelFechas2;

        // --- Lógica de UI ---
        private bool _isWebViewReady = false;
        private readonly System.Windows.Forms.Timer _debounceTimer = new() { Interval = 400 };
        // --- Datos para exportación ---
        private object? _lastChartData;
        private string _lastChartTitle = string.Empty;

        // --- DTOs y Enums ---
        private record PuntoRanking(string Label, decimal Valor);
        private record PuntoTemporal(DateTime Bucket, decimal Valor);
        private enum GranularidadTemporal { Auto, Dia, Semana, Mes }

        public GraficosForm()
        {
            Text = "📈 Gráficos de Ventas";
            WindowState = FormWindowState.Maximized;
            BackColor = Color.FromArgb(248, 249, 250);

            // Usar el ícono de la aplicación si está disponible
            using (var ms = new MemoryStream())
            {
                Properties.Resources.logo.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;
                Icon = Icon.FromHandle(new Bitmap(ms).GetHicon());
            }

            // Inicializar el panel de fechas del segundo período
            _panelFechas2 = CrearPanelFecha(_dpDesde2, _dpHasta2, esOpcional: true);


            ConfigurarLayout();
            CargarCombos();
            ConfigurarEventos();
            InitializeWebViewAsync();
        }

        private void ConfigurarLayout()
        {
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(10)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // --- Panel de Controles (Izquierda) ---
            var panelControles = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(10),
                BackColor = Color.White
            };

            panelControles.Controls.Add(CrearLabelSeccion("1. Tipo de Gráfico"));
            panelControles.Controls.Add(_cboTipoGrafico);

            // Contenedor para Granularidad
            panelControles.Controls.Add(_cboGranularidad);

            panelControles.Controls.Add(CrearLabelSeccion("2. Estilo Visual"));
            panelControles.Controls.Add(_cboEstiloGrafico);

            panelControles.Controls.Add(CrearLabelSeccion("3. Período Principal"));
            panelControles.Controls.Add(CrearPanelFecha(_dpDesde1, _dpHasta1));

            panelControles.Controls.Add(CrearLabelSeccion("4. Comparación (Opcional)"));
            panelControles.Controls.Add(_chkComparar);
            
            panelControles.Controls.Add(_panelFechas2);

            panelControles.Controls.Add(CrearLabelSeccion("5. Filtros Adicionales"));
            panelControles.Controls.Add(new Label { Text = "Vendedor:", AutoSize = true, Padding = new Padding(0, 5, 0, 0) });
            panelControles.Controls.Add(_cboVendedor);
            panelControles.Controls.Add(new Label { Text = "Categoría:", AutoSize = true, Padding = new Padding(0, 5, 0, 0) });
            panelControles.Controls.Add(_cboCategoria);
            panelControles.Controls.Add(new Label { Text = "Provincia:", AutoSize = true, Padding = new Padding(0, 5, 0, 0) });
            panelControles.Controls.Add(_cboProvincia);

            panelControles.Controls.Add(new Panel { Height = 20 }); // Espaciador
            panelControles.Controls.Add(_btnExportar);

            mainLayout.Controls.Add(panelControles, 0, 0);
            mainLayout.Controls.Add(_webView, 1, 0);

            Controls.Add(mainLayout);
        }

        private Label CrearLabelSeccion(string texto)
        {
            return new Label
            {
                Text = texto,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                AutoSize = true,
                Padding = new Padding(0, 15, 0, 5),
                ForeColor = Color.FromArgb(0, 123, 255)
            };
        }

        private FlowLayoutPanel CrearPanelFecha(DateTimePicker desde, DateTimePicker hasta, bool esOpcional = false)
        {
            var panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Visible = !esOpcional
            };
            panel.Controls.Add(new Label { Text = "Desde:", AutoSize = true, Padding = new Padding(0, 4, 0, 0) });
            panel.Controls.Add(desde);
            panel.Controls.Add(new Label { Text = "Hasta:", AutoSize = true, Padding = new Padding(5, 4, 0, 0) });
            panel.Controls.Add(hasta);
            return panel;
        }

        private void CargarCombos()
        {
            // Tipo de Gráfico
            _cboTipoGrafico.Items.AddRange(new object[] {
                "Ventas por Vendedor",
                "Ventas por Categoría",
                "Ventas por Provincia",
                "Ventas por Producto (Top 15)",
                "Ventas a lo largo del tiempo (por día)"
            });
            _cboTipoGrafico.SelectedIndex = 0;

            // Granularidad
            _cboGranularidad.Items.AddRange(Enum.GetNames(typeof(GranularidadTemporal)));
            _cboGranularidad.SelectedItem = "Auto";

            // Estilo de Gráfico
            _cboEstiloGrafico.Items.AddRange(new object[] { "Barras", "Torta (Pie)", "Líneas", "Dona (Doughnut)" });
            _cboEstiloGrafico.SelectedIndex = 0;

            // Vendedores
            var vendedores = Repository.ListarUsuarios().Where(u => u.Activo).Select(u => u.NombreUsuario).Distinct().OrderBy(n => n).ToList();
            vendedores.Insert(0, "Todos");
            FormUtils.BindCombo(_cboVendedor, vendedores, "Todos");

            // Categorías
            var categorias = Repository.ListarCategorias(true).Select(c => c.Nombre).Distinct().OrderBy(n => n).ToList();
            categorias.Insert(0, "Todas");
            FormUtils.BindCombo(_cboCategoria, categorias, "Todos");

            // Provincias
            var provincias = Repository.ListarProvincias(true).ToList();
            provincias.Insert(0, new Provincia { Id = 0, Nombre = "Todas" });
            FormUtils.BindCombo(_cboProvincia, provincias, selectedValue: 0);
        }

        private void ConfigurarEventos()
        {
            _debounceTimer.Tick += async (s, e) =>
            {
                _debounceTimer.Stop();
                await GenerarGrafico();
            };

            // Eventos que disparan el refresh con debounce
            _cboTipoGrafico.SelectedIndexChanged += (s, e) => { HandleTipoGraficoChange(); ScheduleRefresh(); };
            _cboGranularidad.SelectedIndexChanged += (s, e) => ScheduleRefresh();
            _cboEstiloGrafico.SelectedIndexChanged += (s, e) => ScheduleRefresh();
            _dpDesde1.ValueChanged += (s, e) => ScheduleRefresh();
            _dpHasta1.ValueChanged += (s, e) => ScheduleRefresh();
            _dpDesde2.ValueChanged += (s, e) => ScheduleRefresh();
            _dpHasta2.ValueChanged += (s, e) => ScheduleRefresh();
            _cboVendedor.SelectedIndexChanged += (s, e) => ScheduleRefresh();
            _cboCategoria.SelectedIndexChanged += (s, e) => ScheduleRefresh();
            _cboProvincia.SelectedIndexChanged += (s, e) => ScheduleRefresh();
            _chkComparar.CheckedChanged += (s, e) =>
            {
                // Mostrar/ocultar el panel completo de fechas del segundo período
                _panelFechas2.Visible = _chkComparar.Checked;
                _dpDesde2.Enabled = _chkComparar.Checked;
                _dpHasta2.Enabled = _chkComparar.Checked;
                ScheduleRefresh();
            };
            
            // Asegurarse de que el panel de fechas 2 esté oculto al inicio
            _panelFechas2.Visible = false;


            _btnExportar.Click += async (s, e) => await Exportar();
        }

        private void ScheduleRefresh() { _debounceTimer.Stop(); _debounceTimer.Start(); }

        private async void InitializeWebViewAsync()
        {
            await _webView.EnsureCoreWebView2Async(null);
            _webView.CoreWebView2.NavigationCompleted += async (s, e) =>
            {
                _isWebViewReady = true;
                await GenerarGrafico(); // Generar gráfico inicial
            };
            _webView.CoreWebView2.NavigateToString(GetChartHtmlTemplate());
        }

        private string GetChartHtmlTemplate()
        {
            return @"
                <!DOCTYPE html>
                <html>
                <head>
                    <title>Chart</title>
                    <script src='https://cdn.jsdelivr.net/npm/chart.js@4'></script>
                    <script src='https://cdn.jsdelivr.net/npm/chartjs-adapter-date-fns'></script>
                    <style>
                        body, html { margin: 0; padding: 10px; box-sizing: border-box; background-color: #f8f9fa; font-family: Segoe UI, sans-serif; }
                        #chartContainer { width: 100%; height: 95vh; }
                    </style>
                </head>
                <body>
                    <div id='chartContainer'>
                        <canvas id='myChart'></canvas>
                    </div>
                    <div id='noDataMessage' style='display: none; text-align: center; padding-top: 20%; font-size: 1.2em; color: #6c757d;'>
                        No hay datos para los filtros seleccionados.
                    </div>
                    <script>
                        let myChart;
                        function renderChart(type, data, options) {
                            const ctx = document.getElementById('myChart').getContext('2d');
                            if (myChart) {
                                myChart.destroy();
                            }
                            if (!data || !data.datasets || data.datasets.every(ds => ds.data.length === 0)) {
                                document.getElementById('myChart').style.display = 'none';
                                document.getElementById('noDataMessage').style.display = 'block';
                                return;
                            }
                            document.getElementById('myChart').style.display = 'block';
                            document.getElementById('noDataMessage').style.display = 'none';
                            myChart = new Chart(ctx, {
                                type: type,
                                data: data,
                                options: options
                            });
                        }
                    </script>
                </body>
                </html>";
        }

        private void HandleTipoGraficoChange()
        {
            bool esTemporal = _cboTipoGrafico.SelectedItem?.ToString() == "Ventas a lo largo del tiempo (por día)";
            _cboGranularidad.Visible = esTemporal;
        }

        private async Task GenerarGrafico()
        {
            if (!_isWebViewReady)
            {
                // No mostrar mensaje, simplemente esperar a que esté listo.
                return;
            }

            // --- Validaciones ---
            if (_dpDesde1.Value.Date > _dpHasta1.Value.Date)
            {
                MessageBox.Show("La fecha 'Desde' del período principal no puede ser posterior a la fecha 'Hasta'.", "Rango de Fechas Inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_chkComparar.Checked && _dpDesde2.Value.Date > _dpHasta2.Value.Date)
            {
                MessageBox.Show("La fecha 'Desde' del período de comparación no puede ser posterior a la fecha 'Hasta'.", "Rango de Fechas Inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                this.Cursor = Cursors.WaitCursor;

                string tipoGraficoSeleccionado = _cboTipoGrafico.SelectedItem?.ToString() ?? string.Empty;
                bool esTemporal = tipoGraficoSeleccionado == "Ventas a lo largo del tiempo (por día)";
                string estiloGrafico = _cboEstiloGrafico.SelectedItem?.ToString() ?? "Barras";

                // --- Lógica de validación de estilo de gráfico ---
                if (esTemporal && (estiloGrafico == "Torta (Pie)" || estiloGrafico == "Dona (Doughnut)"))
                {
                    MessageBox.Show("Los gráficos de Torta y Dona no son adecuados para series temporales. Se mostrará un gráfico de Barras en su lugar.", "Estilo de Gráfico Inadecuado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _cboEstiloGrafico.SelectedItem = "Barras";
                    estiloGrafico = "Barras";
                }

                if (estiloGrafico == "Torta (Pie)" && _chkComparar.Checked)
                {
                    MessageBox.Show("El gráfico de Torta no soporta la comparación de períodos. Se mostrarán solo los datos del Período 1. Use el gráfico de Dona para comparar.", "Comparación no Soportada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                string chartType = estiloGrafico.ToLowerInvariant() switch
                {
                    "torta (pie)" => "pie",
                    "dona (doughnut)" => "doughnut",
                    "líneas" => "line",
                    _ => "bar"
                };
                
                object chartData;
                GranularidadTemporal granularidad = GranularidadTemporal.Dia; // Default

                if (esTemporal)
                {
                    var seleccionGranularidad = Enum.TryParse<GranularidadTemporal>(_cboGranularidad.SelectedItem?.ToString(), out var g) ? g : GranularidadTemporal.Auto;
                    granularidad = ResolverGranularidad(_dpDesde1.Value, _dpHasta1.Value, seleccionGranularidad);

                    var ventas1 = CargarVentasFiltradas(_dpDesde1.Value, _dpHasta1.Value);
                    var datosTemporales1 = GetTemporal(ventas1, granularidad);

                    var todosBuckets = GenerarBuckets(_dpDesde1.Value, _dpHasta1.Value, granularidad).ToList();
                    var datosAlineados1 = AlignTemporalData(datosTemporales1, todosBuckets);

                    var datasets = new List<object> {
                        new {
                            label = $"Período 1 ({_dpDesde1.Value:d} - {_dpHasta1.Value:d})",
                            data = datosAlineados1.Select(p => new { x = p.Bucket, y = p.Valor }).ToList(),
                            backgroundColor = "rgba(54, 162, 235, 0.6)",
                            borderColor = "rgba(54, 162, 235, 1)",
                            borderWidth = 1
                        }
                    };

                    if (_chkComparar.Checked)
                    {
                        var ventas2 = CargarVentasFiltradas(_dpDesde2.Value, _dpHasta2.Value);
                        var datosTemporales2 = GetTemporal(ventas2, granularidad);
                        // Para comparar, los buckets deben ser relativos al rango principal
                        var datosAlineados2 = AlignTemporalData(datosTemporales2, todosBuckets);
                        datasets.Add(new {
                            label = $"Período 2 ({_dpDesde2.Value:d} - {_dpHasta2.Value:d})",
                            data = datosAlineados2.Select(p => new { x = p.Bucket, y = p.Valor }).ToList(),
                            backgroundColor = "rgba(255, 99, 132, 0.6)",
                            borderColor = "rgba(255, 99, 132, 1)",
                            borderWidth = 1
                        });
                    }
                    chartData = new { datasets };
                }
                else // Es Ranking
                {
                    var ventas1 = CargarVentasFiltradas(_dpDesde1.Value, _dpHasta1.Value);
                    var ranking1 = GetRanking(ventas1, tipoGraficoSeleccionado);

                    var labels = ranking1.Select(p => p.Label).ToList();
                    var data1 = ranking1.Select(p => p.Valor).ToList();

                    var datasets = new List<object> {
                        new {
                            label = $"Período 1 ({_dpDesde1.Value:d} - {_dpHasta1.Value:d})",
                            data = data1,
                            backgroundColor = GenerateColors(labels.Count, 0.6f),
                            borderColor = GenerateColors(labels.Count, 1f),
                            borderWidth = 1
                        }
                    };

                    if (_chkComparar.Checked && chartType != "pie")
                    {
                        var ventas2 = CargarVentasFiltradas(_dpDesde2.Value, _dpHasta2.Value);
                        var ranking2 = GetRanking(ventas2, tipoGraficoSeleccionado);

                        var labels2 = ranking2.Select(p => p.Label).ToList();
                        var data2 = ranking2.Select(p => p.Valor).ToList();

                        var allLabels = labels.Union(labels2).OrderBy(l => l).ToList();
                        var realignedData1 = RealignRankingData(ranking1, allLabels);
                        var realignedData2 = RealignRankingData(ranking2, allLabels);

                        datasets[0] = new { ((dynamic)datasets[0]).label, data = realignedData1, backgroundColor = GenerateColors(allLabels.Count, 0.6f, 0), borderColor = GenerateColors(allLabels.Count, 1f, 0), borderWidth = 1 };
                        datasets.Add(new {
                            label = $"Período 2 ({_dpDesde2.Value:d} - {_dpHasta2.Value:d})",
                            data = realignedData2,
                            backgroundColor = GenerateColors(allLabels.Count, 0.6f, 1),
                            borderColor = GenerateColors(allLabels.Count, 1f, 1),
                            borderWidth = 1
                        });
                        labels = allLabels;
                    }
                    chartData = new { labels, datasets };
                }

                string titulo = _cboTipoGrafico.SelectedItem?.ToString() ?? "Gráfico de Ventas";
                string optionsJson = BuildChartOptions(chartType, esTemporal, granularidad, titulo);
                _lastChartData = chartData;
                _lastChartTitle = titulo;

                string script = $"renderChart('{chartType}', {JsonConvert.SerializeObject(chartData)}, {optionsJson});";
                await _webView.CoreWebView2.ExecuteScriptAsync(script);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar el gráfico: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }       

        private List<Venta> CargarVentasFiltradas(DateTime desde, DateTime hasta)
        {
            var start = desde.Date;
            var end = hasta.Date.AddDays(1);

            var q = Repository.ListarVentas(true)
                .Where(v => v.FechaVenta >= start && v.FechaVenta < end && !v.Anulada);

            // Aplicar filtros
            if (_cboVendedor.SelectedIndex > 0)
            {
                var vend = _cboVendedor.SelectedItem?.ToString() ?? string.Empty;
                q = q.Where(v => string.Equals(v.Vendedor, vend, StringComparison.OrdinalIgnoreCase));
            }

            if (_cboCategoria.SelectedIndex > 0)
            {
                var catNombre = _cboCategoria.SelectedItem?.ToString() ?? string.Empty;
                var productosEnCategoria = Repository.ListarProductos(true)
                    .Where(p => string.Equals(p.CategoriaNombre, catNombre, StringComparison.OrdinalIgnoreCase))
                    .Select(p => p.Id).ToHashSet();

                q = q.Where(v => v.Detalles.Any(d => productosEnCategoria.Contains(d.ProductoId)));
            }

            if (_cboProvincia.SelectedValue is int provId && provId > 0)
            {
                var localidadesEnProvincia = Repository.ListarLocalidades(provId, true)
                    .Select(l => l.Id).ToHashSet();
                q = q.Where(v => v.LocalidadId.HasValue && localidadesEnProvincia.Contains(v.LocalidadId.Value));
            }

            return q.ToList();
        }

        #region Data Processing

        private List<PuntoRanking> GetRanking(List<Venta> ventas, string tipo)
        {
            if (!ventas.Any()) return new List<PuntoRanking>();

            IEnumerable<PuntoRanking> resultado;
            const int topN = 15;

            switch (tipo)
            {
                case "Ventas por Vendedor":
                    resultado = ventas.GroupBy(v => v.Vendedor).Select(g => new PuntoRanking(g.Key, g.Sum(v => v.Total)));
                    break;
                case "Ventas por Categoría":
                    var productosCat = Repository.ListarProductos(true).ToDictionary(p => p.Id, p => p.CategoriaNombre);
                    resultado = ventas.SelectMany(v => v.Detalles).Where(d => productosCat.ContainsKey(d.ProductoId))
                        .GroupBy(d => productosCat[d.ProductoId]).Select(g => new PuntoRanking(g.Key, g.Sum(d => d.Subtotal)));
                    break;
                case "Ventas por Provincia":
                    var localidadesProv = Repository.ListarLocalidades(null, true).ToDictionary(l => l.Id, l => l.ProvinciaNombre ?? "N/A");
                    resultado = ventas.Where(v => v.LocalidadId.HasValue && localidadesProv.ContainsKey(v.LocalidadId.Value))
                        .GroupBy(v => localidadesProv[v.LocalidadId.Value]).Select(g => new PuntoRanking(g.Key, g.Sum(v => v.Total)));
                    break;
                case "Ventas por Producto (Top 15)":
                    resultado = ventas.SelectMany(v => v.Detalles).GroupBy(d => d.ProductoNombre)
                        .Select(g => new PuntoRanking(g.Key, g.Sum(d => d.Subtotal)));
                    break;
                default:
                    return new List<PuntoRanking>();
            }

            var ordenado = resultado.OrderByDescending(p => p.Valor).ToList();

            if (tipo.Contains("Top 15") && ordenado.Count > topN)
            {
                var topItems = ordenado.Take(topN).ToList();
                var otrosValor = ordenado.Skip(topN).Sum(p => p.Valor);
                if (otrosValor > 0)
                {
                    topItems.Add(new PuntoRanking("Otros", otrosValor));
                }
                return topItems;
            }

            return ordenado;
        }

        private List<PuntoTemporal> GetTemporal(List<Venta> ventas, GranularidadTemporal g)
        {
            if (!ventas.Any()) return new List<PuntoTemporal>();

            return ventas
                .GroupBy(v => BucketDate(v.FechaVenta, g))
                .Select(g => new PuntoTemporal(g.Key, g.Sum(v => v.Total)))
                .OrderBy(p => p.Bucket)
                .ToList();
        }

        private List<decimal> RealignRankingData(List<PuntoRanking> originalData, List<string> unifiedLabels)
        {
            var dataMap = originalData.ToDictionary(p => p.Label, p => p.Valor);
            return unifiedLabels.Select(label => dataMap.TryGetValue(label, out var valor) ? valor : 0).ToList();
        }

        private List<PuntoTemporal> AlignTemporalData(List<PuntoTemporal> originalData, List<DateTime> unifiedBuckets)
        {
            var dataMap = originalData.ToDictionary(p => p.Bucket, p => p.Valor);
            return unifiedBuckets.Select(bucket => new PuntoTemporal(bucket, dataMap.TryGetValue(bucket, out var valor) ? valor : 0)).ToList();
        }

        #endregion

        #region Helpers

        private GranularidadTemporal ResolverGranularidad(DateTime desde, DateTime hasta, GranularidadTemporal seleccionada)
        {
            if (seleccionada != GranularidadTemporal.Auto) return seleccionada;
            var dias = (hasta.Date - desde.Date).TotalDays + 1;
            if (dias <= 31) return GranularidadTemporal.Dia;
            if (dias <= 180) return GranularidadTemporal.Semana;
            return GranularidadTemporal.Mes;
        }

        private static DateTime BucketDate(DateTime dt, GranularidadTemporal g) => g switch
        {
            GranularidadTemporal.Dia => dt.Date,
            GranularidadTemporal.Semana => dt.Date.AddDays(-((((int)dt.DayOfWeek) + 6) % 7)), // Lunes es el inicio
            GranularidadTemporal.Mes => new DateTime(dt.Year, dt.Month, 1),
            _ => dt.Date
        };

        private static IEnumerable<DateTime> GenerarBuckets(DateTime d, DateTime h, GranularidadTemporal g)
        {
            var cur = BucketDate(d, g);
            var end = BucketDate(h, g);
            while (cur <= end)
            {
                yield return cur;
                cur = g switch
                {
                    GranularidadTemporal.Dia => cur.AddDays(1),
                    GranularidadTemporal.Semana => cur.AddDays(7),
                    GranularidadTemporal.Mes => cur.AddMonths(1),
                    _ => cur.AddDays(1)
                };
            }
        }

        private string BuildChartOptions(string chartType, bool esTemporal, GranularidadTemporal g, string titulo)
        {
            var unit = g switch { GranularidadTemporal.Dia => "day", GranularidadTemporal.Semana => "week", GranularidadTemporal.Mes => "month", _ => "day" };
            
            string scales = "";
            if (chartType == "line" || chartType == "bar")
            {
                scales = esTemporal
                  ? $@"scales: {{
                          x: {{ type: 'time', time: {{ unit: '{unit}', tooltipFormat: 'dd/MM/yyyy' }}, ticks: {{ source: 'auto', maxRotation: 45 }} }},
                          y: {{ beginAtZero: true, ticks: {{ callback: (v) => '$' + v.toLocaleString('es-AR') }} }}
                      }},"
                  : @"scales: { y: { beginAtZero: true, ticks: { callback: (v) => '$' + v.toLocaleString('es-AR') } } },";
            }

            return $@"{{
                responsive: true,
                maintainAspectRatio: false,
                plugins: {{
                    title: {{ display: true, text: '{titulo.Replace("'", "\\'")}', font: {{ size: 18 }} }},
                    legend: {{ position: 'top' }},
                    tooltip: {{
                        callbacks: {{
                            label: function(context) {{
                                let label = context.dataset.label || '';
                                if (label) {{ label += ': '; }}
                                if (context.parsed.y !== null) {{
                                    label += new Intl.NumberFormat('es-AR', {{ style: 'currency', currency: 'ARS' }}).format(context.parsed.y);
                                }}
                                return label;
                            }}
                        }}
                    }}
                }},
                {scales}
            }}";
        }

        private static List<string> GenerateColors(int count, float alpha, int paletteIndex = 0)
        {
            var palettes = new[]
            {
                new[] { "54, 162, 235", "255, 99, 132", "255, 206, 86", "75, 192, 192", "153, 102, 255", "255, 159, 64" }, // Palette 1
                new[] { "46, 204, 113", "231, 76, 60", "241, 196, 15", "52, 152, 219", "155, 89, 182", "26, 188, 156" }  // Palette 2
            };
            var selectedPalette = palettes[paletteIndex % palettes.Length];
            var colors = new List<string>();
            for (int i = 0; i < count; i++)
            {
                colors.Add($"rgba({selectedPalette[i % selectedPalette.Length]}, {alpha.ToString(CultureInfo.InvariantCulture)})");
            }
            return colors;
        }

        #endregion

        private async Task Exportar()
        {
            if (!_isWebViewReady || _lastChartData == null)
            {
                MessageBox.Show("No hay gráfico para exportar.", "Vacío", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var optionsForm = new GraficoExportOptionsForm();
            if (optionsForm.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            string filter = optionsForm.SelectedFormat == GraficoExportOptionsForm.ExportFormat.Excel
                ? "Excel Workbook (*.xlsx)|*.xlsx"
                : "PDF Document (*.pdf)|*.pdf";

            string extension = optionsForm.SelectedFormat == GraficoExportOptionsForm.ExportFormat.Excel
                ? ".xlsx"
                : ".pdf";

            using var sfd = new SaveFileDialog
            {
                Filter = filter,
                FileName = $"Grafico_{(_cboTipoGrafico.SelectedItem?.ToString() ?? "Grafico").Replace(" ", "")}_{DateTime.Now:yyyyMMdd}{extension}"
            };

            if (sfd.ShowDialog(this) != DialogResult.OK) return;
        
            try
            {
                this.Cursor = Cursors.WaitCursor;
        
                if (optionsForm.SelectedFormat == GraficoExportOptionsForm.ExportFormat.Pdf)
                {
                    await ExportarPdf(sfd.FileName);
                }
                else // Excel
                {
                    await ExportarExcel(sfd.FileName);
                }
        
                MessageBox.Show("Archivo exportado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar el archivo: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private async Task ExportarPdf(string filePath)
        {
            // Capturar la imagen del WebView2
            using var stream = new MemoryStream();
            await _webView.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, stream);
            stream.Position = 0; 

            // Generar PDF con QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Header().Element(header =>
                    {
                        header.Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Reporte Gráfico de Ventas").Bold().FontSize(20);
                                col.Item().Text($"Generado: {DateTime.Now:g} por {Sesion.Usuario}");
                                col.Item().Text($"Tipo de Gráfico: {_cboTipoGrafico.SelectedItem?.ToString() ?? "Gráfico Desconocido"}");
                            });
                            try
                            {
                                var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "recursos", "logo.png");
                                if (File.Exists(logoPath))
                                    row.ConstantItem(100).AlignRight().Image(logoPath);
                            }
                            catch { /* Ignorar si falla el logo */ }
                        });
                    });

                    page.Content().Element(body =>
                    {
                        body.Column(col =>
                        {
                            col.Item().PaddingVertical(10).LineHorizontal(1);

                            // Información de Períodos y Filtros
                            col.Item().Text("Período Principal:").SemiBold();
                            col.Item().Text($"{_dpDesde1.Value:d} al {_dpHasta1.Value:d}");

                            if (_chkComparar.Checked)
                            {
                                col.Item().PaddingTop(5).Text("Período de Comparación:").SemiBold();
                                col.Item().Text($"{_dpDesde2.Value:d} al {_dpHasta2.Value:d}");
                            }

                            col.Item().PaddingTop(10).Text("Filtros Aplicados:").SemiBold();
                            bool sinFiltros = true;
                            if (_cboVendedor.SelectedIndex > 0) { col.Item().Text($"Vendedor: {_cboVendedor.SelectedItem}"); sinFiltros = false; }
                            if (_cboCategoria.SelectedIndex > 0) { col.Item().Text($"Categoría: {_cboCategoria.SelectedItem?.ToString() ?? "Todas"}"); sinFiltros = false; }
                            if (_cboProvincia.SelectedIndex > 0) { col.Item().Text($"Provincia: {_cboProvincia.Text}"); sinFiltros = false; }
                            if (sinFiltros) col.Item().Text("Ninguno").Italic(); 

                            // Imagen del gráfico
                            col.Item().PaddingTop(20).AlignCenter().Image(stream.ToArray(), ImageScaling.FitWidth);
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                    });
                });
            }).GeneratePdf(filePath);
        }

        private async Task ExportarExcel(string filePath)
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Gráfico");
            int row = 1;

            // --- Título y Filtros ---
            ws.Cell(row, 1).Value = _lastChartTitle;
            ws.Range(row, 1, row, 5).Merge().Style.Font.SetBold();
            ws.Range(row, 1, row, 5).Style.Font.FontSize = 16;
            row++;
            ws.Cell(row, 1).Value = $"Generado: {DateTime.Now:g}";
            row++;
            ws.Cell(row, 1).Value = $"Período Principal: {_dpDesde1.Value:d} al {_dpHasta1.Value:d}";
            if (_chkComparar.Checked)
            {
                row++;
                ws.Cell(row, 1).Value = $"Período Comparación: {_dpDesde2.Value:d} al {_dpHasta2.Value:d}";
            }
            row += 2;

            // --- Imagen del Gráfico ---
            using (var stream = new MemoryStream())
            {
                await _webView.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, stream);
                stream.Position = 0;
                var pic = ws.AddPicture(stream).MoveTo(ws.Cell(row, 1));
                pic.Scale(0.75);
                row += (int)(pic.Height / 15) + 2; // Estimar filas ocupadas por la imagen
            }

            // --- Tabla de Datos ---
            ws.Cell(row, 1).Value = "Datos del Gráfico";
            ws.Range(row, 1, row, 5).Merge().Style.Font.SetBold();
            ws.Range(row, 1, row, 5).Style.Font.FontSize = 14;
            row++;

            var data = _lastChartData!;
            var dataType = data.GetType();

            // Obtener datasets como IEnumerable (no intentar castear List<AnonType> a List<T>)
            var datasetsEnumerable = dataType.GetProperty("datasets")?.GetValue(data) as System.Collections.IEnumerable;
            var datasetsList = datasetsEnumerable != null ? datasetsEnumerable.Cast<object>().ToList() : new List<object>();

            // Encabezados
            var headerCell = ws.Cell(row, 1);
            headerCell.Value = "Etiqueta";
            headerCell.Style.Font.SetBold();

            for (int i = 0; i < datasetsList.Count; i++)
            {
                var ds = datasetsList[i];
                var labelProp = ds.GetType().GetProperty("label");
                var dsLabel = labelProp != null ? labelProp.GetValue(ds)?.ToString() ?? $"Serie {i + 1}" : $"Serie {i + 1}";
                var dsHeaderCell = ws.Cell(row, i + 2);
                dsHeaderCell.Value = dsLabel;
                dsHeaderCell.Style.Font.SetBold();
            }
            row++;

            // Determinar si es ranking (tiene 'labels') o temporal
            var labelsPropInfo = dataType.GetProperty("labels");
            if (labelsPropInfo != null)
            {
                // Ranking
                var labelsEnumerable = labelsPropInfo.GetValue(data) as System.Collections.IEnumerable;
                var labels = labelsEnumerable != null ? labelsEnumerable.Cast<object>().Select(x => x?.ToString() ?? string.Empty).ToList() : new List<string>();

                for (int i = 0; i < labels.Count; i++)
                {
                    ws.Cell(row + i, 1).Value = labels[i];
                    for (int j = 0; j < datasetsList.Count; j++)
                    {
                        var ds = datasetsList[j];
                        var dataProp = ds.GetType().GetProperty("data");
                        var valuesEnumerable = dataProp?.GetValue(ds) as System.Collections.IEnumerable;
                        var valuesList = valuesEnumerable != null ? valuesEnumerable.Cast<object>().Select(v => Convert.ToDecimal(v ?? 0)).ToList() : new List<decimal>();
                        var cell = ws.Cell(row + i, j + 2);
                        var val = i < valuesList.Count ? valuesList[i] : 0m;
                        cell.Value = val;
                        cell.Style.NumberFormat.Format = "$ #,##0.00";
                    }
                }
            }
            else
            {
                // Temporal: cada elemento de datasets[].data es un objeto con { x = DateTime, y = decimal }
                if (datasetsList.Count > 0)
                {
                    var firstDs = datasetsList[0];
                    var firstDataProp = firstDs.GetType().GetProperty("data");
                    var firstDataEnumerable = firstDataProp?.GetValue(firstDs) as System.Collections.IEnumerable;
                    var firstDataList = firstDataEnumerable != null ? firstDataEnumerable.Cast<object>().ToList() : new List<object>();

                    for (int i = 0; i < firstDataList.Count; i++)
                    {
                        var item = firstDataList[i];
                        var itemType = item.GetType();
                        var xProp = itemType.GetProperty("x");
                        var yProp = itemType.GetProperty("y");

                        var xVal = xProp != null ? xProp.GetValue(item) : null;
                        DateTime dateVal = xVal != null ? Convert.ToDateTime(xVal) : DateTime.MinValue;
                        ws.Cell(row + i, 1).Value = dateVal;
                        ws.Cell(row + i, 1).Style.DateFormat.Format = "dd/MM/yyyy";

                        for (int j = 0; j < datasetsList.Count; j++)
                        {
                            var ds = datasetsList[j];
                            var dataProp = ds.GetType().GetProperty("data");
                            var valuesEnumerable = dataProp?.GetValue(ds) as System.Collections.IEnumerable;
                            var valuesList = valuesEnumerable != null ? valuesEnumerable.Cast<object>().ToList() : new List<object>();

                            decimal yValue = 0m;
                            if (i < valuesList.Count)
                            {
                                var element = valuesList[i];
                                var yyProp = element.GetType().GetProperty("y");
                                var rawY = yyProp != null ? yyProp.GetValue(element) : element;
                                yValue = rawY != null ? Convert.ToDecimal(rawY) : 0m;
                            }

                            var cell = ws.Cell(row + i, j + 2);
                            cell.Value = yValue;
                            cell.Style.NumberFormat.Format = "$ #,##0.00";
                        }
                    }
                }
            }

            ws.Columns().AdjustToContents();

            // Proteger la hoja para que sea de solo lectura
            ws.Protect();
            wb.SaveAs(filePath);
        }

       
        private async Task Exportar_OLD()
        {
            if (!_isWebViewReady)
            {
                MessageBox.Show("No hay gráfico para exportar.", "Vacío", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "PDF Document (*.pdf)|*.pdf",
                FileName = $"Grafico_{(_cboTipoGrafico.SelectedItem?.ToString() ?? "Grafico").Replace(" ", "")}_{DateTime.Now:yyyyMMdd}.pdf"
            };

            if (sfd.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                this.Cursor = Cursors.WaitCursor;

                // Capturar la imagen del WebView2
                using var stream = new MemoryStream();
                await _webView.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, stream); 
                stream.Position = 0; 
                var chartImage = System.Drawing.Image.FromStream(stream);

                // Generar PDF con QuestPDF
                QuestPDF.Settings.License = LicenseType.Community;

                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(30);

                        page.Header().Element(header =>
                        {
                            header.Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("Reporte Gráfico de Ventas").Bold().FontSize(20);
                                    col.Item().Text($"Generado: {DateTime.Now:g} por {Sesion.Usuario}");
                                    col.Item().Text($"Tipo de Gráfico: {_cboTipoGrafico.SelectedItem?.ToString() ?? "Gráfico Desconocido"}");
                                });
                                try
                                {
                                    var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "recursos", "logo.png");
                                    if (File.Exists(logoPath))
                                        row.ConstantItem(100).AlignRight().Image(logoPath);
                                }
                                catch { /* Ignorar si falla el logo */ }
                            });
                        });

                        page.Content().Element(body =>
                        {
                            body.Column(col =>
                            {
                                col.Item().PaddingVertical(10).LineHorizontal(1);

                                // Información de Períodos y Filtros
                                col.Item().Text("Período Principal:").SemiBold();
                                col.Item().Text($"{_dpDesde1.Value:d} al {_dpHasta1.Value:d}");

                                if (_chkComparar.Checked)
                                {
                                    col.Item().PaddingTop(5).Text("Período de Comparación:").SemiBold();
                                    col.Item().Text($"{_dpDesde2.Value:d} al {_dpHasta2.Value:d}");
                                }

                                col.Item().PaddingTop(10).Text("Filtros Aplicados:").SemiBold();
                                bool sinFiltros = true;
                                if (_cboVendedor.SelectedIndex > 0) { col.Item().Text($"Vendedor: {_cboVendedor.SelectedItem}"); sinFiltros = false; }
                                if (_cboCategoria.SelectedIndex > 0) { col.Item().Text($"Categoría: {_cboCategoria.SelectedItem?.ToString() ?? "Todas"}"); sinFiltros = false; } // Corrected null handling
                                if (_cboProvincia.SelectedIndex > 0) { col.Item().Text($"Provincia: {_cboProvincia.Text}"); sinFiltros = false; }
                                if (sinFiltros) col.Item().Text("Ninguno").Italic(); // This line is fine, it's a literal string.

                                // Imagen del gráfico
                                col.Item().PaddingTop(20).AlignCenter().Image(stream.ToArray(), ImageScaling.FitWidth);
                            });
                        });

                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                        });
                    });
                }).GeneratePdf(sfd.FileName);

                MessageBox.Show("PDF exportado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar el archivo: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
    }
}