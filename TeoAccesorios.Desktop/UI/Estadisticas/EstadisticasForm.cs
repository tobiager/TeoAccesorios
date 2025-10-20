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
                grid.RowHeadersVisible = false;
                grid.AllowUserToAddRows = false;
                grid.AllowUserToDeleteRows = false;
                grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                grid.MultiSelect = false;
                grid.BackgroundColor = DrawingColor.White;
                grid.GridColor = DrawingColor.FromArgb(233, 236, 239);
                grid.DefaultCellStyle.SelectionBackColor = DrawingColor.FromArgb(0, 120, 215);
                grid.DefaultCellStyle.SelectionForeColor = DrawingColor.White;
                
                ConfigurarColumnasGrid(grid);
            }
        }

        private void ConfigurarColumnasGrid(DataGridView grid)
        {
            grid.Columns.Clear();

            if (grid == _gridTopProductos)
            {
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Producto", DataPropertyName = "Nombre", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cant.", DataPropertyName = "Cantidad", Width = 60, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total", DataPropertyName = "Monto", Width = 90, DefaultCellStyle = { Format = "C0", Alignment = DataGridViewContentAlignment.MiddleRight } });
            }
            else if (grid == _gridTopClientes)
            {
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cliente", DataPropertyName = "Nombre", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Comp.", DataPropertyName = "Cantidad", Width = 60, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total", DataPropertyName = "Monto", Width = 90, DefaultCellStyle = { Format = "C0", Alignment = DataGridViewContentAlignment.MiddleRight } });
            }
            else if (grid == _gridTopVendedores)
            {
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Vendedor", DataPropertyName = "Nombre", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ventas", DataPropertyName = "Cantidad", Width = 60, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total", DataPropertyName = "Monto", Width = 90, DefaultCellStyle = { Format = "C0", Alignment = DataGridViewContentAlignment.MiddleRight } });
            }
            else if (grid == _gridTopCategorias)
            {
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Categoría", DataPropertyName = "Nombre", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Prod.", DataPropertyName = "Cantidad", Width = 60, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total", DataPropertyName = "Monto", Width = 90, DefaultCellStyle = { Format = "C0", Alignment = DataGridViewContentAlignment.MiddleRight } });
            }
            else if (grid == _gridTopProvincias)
            {
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Provincia", DataPropertyName = "Nombre", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ventas", DataPropertyName = "Cantidad", Width = 60, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total", DataPropertyName = "Monto", Width = 90, DefaultCellStyle = { Format = "C0", Alignment = DataGridViewContentAlignment.MiddleRight } });
            }
            else if (grid == _gridTopLocalidades)
            {
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Localidad", DataPropertyName = "Nombre", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ventas", DataPropertyName = "Cantidad", Width = 60, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
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

            // Filtrar por categoría y subcategoría
            if (cboCategoria.SelectedIndex > 0 || cboSubcategoria.SelectedIndex > 0)
            {
                var productos = Repository.ListarProductos(true).ToList();
                var productosIds = new HashSet<int>();

                if (cboSubcategoria.SelectedIndex > 0 && cboSubcategoria.SelectedItem?.ToString() != "Todas")
                {
                    // Filtrar por subcategoría específica
                    var subcategoriaNombre = cboSubcategoria.SelectedItem!.ToString();
                    productosIds = productos
                        .Where(p => string.Equals(p.SubcategoriaNombre, subcategoriaNombre, StringComparison.OrdinalIgnoreCase))
                        .Select(p => p.Id)
                        .ToHashSet();
                }
                else if (cboCategoria.SelectedIndex > 0 && cboCategoria.SelectedItem?.ToString() != "Todas")
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

            // Esta validación adicional no debería ser necesaria ya que el formulario la maneja,
            // pero la mantenemos por seguridad
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
            var filtros = ObtenerFiltrosActivos();

            // Crear una sola hoja para todos los rankings
            var ws = wb.AddWorksheet("Reporte de Estadísticas");

            // Configurar impresión para usar todo el ancho
            ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            ws.PageSetup.Margins.Left = 0.3;
            ws.PageSetup.Margins.Right = 0.3;
            ws.PageSetup.Margins.Top = 0.5;
            ws.PageSetup.Margins.Bottom = 0.5;
            ws.PageSetup.FitToPages(1, 0); // Ajustar a 1 página de ancho

            // --- ENCABEZADO MEJORADO ---
            // Título principal con barra azul - usar más columnas para ancho completo
            ws.Cell("A1").Value = "REPORTE DE ESTADÍSTICAS";
            ws.Range("A1:J1").Merge(); // Usar más columnas para aprovechar el ancho
            ws.Cell("A1").Style.Font.SetBold(true);
            ws.Cell("A1").Style.Font.FontSize = 22; // Fuente más grande
            ws.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("A1").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell("A1").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 120, 215);
            ws.Cell("A1").Style.Font.FontColor = XLColor.White;
            ws.Row(1).Height = 35; // Hacer la fila más alta

            // Logo mejorado - más grande y prominente
            try
            {
                var logoResource = Properties.Resources.logo;
                if (logoResource != null)
                {
                    using var ms = new MemoryStream();
                    logoResource.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;
                    
                    var picture = ws.AddPicture(ms)
                        .MoveTo(ws.Cell("K1"))
                        .Scale(0.8); // Logo más grande
                    
                    // Ajustar la altura de la fila para el logo
                    ws.Row(1).Height = Math.Max(ws.Row(1).Height, 50);
                }
            }
            catch
            {
                // Intentar cargar desde archivo
                try
                {
                    var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "recursos", "logo.png");
                    if (File.Exists(logoPath))
                    {
                        var picture = ws.AddPicture(logoPath)
                            .MoveTo(ws.Cell("K1"))
                            .Scale(0.8);
                        ws.Row(1).Height = Math.Max(ws.Row(1).Height, 50);
                    }
                    else
                    {
                        logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.png");
                        if (File.Exists(logoPath))
                        {
                            var picture = ws.AddPicture(logoPath)
                                .MoveTo(ws.Cell("K1"))
                                .Scale(0.8);
                            ws.Row(1).Height = Math.Max(ws.Row(1).Height, 50);
                        }
                    }
                }
                catch { }
            }

            // Información del reporte con fuentes más grandes
            int currentRow = 3;
            ws.Cell(currentRow, 1).Value = "Generado:";
            ws.Cell(currentRow, 1).Style.Font.SetBold();
            ws.Cell(currentRow, 1).Style.Font.FontSize = 12; // Fuente más grande
            ws.Cell(currentRow, 2).Value = $"{DateTime.Now:dd/MM/yyyy HH:mm} por {Sesion.Usuario ?? Environment.UserName}";
            ws.Cell(currentRow, 2).Style.Font.FontSize = 12;
            
            currentRow++;
            ws.Cell(currentRow, 1).Value = "Período:";
            ws.Cell(currentRow, 1).Style.Font.SetBold();
            ws.Cell(currentRow, 1).Style.Font.FontSize = 12;
            ws.Cell(currentRow, 2).Value = $"{dpDesde.Value:dd/MM/yyyy} - {dpHasta.Value:dd/MM/yyyy}";
            ws.Cell(currentRow, 2).Style.Font.FontSize = 12;

            // --- FILTROS APLICADOS ---
            currentRow += 2;
            ws.Cell(currentRow, 1).Value = "Filtros Aplicados:";
            ws.Cell(currentRow, 1).Style.Font.SetBold();
            ws.Cell(currentRow, 1).Style.Font.FontSize = 14; // Fuente más grande para sección
            ws.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
            ws.Range(currentRow, 1, currentRow, 10).Merge(); // Usar más columnas
            currentRow++;

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
                    ws.Cell(currentRow, 1).Value = $"• {nombreFiltro}:";
                    ws.Cell(currentRow, 1).Style.Font.SetBold();
                    ws.Cell(currentRow, 1).Style.Font.FontSize = 11;
                    ws.Cell(currentRow, 2).Value = filtro.Value.ToString();
                    ws.Cell(currentRow, 2).Style.Font.FontSize = 11;
                    currentRow++;
                    hayFiltrosEspecificos = true;
                }
            }

            if (!hayFiltrosEspecificos)
            {
                ws.Cell(currentRow, 1).Value = "• Sin filtros específicos aplicados";
                ws.Cell(currentRow, 1).Style.Font.Italic = true;
                ws.Cell(currentRow, 1).Style.Font.FontSize = 11;
                currentRow++;
            }

            currentRow += 2;

            // --- PROCESAR CADA RANKING CON TABLAS DE ANCHO COMPLETO ---
            foreach (var topName in selectedTops)
            {
                var datos = _topDataSources.TryGetValue(topName, out var data) ? data : new List<dynamic>();
                var grid = _topGrids[topName];

                // Título del ranking con estilo mejorado
                ws.Cell(currentRow, 1).Value = topName;
                ws.Range(currentRow, 1, currentRow, 10).Merge(); // Usar todo el ancho
                ws.Cell(currentRow, 1).Style.Font.SetBold(true);
                ws.Cell(currentRow, 1).Style.Font.FontSize = 16; // Fuente más grande
                ws.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.FromArgb(0, 120, 215);
                ws.Cell(currentRow, 1).Style.Font.FontColor = XLColor.White;
                ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                ws.Row(currentRow).Height = 25; // Hacer la fila del título más alta
                currentRow += 2;

                if (datos.Any())
                {
                    // Configurar columnas para usar todo el ancho disponible
                    int totalColumns = grid.Columns.Count;
                    int startCol = 1;
                    int endCol = Math.Max(10, totalColumns + 2); // Asegurar que use al menos 10 columnas
                    
                    // Calcular anchos de columna para distribución equitativa
                    double[] columnWidths = new double[totalColumns];
                    if (totalColumns == 3) // Típico: Nombre, Cantidad, Monto
                    {
                        columnWidths[0] = 50; // Columna de nombre más ancha
                        columnWidths[1] = 15; // Cantidad
                        columnWidths[2] = 20; // Monto
                    }
                    else
                    {
                        // Distribución equitativa para otros casos
                        for (int i = 0; i < totalColumns; i++)
                        {
                            columnWidths[i] = 85.0 / totalColumns;
                        }
                    }

                    // Encabezados de columna con mejor formato y fuentes más grandes
                    int col = startCol;
                    for (int i = 0; i < grid.Columns.Count; i++)
                    {
                        var headerCell = ws.Cell(currentRow, col);
                        headerCell.Value = grid.Columns[i].HeaderText;
                        headerCell.Style.Font.SetBold(true);
                        headerCell.Style.Font.FontSize = 13; // Fuente más grande para headers
                        headerCell.Style.Fill.BackgroundColor = XLColor.FromArgb(108, 117, 125);
                        headerCell.Style.Font.FontColor = XLColor.White;
                        headerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        headerCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        headerCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        
                        // Aplicar ancho de columna
                        ws.Column(col).Width = columnWidths[i];
                        
                        col++;
                    }
                    ws.Row(currentRow).Height = 20; // Hacer las filas de header más altas
                    currentRow++;

                    // Filas de datos con formato alternado y fuentes más grandes
                    bool isEvenRow = false;
                    foreach (var item in datos)
                    {
                        var properties = item.GetType().GetProperties();
                        col = startCol;
                        
                        foreach (var prop in properties)
                        {
                            var value = prop.GetValue(item);
                            var cell = ws.Cell(currentRow, col);
                            
                            // Formato de fondo alternado
                            if (isEvenRow)
                            {
                                cell.Style.Fill.BackgroundColor = XLColor.FromArgb(248, 249, 250);
                            }
                            
                            // Fuente más grande para datos
                            cell.Style.Font.FontSize = 11;
                            
                            if (prop.Name == "Monto" && (value is decimal || value is double || value is float))
                            {
                                cell.Value = (decimal)value;
                                cell.Style.NumberFormat.Format = "$ #,##0";
                                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                cell.Style.Font.SetBold();
                            }
                            else if (prop.Name == "Cantidad")
                            {
                                if (int.TryParse(value?.ToString(), out int cantidad))
                                {
                                    cell.Value = cantidad;
                                    cell.Style.NumberFormat.Format = "#,##0";
                                }
                                else
                                {
                                    cell.Value = value?.ToString() ?? "";
                                }
                                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            }
                            else
                            {
                                cell.Value = value?.ToString() ?? "";
                                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                cell.Style.Alignment.WrapText = true; // Permitir ajuste de texto
                            }
                            
                            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            col++;
                        }
                        
                        ws.Row(currentRow).Height = 18; // Filas de datos más altas
                        currentRow++;
                        isEvenRow = !isEvenRow;
                    }

                    // Línea de separación después de cada ranking
                    if (topName != selectedTops.Last())
                    {
                        currentRow++;
                        ws.Range(currentRow, 1, currentRow, 10).Style.Border.TopBorder = XLBorderStyleValues.Medium;
                        ws.Range(currentRow, 1, currentRow, 10).Style.Border.TopBorderColor = XLColor.FromArgb(200, 200, 200);
                        currentRow += 2;
                    }
                }
                else
                {
                    ws.Cell(currentRow, 1).Value = "No hay datos para mostrar";
                    ws.Cell(currentRow, 1).Style.Font.Italic = true;
                    ws.Cell(currentRow, 1).Style.Font.FontSize = 12;
                    ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(currentRow, 1, currentRow, 10).Merge();
                    currentRow += 3;
                }
            }

            // --- PIE DE PÁGINA MEJORADO ---
            ws.PageSetup.Header.Center.AddText("&\"Arial,Bold\"&16REPORTE DE ESTADÍSTICAS"); // Fuente más grande
            ws.PageSetup.Footer.Left.AddText($"&\"Arial\"&12Generado: {DateTime.Now:dd/MM/yyyy HH:mm}"); // Fuente más grande
            ws.PageSetup.Footer.Center.AddText("&\"Arial\"&12Página &P de &N");
            ws.PageSetup.Footer.Right.AddText($"&\"Arial\"&12{Sesion.Usuario ?? Environment.UserName}");

            // Configuración final para usar todo el ancho
            ws.PageSetup.PrintAreas.Add($"A1:J{currentRow - 1}"); // Área de impresión más amplia
            ws.ShowGridLines = false;

            // Configurar zoom para mejor visualización
            ws.SheetView.ZoomScale = 85;

            // Guardar archivo
            wb.SaveAs(filePath);
        }

        #endregion
    }
}