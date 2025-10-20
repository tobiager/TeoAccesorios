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
            var tops = new (string titulo, string icono, DataGridView grid, DrawingColor color)[]
            {
                ("Top Productos", "📦", _gridTopProductos, DrawingColor.FromArgb(220, 53, 69)),
                ("Top Clientes", "👥", _gridTopClientes, DrawingColor.FromArgb(0, 120, 215)),
                ("Top Vendedores", "🏆", _gridTopVendedores, DrawingColor.FromArgb(255, 193, 7)),
                ("Top Categorías", "📋", _gridTopCategorias, DrawingColor.FromArgb(40, 167, 69)),
                ("Top Provincias", "🌍", _gridTopProvincias, DrawingColor.FromArgb(108, 117, 125)),
                ("Top Localidades", "📍", _gridTopLocalidades, DrawingColor.FromArgb(111, 66, 193))
            };

            for (int i = 0; i < tops.Length; i++)
            {
                var card = CrearCardTop(tops[i].titulo, tops[i].icono, tops[i].grid, tops[i].color);
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
            return new Dictionary<string, object>
            {
                ["FechaDesde"] = dpDesde.Value,
                ["FechaHasta"] = dpHasta.Value,
                ["Vendedor"] = cboVendedor.SelectedItem?.ToString(),
                ["Cliente"] = cboCliente.SelectedItem?.ToString(),
                ["Categoria"] = cboCategoria.SelectedItem?.ToString(),
                ["Subcategoria"] = cboSubcategoria.SelectedItem?.ToString(),
                ["Provincia"] = cboProvincia.SelectedItem?.ToString(),
                ["Localidad"] = cboLocalidad.SelectedItem?.ToString()
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
            _gridTopProductos.DataSource = topProductos;

            // Top Clientes
            var topClientes = _ventasFiltradas
                .GroupBy(v => v.ClienteNombre)
                .Select(g => new { Nombre = g.Key, Cantidad = g.Count(), Monto = g.Sum(v => v.Total) })
                .OrderByDescending(x => x.Monto)
                .Take(15)
                .ToList();
            _gridTopClientes.DataSource = topClientes;

            // Top Vendedores
            var topVendedores = _ventasFiltradas
                .GroupBy(v => v.Vendedor)
                .Select(g => new { Nombre = g.Key, Cantidad = g.Count(), Monto = g.Sum(v => v.Total) })
                .OrderByDescending(x => x.Monto)
                .Take(15)
                .ToList();
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

            using var sfd = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx|PDF Document (*.pdf)|*.pdf",
                FileName = $"Estadisticas_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    if (Path.GetExtension(sfd.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                        ExportarPdf(sfd.FileName);
                    else
                        ExportarExcel(sfd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al exportar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExportarExcel(string filePath)
        {
            using var wb = new XLWorkbook();

            // Crear hojas para cada top
            var tops = new (string nombre, IEnumerable<dynamic> datos)[]
            {
                ("Top Productos", (IEnumerable<dynamic>)_gridTopProductos.DataSource ?? new List<dynamic>()),
                ("Top Clientes", (IEnumerable<dynamic>)_gridTopClientes.DataSource ?? new List<dynamic>()),
                ("Top Vendedores", (IEnumerable<dynamic>)_gridTopVendedores.DataSource ?? new List<dynamic>()),
                ("Top Categorías", (IEnumerable<dynamic>)_gridTopCategorias.DataSource ?? new List<dynamic>()),
                ("Top Provincias", (IEnumerable<dynamic>)_gridTopProvincias.DataSource ?? new List<dynamic>()),
                ("Top Localidades", (IEnumerable<dynamic>)_gridTopLocalidades.DataSource ?? new List<dynamic>())
            };

            foreach (var (nombre, datos) in tops)
            {
                var ws = wb.AddWorksheet(nombre);
                if (datos.Any())
                {
                    ws.Cell(1, 1).InsertTable(datos);
                    ws.Column(3).Style.NumberFormat.Format = "$ #,##0.00";
                    ws.Columns().AdjustToContents();
                }
            }

            wb.SaveAs(filePath);
            MessageBox.Show("Excel exportado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExportarPdf(string filePath)
        {
            MessageBox.Show("Funcionalidad de PDF en desarrollo.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion
    }
}