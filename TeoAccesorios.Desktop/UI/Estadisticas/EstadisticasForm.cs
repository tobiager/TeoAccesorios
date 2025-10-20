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
using DrawingColor = System.Drawing.Color;
using QuestPDF.Drawing;

namespace TeoAccesorios.Desktop.UI.Estadisticas
{
    public class EstadisticasForm : Form
    {
        // --- Filtros ---
        private readonly DateTimePicker dpDesde = new() { Value = DateTime.Today.AddDays(-30), Width = 110 };
        private readonly DateTimePicker dpHasta = new() { Value = DateTime.Today, Width = 110 };
        private readonly ComboBox cboVendedor = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 130 };
        private readonly ComboBox cboCliente = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 130 };
        private readonly ComboBox cboCategoria = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 130 };
        private readonly ComboBox cboSubcategoria = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 130 };
        private readonly ComboBox cboProvincia = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 130 };
        private readonly ComboBox cboLocalidad = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 130 };
        
        // --- Botones de acción ---
        private readonly Button btnExportar = new() { Text = "📊 Exportar", AutoSize = true, BackColor = DrawingColor.FromArgb(40, 167, 69), ForeColor = DrawingColor.White, FlatStyle = FlatStyle.Flat };
        private readonly Button btnGraficos = new() { Text = "📈 Gráficos", AutoSize = true, BackColor = DrawingColor.FromArgb(23, 162, 184), ForeColor = DrawingColor.White, FlatStyle = FlatStyle.Flat };

        // --- Definición de Tops ---
        private readonly (string titulo, string icono, DataGridView grid, DrawingColor color)[] _topsDefinition;

        // --- Diccionario para mapear nombre de top a su grid ---
        private readonly Dictionary<string, DataGridView> _topGrids = new();

        // --- Diccionario para mapear nombre de top a sus datos ---
        private readonly Dictionary<string, IEnumerable<dynamic>> _topDataSources = new();



        // --- Grillas para tops ---
        private readonly DataGridView _gridTopProductos = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false };
        private readonly DataGridView _gridTopClientes = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false };
        private readonly DataGridView _gridTopVendedores = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false };
        private readonly DataGridView _gridTopCategorias = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false };
        private readonly DataGridView _gridTopProvincias = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false };
        private readonly DataGridView _gridTopLocalidades = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false };

        // --- Datos ---
        private List<Venta> _ventasFiltradas = new();
        private readonly CultureInfo _culture = new("es-AR");

        public EstadisticasForm()
        {
            Text = "📊 Estadísticas de Ventas";
            WindowState = FormWindowState.Maximized;
            BackColor = DrawingColor.FromArgb(248, 249, 250);

            _topsDefinition = new (string, string, DataGridView, DrawingColor)[]
            {
                ("Top Productos", "📦", _gridTopProductos, DrawingColor.FromArgb(220, 53, 69)),
                ("Top Clientes", "👥", _gridTopClientes, DrawingColor.FromArgb(0, 120, 215)),
                ("Top Vendedores", "🏆", _gridTopVendedores, DrawingColor.FromArgb(255, 193, 7)),
                ("Top Categorías", "📋", _gridTopCategorias, DrawingColor.FromArgb(40, 167, 69)),
                ("Top Provincias", "🌍", _gridTopProvincias, DrawingColor.FromArgb(108, 117, 125)),
                ("Top Localidades", "📍", _gridTopLocalidades, DrawingColor.FromArgb(111, 66, 193))
            };
            _topGrids = _topsDefinition.ToDictionary(t => t.titulo, t => t.grid);

            ConfigurarLayout();
            ConfigurarGrids();
            ConfigurarEventos();
            CargarCombos();
            CargarDatos();
        }

        private void ConfigurarLayout()
        {
            // Layout principal simplificado - solo filtros y rankings
            var layoutPrincipal = new TableLayoutPanel 
            { 
                Dock = DockStyle.Fill, 
                ColumnCount = 1, 
                RowCount = 2,
                Padding = new Padding(15),
                BackColor = DrawingColor.FromArgb(248, 249, 250)
            };
            layoutPrincipal.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Filtros y controles
            layoutPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Rankings - todo el espacio restante

            // === PANEL DE FILTROS Y CONTROLES ===
            var panelFiltros = CrearPanelFiltros();
            
            // === PANEL DE RANKINGS ===
            var panelTops = CrearPanelTops();

            layoutPrincipal.Controls.Add(panelFiltros, 0, 0);
            layoutPrincipal.Controls.Add(panelTops, 0, 1);
            
            Controls.Add(layoutPrincipal);
        }

        private Panel CrearPanelFiltros()
        {
            var panel = new Panel 
            { 
                Dock = DockStyle.Fill, 
                AutoSize = true,
                BackColor = DrawingColor.White,
                Padding = new Padding(20),
                Margin = new Padding(0, 0, 0, 15)
            };

            // Borde y sombra sutil
            panel.Paint += (s, e) => 
            {
                using var pen = new Pen(DrawingColor.FromArgb(222, 226, 230));
                e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
            };

            var layout = new TableLayoutPanel 
            { 
                Dock = DockStyle.Top, 
                AutoSize = true, 
                ColumnCount = 8, 
                RowCount = 4 
            };

            // Configurar columnas
            for (int i = 0; i < 8; i++)
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            // Título y botones
            var panelTitulo = new FlowLayoutPanel 
            { 
                Dock = DockStyle.Top, 
                AutoSize = true, 
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(0, 0, 0, 15)
            };

            var lblTitulo = new Label 
            { 
                Text = "🔍 Filtros y Configuración", 
                Font = new Font("Segoe UI", 14, FontStyle.Bold), 
                ForeColor = DrawingColor.FromArgb(33, 37, 41),
                AutoSize = true, 
                Margin = new Padding(0, 0, 20, 0)
            };

            panelTitulo.Controls.Add(lblTitulo);
  
            panelTitulo.Controls.Add(new Panel { Width = 10 }); // Espaciador
            panelTitulo.Controls.Add(btnExportar);
            panelTitulo.Controls.Add(btnGraficos);

            // Filtros organizados
            var lblFechas = new Label 
            { 
                Text = "📅 Período:", 
                Font = new Font("Segoe UI", 10, FontStyle.Bold), 
                AutoSize = true,
                ForeColor = DrawingColor.FromArgb(73, 80, 87),
                Anchor = AnchorStyles.Right,
                Padding = new Padding(0, 6, 5, 6) 
            };
            layout.Controls.Add(lblFechas, 0, 0);
            layout.Controls.Add(new Label { Text = "Desde:", AutoSize = true, Anchor = AnchorStyles.Right, Padding = new Padding(10, 6, 5, 0) }, 1, 0);
            layout.Controls.Add(dpDesde, 2, 0);
            layout.Controls.Add(new Label { Text = "Hasta:", AutoSize = true, Anchor = AnchorStyles.Right, Padding = new Padding(15, 6, 5, 0) }, 3, 0);
            layout.Controls.Add(dpHasta, 4, 0);

            var lblPersonas = new Label 
            { 
                Text = "👥 Personas:", 
                Font = new Font("Segoe UI", 10, FontStyle.Bold), 
                AutoSize = true,
                ForeColor = DrawingColor.FromArgb(73, 80, 87),
                Anchor = AnchorStyles.Right,
                Padding = new Padding(0, 6, 5, 6) 
            };
            layout.Controls.Add(lblPersonas, 0, 1);
            layout.Controls.Add(new Label { Text = "Vendedor:", AutoSize = true, Anchor = AnchorStyles.Right, Padding = new Padding(10, 6, 5, 0) }, 1, 1);
            layout.Controls.Add(cboVendedor, 2, 1);
            layout.Controls.Add(new Label { Text = "Cliente:", AutoSize = true, Anchor = AnchorStyles.Right, Padding = new Padding(15, 6, 5, 0) }, 3, 1);
            layout.Controls.Add(cboCliente, 4, 1);

            var lblProductos = new Label 
            { 
                Text = "📦 Productos:", 
                Font = new Font("Segoe UI", 10, FontStyle.Bold), 
                AutoSize = true,
                ForeColor = DrawingColor.FromArgb(73, 80, 87),
                Anchor = AnchorStyles.Right,
                Padding = new Padding(0, 6, 5, 6) 
            };
            layout.Controls.Add(lblProductos, 0, 2);
            layout.Controls.Add(new Label { Text = "Categoría:", AutoSize = true, Anchor = AnchorStyles.Right, Padding = new Padding(10, 6, 5, 0) }, 1, 2);
            layout.Controls.Add(cboCategoria, 2, 2);
            layout.Controls.Add(new Label { Text = "Subcategoría:", AutoSize = true, Anchor = AnchorStyles.Right, Padding = new Padding(15, 6, 5, 0) }, 3, 2);
            layout.Controls.Add(cboSubcategoria, 4, 2);

            var lblUbicacion = new Label 
            { 
                Text = "🌍 Ubicación:", 
                Font = new Font("Segoe UI", 10, FontStyle.Bold), 
                AutoSize = true,
                ForeColor = DrawingColor.FromArgb(73, 80, 87),
                Anchor = AnchorStyles.Right,
                Padding = new Padding(0, 6, 5, 6) 
            };
            layout.Controls.Add(lblUbicacion, 0, 3);
            layout.Controls.Add(new Label { Text = "Provincia:", AutoSize = true, Anchor = AnchorStyles.Right, Padding = new Padding(10, 6, 5, 0) }, 1, 3);
            layout.Controls.Add(cboProvincia, 2, 3);
            layout.Controls.Add(new Label { Text = "Localidad:", AutoSize = true, Anchor = AnchorStyles.Right, Padding = new Padding(15, 6, 5, 0) }, 3, 3);
            layout.Controls.Add(cboLocalidad, 4, 3);

            panel.Controls.Add(panelTitulo);
            panel.Controls.Add(layout);
            return panel;
        }

        private Panel CrearPanelTops()
        {
            var panel = new Panel 
            { 
                Dock = DockStyle.Fill, 
                BackColor = DrawingColor.Transparent
            };

            // Título
            var lblTitulo = new Label 
            { 
                Text = "🏆 Rankings y Clasificaciones", 
                Font = new Font("Segoe UI", 12, FontStyle.Bold), 
                ForeColor = DrawingColor.FromArgb(33, 37, 41),
                AutoSize = true, 
                Dock = DockStyle.Top,
                Padding = new Padding(0, 0, 0, 0) 
            };

            // Layout de tops en grid 3x2
            var layoutTops = new TableLayoutPanel 
            { 
                Dock = DockStyle.Fill, 
                ColumnCount = 3, 
                RowCount = 2 
            };
            
            // Configurar columnas y filas
            for (int i = 0; i < 3; i++)
                layoutTops.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            
            layoutTops.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            layoutTops.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            // Crear cards para cada top
            for (int i = 0; i < _topsDefinition.Length; i++)
            {
                var (titulo, icono, grid, color) = _topsDefinition[i];
                var card = CrearCardTop(titulo, icono, grid, color);
                var col = i % 3;
                var row = i / 3;
                layoutTops.Controls.Add(card, col, row);
            }

            panel.Controls.Add(lblTitulo);
            panel.Controls.Add(layoutTops);
            return panel;
        }

        private Panel CrearCardTop(string titulo, string icono, DataGridView grid, DrawingColor accentColor)
        {
            var card = new Panel 
            { 
                Dock = DockStyle.Fill, 
                Margin = new Padding(8),
                BackColor = DrawingColor.White,
                Padding = new Padding(15)
            };

            // Borde con color de acento
            card.Paint += (s, e) => 
            {
                using var pen = new Pen(DrawingColor.FromArgb(222, 226, 230));
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                // Línea de acento en la parte superior
                using var accentPen = new Pen(accentColor, 3);
                e.Graphics.DrawLine(accentPen, 0, 0, card.Width, 0);
            };

            var layout = new TableLayoutPanel 
            { 
                Dock = DockStyle.Fill, 
                RowCount = 2 
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Título con icono y color
            var lblTitulo = new Label 
            { 
                Text = $"{icono} {titulo}", 
                Font = new Font("Segoe UI", 11, FontStyle.Bold), 
                ForeColor = accentColor,
                AutoSize = true, 
                Padding = new Padding(0, 0, 0, 10) 
            };

            layout.Controls.Add(lblTitulo, 0, 0);
            layout.Controls.Add(grid, 0, 1);
            card.Controls.Add(layout);

            return card;
        }

        private void ConfigurarGrids()
        {
            var grids = new[] { _gridTopProductos, _gridTopClientes, _gridTopVendedores, 
                              _gridTopCategorias, _gridTopProvincias, _gridTopLocalidades };

            foreach (var grid in grids)
            {
                GridHelper.Estilizar(grid);
                ConfigurarColumnasGrid(grid);
            }
        }

        private void ConfigurarColumnasGrid(DataGridView grid)
        {
            grid.Columns.Clear();

            if (grid == _gridTopProductos)
            {
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Producto", DataPropertyName = "Nombre", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cantidad", DataPropertyName = "Cantidad", Width = 80, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total", DataPropertyName = "Monto", Width = 90, DefaultCellStyle = { Format = "C0", Alignment = DataGridViewContentAlignment.MiddleRight } });
            }
            else if (grid == _gridTopClientes)
            {
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cliente", DataPropertyName = "Nombre", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Compras", DataPropertyName = "Cantidad", Width = 80, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total", DataPropertyName = "Monto", Width = 90, DefaultCellStyle = { Format = "C0", Alignment = DataGridViewContentAlignment.MiddleRight } });
            }
            else if (grid == _gridTopVendedores)
            {
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Vendedor", DataPropertyName = "Nombre", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ventas", DataPropertyName = "Cantidad", Width = 80, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total", DataPropertyName = "Monto", Width = 90, DefaultCellStyle = { Format = "C0", Alignment = DataGridViewContentAlignment.MiddleRight } });
            }
            else if (grid == _gridTopCategorias)
            {
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Categoría", DataPropertyName = "Nombre", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Productos", DataPropertyName = "Cantidad", Width = 80, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total", DataPropertyName = "Monto", Width = 90, DefaultCellStyle = { Format = "C0", Alignment = DataGridViewContentAlignment.MiddleRight } });
            }
            else if (grid == _gridTopProvincias)
            {
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Provincia", DataPropertyName = "Nombre", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ventas", DataPropertyName = "Cantidad", Width = 80, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total", DataPropertyName = "Monto", Width = 90, DefaultCellStyle = { Format = "C0", Alignment = DataGridViewContentAlignment.MiddleRight } });
            }
            else if (grid == _gridTopLocalidades)
            {
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Localidad", DataPropertyName = "Nombre", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ventas", DataPropertyName = "Cantidad", Width = 80, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total", DataPropertyName = "Monto", Width = 90, DefaultCellStyle = { Format = "C0", Alignment = DataGridViewContentAlignment.MiddleRight } });
            }
        }

        private void ConfigurarEventos()
        {
            // Eventos de filtros
            dpDesde.ValueChanged += (_, __) => CargarDatos();
            dpHasta.ValueChanged += (_, __) => CargarDatos();
            cboVendedor.SelectedIndexChanged += (_, __) => CargarDatos();
            cboCliente.SelectedIndexChanged += (_, __) => CargarDatos();
            cboCategoria.SelectedIndexChanged += (_, __) => { CargarSubcategorias(); CargarDatos(); };
            cboSubcategoria.SelectedIndexChanged += (_, __) => CargarDatos();
            cboProvincia.SelectedIndexChanged += (_, __) => { CargarLocalidades(); CargarDatos(); };
            cboLocalidad.SelectedIndexChanged += (_, __) => CargarDatos();

            // Eventos de botones
            btnExportar.Click += (_, __) => Exportar();
            btnGraficos.Click += (_, __) => AbrirFormGraficos();
        }

        private void AbrirFormGraficos()
        {
            var graficosForm = new GraficosForm();
            graficosForm.Show(this);
        }

        private Dictionary<string, object> ObtenerFiltrosActivos()
        {
            // Obtener nombres correctos para provincia y localidad
            string provinciaNombre = "Todas";
            string localidadNombre = "Todas";

            if (cboProvincia.SelectedValue is int provinciaId && provinciaId > 0)
            {
                var provincia = cboProvincia.SelectedItem as Provincia;
                provinciaNombre = provincia?.Nombre ?? "Todas";
            }

            if (cboLocalidad.SelectedValue is int localidadIdSel && localidadIdSel > 0)
            {
                var localidad = cboLocalidad.SelectedItem as Localidad;
                localidadNombre = localidad?.Nombre ?? "Todas";
            }

            return new Dictionary<string, object>
            {
                ["Vendedor"] = cboVendedor.SelectedItem?.ToString(),
                ["Cliente"] = cboCliente.SelectedItem?.ToString(),
                ["Categoria"] = cboCategoria.SelectedItem?.ToString(),
                ["Subcategoria"] = cboSubcategoria.SelectedItem?.ToString(),
                ["Provincia"] = provinciaNombre,
                ["Localidad"] = localidadNombre
            };
        }

        private void CargarCombos()
        {
            // Vendedores
            var vendedores = Repository.ListarUsuarios()
                .Where(u => u.Activo)
                .Select(u => u.NombreUsuario)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s)
                .ToList();
            vendedores.Insert(0, "Todos");
            FormUtils.BindCombo(cboVendedor, vendedores, "Todos");

            // Clientes
            var clientes = Repository.Clientes
                .Select(c => c.Nombre)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s)
                .ToList();
            clientes.Insert(0, "Todos");
            FormUtils.BindCombo(cboCliente, clientes, "Todos");

            // Categorías
            var categorias = Repository.ListarCategorias(true)
                .Select(c => c.Nombre)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s)
                .ToList();
            categorias.Insert(0, "Todas");
            FormUtils.BindCombo(cboCategoria, categorias, "Todas");

            // Provincias
            var provincias = Repository.ListarProvincias(true).ToList();
            provincias.Insert(0, new Provincia { Id = 0, Nombre = "Todas" });
            FormUtils.BindCombo(cboProvincia, provincias, selectedValue: 0);

            CargarSubcategorias();
            CargarLocalidades();
        }

        private void CargarSubcategorias()
        {
            if (cboCategoria.SelectedIndex == 0 || cboCategoria.SelectedItem?.ToString() == "Todas")
            {
                // Si no hay categoría específica seleccionada, mostrar todas las subcategorías
                var todasSubcategorias = Repository.ListarSubcategorias(null, false)
                    .Select(s => s.Nombre)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(s => s)
                    .ToList();
                todasSubcategorias.Insert(0, "Todas");
                FormUtils.BindCombo(cboSubcategoria, todasSubcategorias, "Todas");
            }
            else
            {
                // Buscar la categoría seleccionada y filtrar subcategorías
                var categoriaNombre = cboCategoria.SelectedItem?.ToString();
                var categoria = Repository.ListarCategorias(true)
                    .FirstOrDefault(c => string.Equals(c.Nombre, categoriaNombre, StringComparison.OrdinalIgnoreCase));

                if (categoria != null)
                {
                    var subcategorias = Repository.ListarSubcategorias(categoria.Id, false)
                        .Select(s => s.Nombre)
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(s => s)
                        .ToList();
                    subcategorias.Insert(0, "Todas");
                    FormUtils.BindCombo(cboSubcategoria, subcategorias, "Todas");
                }
                else
                {
                    // Si no se encuentra la categoría, limpiar subcategorías
                    var vacia = new List<string> { "Todas" };
                    FormUtils.BindCombo(cboSubcategoria, vacia, "Todas");
                }
            }
        }

        private void CargarLocalidades()
        {
            if (cboProvincia.SelectedIndex == 0 || cboProvincia.SelectedValue == null || (int)cboProvincia.SelectedValue == 0)
            {
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
            try
            {
                // Cargar datos del rango principal
                _ventasFiltradas = CargarVentasPorRango(dpDesde.Value, dpHasta.Value);

                ActualizarTodosLosTops();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                var vend = cboVendedor.SelectedItem?.ToString() ?? string.Empty;
                q = q.Where(v => string.Equals(v.Vendedor, vend, StringComparison.OrdinalIgnoreCase));
            }

            if (cboCliente.SelectedIndex > 0)
            {
                var cliente = cboCliente.SelectedItem?.ToString() ?? string.Empty;
                q = q.Where(v => string.Equals(v.ClienteNombre, cliente, StringComparison.OrdinalIgnoreCase));
            }

            // Filtrar por localidad/provincia
            if (cboLocalidad.SelectedIndex > 0 && cboLocalidad.SelectedValue is int localidadId)
            {
                q = q.Where(v => v.LocalidadId == localidadId); // LocalidadId is nullable, but comparison with int is fine.
            }
            else if (cboProvincia.SelectedIndex > 0 && cboProvincia.SelectedValue is int provinciaId)
            {
                var localidadesEnProvincia = Repository.ListarLocalidades(provinciaId, true)
                    .Select(l => l.Id)
                    .ToHashSet();
                q = q.Where(v => v.LocalidadId.HasValue && localidadesEnProvincia.Contains(v.LocalidadId.Value));
            }

            var ventas = q.ToList();

            // Filtrar por categoría y subcategoría
            if (cboCategoria.SelectedIndex > 0 || cboSubcategoria.SelectedIndex > 0)
            {
                var productos = Repository.ListarProductos(true).ToList();
                var productosIds = new HashSet<int>();

                if (cboSubcategoria.SelectedIndex > 0 && cboSubcategoria.SelectedItem?.ToString() != "Todas")
                {
                    // Filtrar por subcategoría específica
                    var subcategoriaNombre = cboSubcategoria.SelectedItem?.ToString() ?? string.Empty;
                    productosIds = productos
                        .Where(p => string.Equals(p.SubcategoriaNombre, subcategoriaNombre, StringComparison.OrdinalIgnoreCase))
                        .Select(p => p.Id)
                        .ToHashSet();
                }
                else if (cboCategoria.SelectedIndex > 0 && cboCategoria.SelectedItem?.ToString() != "Todas") // This check is fine
                {
                    // Filtrar por categoría específica
                    var categoriaNombre = cboCategoria.SelectedItem!.ToString();
                    productosIds = productos
                        .Where(p => string.Equals(p.CategoriaNombre, categoriaNombre, StringComparison.OrdinalIgnoreCase))
                        .Select(p => p.Id)
                        .ToHashSet();
                }

                if (productosIds.Any())
                {
                    ventas = ventas
                        .Where(v => v.Detalles.Any(d => productosIds.Contains(d.ProductoId)))
                        .ToList();
                }
            }

            return ventas;
        }

        private void ActualizarTodosLosTops()
        {
            // Top Productos
            var topProductos = _ventasFiltradas
                .SelectMany(v => v.Detalles)
                .GroupBy(d => d.ProductoNombre)
                .Select(g => new { Nombre = g.Key, Cantidad = g.Sum(d => d.Cantidad), Monto = g.Sum(d => d.Subtotal) })
                .OrderByDescending(x => x.Monto)
                .Take(15)
                .ToList();
            _topDataSources["Top Productos"] = topProductos;
            _gridTopProductos.DataSource = topProductos;

            // Top Clientes
            var topClientes = _ventasFiltradas
                .GroupBy(v => v.ClienteNombre)
                .Select(g => new { Nombre = g.Key, Cantidad = g.Count(), Monto = g.Sum(v => v.Total) })
                .OrderByDescending(x => x.Monto)
                .Take(15)
                .ToList();
            _topDataSources["Top Clientes"] = topClientes;
            _gridTopClientes.DataSource = topClientes;

            // Top Vendedores
            var topVendedores = _ventasFiltradas
                .GroupBy(v => v.Vendedor)
                .Select(g => new { Nombre = g.Key, Cantidad = g.Count(), Monto = g.Sum(v => v.Total) })
                .OrderByDescending(x => x.Monto)
                .Take(15)
                .ToList();
            _topDataSources["Top Vendedores"] = topVendedores;
            _gridTopVendedores.DataSource = topVendedores;

            // Top Categorías
            var productos = Repository.ListarProductos(true).ToDictionary(p => p.Id, p => p.CategoriaNombre);
            var topCategorias = _ventasFiltradas
                .SelectMany(v => v.Detalles)
                .Where(d => productos.ContainsKey(d.ProductoId))
                .GroupBy(d => productos[d.ProductoId])
                .Select(g => new { Nombre = g.Key, Cantidad = g.Sum(d => d.Cantidad), Monto = g.Sum(d => d.Subtotal) })
                .OrderByDescending(x => x.Monto)
                .Take(15)
                .ToList();
            _topDataSources["Top Categorías"] = topCategorias;
            _gridTopCategorias.DataSource = topCategorias;

            // Top Provincias
            var localidades = Repository.ListarLocalidades(null, true).ToDictionary(l => l.Id, l => l.ProvinciaNombre ?? "Sin Provincia");
            var topProvincias = _ventasFiltradas
                .Where(v => v.LocalidadId.HasValue && localidades.ContainsKey(v.LocalidadId.Value))
                .GroupBy(v => localidades[v.LocalidadId.Value])
                .Select(g => new { Nombre = g.Key, Cantidad = g.Count(), Monto = g.Sum(v => v.Total) })
                .OrderByDescending(x => x.Monto)
                .Take(15)
                .ToList();
            _topDataSources["Top Provincias"] = topProvincias;
            _gridTopProvincias.DataSource = topProvincias;

            // Top Localidades
            var localidadesNombre = Repository.ListarLocalidades(null, true).ToDictionary(l => l.Id, l => l.Nombre);
            var topLocalidades = _ventasFiltradas
                .Where(v => v.LocalidadId.HasValue && localidadesNombre.ContainsKey(v.LocalidadId.Value))
                .GroupBy(v => localidadesNombre[v.LocalidadId.Value])
                .Select(g => new { Nombre = g.Key, Cantidad = g.Count(), Monto = g.Sum(v => v.Total) })
                .OrderByDescending(x => x.Monto)
                .Take(15)
                .ToList();
            _topDataSources["Top Localidades"] = topLocalidades;
            _gridTopLocalidades.DataSource = topLocalidades;
        }

        #region Exportación

        private void Exportar()
        {
            if (!_ventasFiltradas.Any())
            {
                MessageBox.Show("No hay datos para exportar.", "Sin Datos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var topNames = _topsDefinition.Select(t => t.titulo);
            using var optionsForm = new ExportOptionsForm(topNames);

            if (optionsForm.ShowDialog(this) != DialogResult.OK || optionsForm.SelectedFormat == ExportOptionsForm.ExportFormat.None)
            {
                return;
            }

            var selectedTops = optionsForm.SelectedTops;
            var format = optionsForm.SelectedFormat;

            if (!selectedTops.Any())
            {
                MessageBox.Show("Debe seleccionar al menos un ranking para exportar.", "Selección Vacía", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string filter = format == ExportOptionsForm.ExportFormat.Excel ? "Excel Workbook (*.xlsx)|*.xlsx" : "PDF Document (*.pdf)|*.pdf";
            string extension = format == ExportOptionsForm.ExportFormat.Excel ? ".xlsx" : ".pdf";

            using var sfd = new SaveFileDialog
            {
                Filter = filter,
                FileName = $"Estadisticas_{DateTime.Now:yyyyMMdd_HHmmss}{extension}"
            };

            if (sfd.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                if (format == ExportOptionsForm.ExportFormat.Pdf)
                    ExportarPdf(sfd.FileName, selectedTops);
                else
                    ExportarExcel(sfd.FileName, selectedTops);

                MessageBox.Show("Archivo exportado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportarPdf(string filePath, List<string> selectedTops)
        {
            try
            {
                // Configurar QuestPDF para usar la licencia Community
                QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

                var filtros = ObtenerFiltrosActivos();

                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(30);

                        // Encabezado
                        page.Header().Element(header =>
                        {
                            header.Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("Reporte de Estadísticas").Bold().FontSize(20);
                                    col.Item().Text($"Generado: {DateTime.Now:g} por {Sesion.Usuario}");
                                    col.Item().Text($"Período: {dpDesde.Value:d} - {dpHasta.Value:d}");
                                });

                                // Cargar logo desde recursos embebidos
                                try
                                {
                                    var logoResource = Properties.Resources.logo;
                                    if (logoResource != null)
                                    {
                                        using var ms = new MemoryStream();
                                        logoResource.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                        ms.Position = 0;
                                        
                                        row.ConstantItem(100).AlignRight().Image(ms.ToArray(), ImageScaling.FitArea);
                                    }
                                }
                                catch
                                {
                                    // Si falla cargar el logo, continuar sin él
                                    // Intentar método alternativo con archivo
                                    try
                                    {
                                        var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "recursos", "logo.png");
                                        if (File.Exists(logoPath))
                                        {
                                            row.ConstantItem(100).AlignRight().Image(logoPath, ImageScaling.FitArea);
                                        }
                                        else
                                        {
                                            // Buscar en el directorio base
                                            logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.png");
                                            if (File.Exists(logoPath))
                                            {
                                                row.ConstantItem(100).AlignRight().Image(logoPath, ImageScaling.FitArea);
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        // Si no se puede cargar ningún logo, continuar sin él
                                    }
                                }
                            });
                        });

                        // Contenido
                        page.Content().Element(body =>
                        {
                            body.Column(col =>
                            {
                                // Mostrar filtros aplicados (sin fechas)
                                col.Item().PaddingBottom(10).Text("Filtros Aplicados:").Bold().FontSize(12);

                                var hayFiltrosEspecificos = false;

                                foreach (var filtro in filtros)
                                {
                                    if (filtro.Value != null && filtro.Value.ToString() != "Todos" && filtro.Value.ToString() != "Todas")
                                    {
                                        var nombreFiltro = filtro.Key switch
                                        {
                                            "Vendedor" => "Vendedor",
                                            "Cliente" => "Cliente",
                                            "Categoria" => "Categoría",
                                            "Subcategoria" => "Subcategoría",
                                            "Provincia" => "Provincia",
                                            "Localidad" => "Localidad",
                                            _ => filtro.Key
                                        };
                                        col.Item().Text($"{nombreFiltro}: {filtro.Value}").FontSize(10);
                                        hayFiltrosEspecificos = true;
                                    }
                                    else if (filtro.Value != null)
                                    {
                                        var nombreFiltro = filtro.Key switch
                                        {
                                            "Vendedor" => "Vendedor",
                                            "Cliente" => "Cliente",
                                            "Categoria" => "Categoría",
                                            "Subcategoria" => "Subcategoría",
                                            "Provincia" => "Provincia",
                                            "Localidad" => "Localidad",
                                            _ => filtro.Key
                                        };
                                        col.Item().Text($"{nombreFiltro}: Todos").FontSize(10);
                                    }
                                }

                                if (!hayFiltrosEspecificos)
                                {
                                    col.Item().Text("Mostrando todos los datos sin filtros específicos").FontSize(10).Italic();
                                }

                                col.Item().PaddingVertical(10).LineHorizontal(1);

                                foreach (var topName in selectedTops)
                                {
                                    col.Item().PaddingTop(20).Text(topName).Bold().FontSize(14);
                                    var grid = _topGrids[topName];
                                    var datos = _topDataSources.TryGetValue(topName, out var data) ? data.Cast<object>().ToList() : new List<object>();

                                    if (!datos.Any())
                                    {
                                        col.Item().Text("No hay datos para mostrar.").FontSize(10).Italic();
                                        continue;
                                    }

                                    col.Item().Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            for (int i = 0; i < grid.Columns.Count; i++)
                                                columns.RelativeColumn();
                                        });

                                        table.Header(header =>
                                        {
                                            foreach (DataGridViewColumn column in grid.Columns)
                                            {
                                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text(column.HeaderText).Bold();
                                            }
                                        });

                                        foreach (var item in datos)
                                        {
                                            var properties = item.GetType().GetProperties();
                                            foreach (var prop in properties)
                                            {
                                                var value = prop.GetValue(item);
                                                var cell = table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5);

                                                if (prop.Name == "Monto" && (value is decimal || value is double || value is float))
                                                    cell.AlignRight().Text(((decimal)value).ToString("C0", _culture));
                                                else if (prop.Name == "Cantidad")
                                                    cell.AlignRight().Text(value?.ToString() ?? "");
                                                else
                                                    cell.Text(value?.ToString() ?? "");
                                            }
                                        }
                                    });
                                }
                            });
                        });

                        // Pie de página
                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                        });
                    });
                }).GeneratePdf(filePath);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar PDF: {ex.Message}", ex);
            }
        }

        private void ExportarExcel(string filePath, List<string> selectedTops)
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Reporte de Estadísticas");

            // --- Página / márgenes / escala ---
            ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            ws.PageSetup.Margins.Left = 0.2;
            ws.PageSetup.Margins.Right = 0.2;
            ws.PageSetup.Margins.Top = 0.4;
            ws.PageSetup.Margins.Bottom = 0.7;
            ws.PageSetup.Margins.Header = 0.3;
            ws.PageSetup.Margins.Footer = 0.5;
            ws.PageSetup.FitToPages(1, 0);
            ws.ShowGridLines = false;

            // Mantener tamaño del header/footer aunque se escale la hoja
            ws.PageSetup.ScaleHFWithDocument = false;
            ws.PageSetup.AlignHFWithMargins = true;

            // --- Anchuras de columnas (3 columnas reales para las tablas) ---
            ws.Column(1).Width = 40; // Nombre - Reducido para dar más espacio a segunda columna
            ws.Column(2).Width = 15; // Cantidad/Compras/Ventas/Productos - Aumentado para títulos completos
            ws.Column(3).Width = 40.71; // Total

            int row = 1;

            // ==== ENCABEZADO CON TÍTULO AZUL CENTRADO ====
            // Título en azul que se extiende por toda la hoja (columnas A, B, C)
            EstiloTitulo(ws.Range(row, 1, row, 3), "REPORTE DE ESTADÍSTICAS");
            row++;

            // Primera fila de información: "Generado: valor" en A, logo en C
            ws.Cell(row, 1).Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm} por {Sesion.Usuario ?? Environment.UserName}";
            ws.Cell(row, 1).Style.Font.SetBold();
            ws.Cell(row, 1).Style.Font.FontSize = 11;
            
            // Logo en la columna C, centrado
            try
            {
                var logo = Properties.Resources.logo;
                if (logo != null)
                {
                    using var ms = new MemoryStream();
                    logo.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;
                    var picture = ws.AddPicture(ms);
                    // Colocar el logo en la columna C, centrado
                    picture.MoveTo(ws.Cell(row, 3), 15, 5);
                    picture.Scale(0.6);
                }
            }
            catch 
            { 
                // Si no hay logo, continuar sin él
            }
            
            // Ajustar altura de la fila para acomodar logo
            ws.Row(row).Height = 40;
            row++;
            
            // Segunda fila de información: "Período: fechas" en A
            ws.Cell(row, 1).Value = $"Período: {dpDesde.Value:dd/MM/yyyy} - {dpHasta.Value:dd/MM/yyyy}";
            ws.Cell(row, 1).Style.Font.SetBold();
            ws.Cell(row, 1).Style.Font.FontSize = 11;

            // ==== FILTROS APLICADOS ====
            row += 2;
            var filtros = ObtenerFiltrosActivos();
            var banda = ws.Range(row, 1, row, 3);
            banda.Merge().Value = "Filtros Aplicados:";
            banda.Style.Fill.BackgroundColor = XLColor.FromHtml("#D9D9D9");
            banda.Style.Font.SetBold();
            row++;

            bool hayEspecificos = false;
            foreach (var f in filtros)
            {
                var v = f.Value?.ToString();
                if (!string.IsNullOrWhiteSpace(v) && v != "Todos" && v != "Todas")
                {
                    ws.Cell(row, 1).Value = $"• {NombreFiltroLegible(f.Key)}:";
                    ws.Cell(row, 1).Style.Font.SetBold();
                    ws.Cell(row, 2).Value = v;
                    row++;
                    hayEspecificos = true;
                }
            }
            if (!hayEspecificos)
            {
                ws.Cell(row, 1).Value = "• Sin filtros específicos aplicados";
                ws.Cell(row, 1).Style.Font.Italic = true;
                row++;
            }

            row += 1;

            // ==== BLOQUES TOP ====
            foreach (var topName in selectedTops)
            {
                var datos = _topDataSources.TryGetValue(topName, out var data)
                    ? data?.Cast<object>()?.ToList() ?? new List<object>()
                    : new List<object>();

                if (!datos.Any())
                {
                    // Título
                    EstiloBarraSeccion(ws.Range(row, 1, row, 3), topName);
                    row += 1;
                    ws.Cell(row, 1).Value = "No hay datos para mostrar.";
                    ws.Cell(row, 1).Style.Font.Italic = true;
                    row += 2;
                    continue;
                }

                // Título
                EstiloBarraSeccion(ws.Range(row, 1, row, 3), topName);
                row++;

                // Encabezados de tabla según top - TÍTULOS COMPLETOS
                string col2Header = topName switch
                {
                    "Top Clientes" => "Compras",
                    "Top Vendedores" => "Ventas",
                    "Top Categorías" => "Productos",
                    _ => "Cantidad"
                };

                // Header
                ws.Cell(row, 1).Value = (topName.Contains("Productos") ? "Producto" :
                                         topName.Contains("Clientes") ? "Cliente" :
                                         topName.Contains("Vendedores") ? "Vendedor" :
                                         topName.Contains("Categorías") ? "Categoría" :
                                         "Nombre");
                ws.Cell(row, 2).Value = col2Header;
                ws.Cell(row, 3).Value = "Total";
                EstiloHeaderTabla(ws.Range(row, 1, row, 3));
                row++;

                // Datos
                // Asegurar orden por Monto desc
                var ordenados = datos
                    .OrderByDescending(o => Convert.ToDecimal(
                        o.GetType().GetProperty("Monto")?.GetValue(o) ?? 0m))
                    .ToList();

                int dataStart = row;
                for (int i = 0; i < ordenados.Count; i++)
                {
                    var item = ordenados[i];
                    var t = item.GetType();

                    var nombre = t.GetProperty("Nombre")?.GetValue(item)?.ToString() ?? "";
                    var cantidad = t.GetProperty("Cantidad")?.GetValue(item);
                    var montoObj = t.GetProperty("Monto")?.GetValue(item);

                    ws.Cell(row, 1).Value = nombre;

                    if (cantidad is IConvertible)
                    {
                        ws.Cell(row, 2).Value = Convert.ToInt32(cantidad);
                        ws.Cell(row, 2).Style.NumberFormat.Format = "#,##0";
                        ws.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    else
                    {
                        ws.Cell(row, 2).Value = cantidad?.ToString() ?? "";
                    }

                    if (montoObj is IConvertible)
                    {
                        ws.Cell(row, 3).Value = Convert.ToDecimal(montoObj);
                        ws.Cell(row, 3).Style.NumberFormat.Format = "$ #,##0";
                        ws.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        ws.Cell(row, 3).Style.Font.SetBold();
                    }
                    else
                    {
                        ws.Cell(row, 3).Value = montoObj?.ToString() ?? "";
                        ws.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    }

                    // Zebra
                    if (i % 2 == 1)
                        ws.Range(row, 1, row, 3).Style.Fill.BackgroundColor = XLColor.FromHtml("#F8F9FA");

                    ws.Row(row).Height = 18;
                    row++;
                }

                // Bordes + autofiltro + alineaciones
                var rangoTabla = ws.Range(dataStart - 1, 1, row - 1, 3); // incluye header
                rangoTabla.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                rangoTabla.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                rangoTabla.SetAutoFilter();

                // Espacio entre bloques
                row += 1;
            }

            // Dejar 1 fila en blanco como colchón antes del pie
            row++;
            ws.Row(row).Height = 12;

            // ==== Header/Footer de página ====
            // Quitar cualquier header de impresión (no quiero texto negro arriba)
            ws.PageSetup.Header.Left.Clear();
            ws.PageSetup.Header.Center.Clear();
            ws.PageSetup.Header.Right.Clear();

            // Footer correcto
            ws.PageSetup.Footer.Left.Clear();
            ws.PageSetup.Footer.Center.Clear();
            ws.PageSetup.Footer.Right.Clear();

            ws.PageSetup.Footer.Left.AddText($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}");
            ws.PageSetup.Footer.Center.AddText("Página &P de &N");
            ws.PageSetup.Footer.Right.AddText(Sesion.Usuario ?? Environment.UserName);

            // Área de impresión hasta la última fila escrita 
            ws.PageSetup.PrintAreas.Clear();
            ws.PageSetup.PrintAreas.Add($"A1:C{row}");
            ws.SheetView.ZoomScale = 90;

            wb.SaveAs(filePath);
        }

        private static void EstiloTitulo(IXLRange rango, string texto)
        {
            rango.Merge();
            rango.Value = texto;
            rango.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E73BE"); // Azul
            rango.Style.Font.SetBold();
            rango.Style.Font.FontColor = XLColor.White;
            rango.Style.Font.FontSize = 14;
            rango.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            rango.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            rango.Worksheet.Row(rango.FirstRow().RowNumber()).Height = 28;
        }

        private static void EstiloBarraSeccion(IXLRange rango, string texto)
        {
            rango.Merge();
            rango.Value = texto;
            rango.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E73BE");
            rango.Style.Font.SetBold();
            rango.Style.Font.FontColor = XLColor.White;
            rango.Style.Font.FontSize = 12;
            rango.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            rango.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            rango.Worksheet.Row(rango.FirstRow().RowNumber()).Height = 22;
        }

        private static void EstiloHeaderTabla(IXLRange rango)
        {
            rango.Style.Fill.BackgroundColor = XLColor.FromHtml("#6C757D");
            rango.Style.Font.SetBold();
            rango.Style.Font.FontColor = XLColor.White;
            rango.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            rango.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            rango.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            rango.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        }

        private static string NombreFiltroLegible(string key)
        {
            return key switch
            {
                "Vendedor" => "Vendedor",
                "Cliente" => "Cliente",
                "Categoria" => "Categoría",
                "Subcategoria" => "Subcategoría",
                "Provincia" => "Provincia",
                "Localidad" => "Localidad",
                _ => key
            };
        }

        #endregion
    }
}