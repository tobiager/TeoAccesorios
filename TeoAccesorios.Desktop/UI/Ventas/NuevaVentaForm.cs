using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using TeoAccesorios.Desktop.UI.Common;
using TeoAccesorios.Desktop.Models; // Ya existe
using System.Drawing;
using System.Reflection;

namespace TeoAccesorios.Desktop
{
    public class NuevaVentaForm : Form
    {
        //  UI principal 
        private readonly ComboBox cboCliente = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };
        private readonly ComboBox cboProducto = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 320 };
        private readonly NumericUpDown numCant = new() { Minimum = 1, Maximum = 1000, Value = 1, Width = 90, TextAlign = HorizontalAlignment.Right };
        private readonly ComboBox cboCanal = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 170 };
        private readonly TextBox txtDireccionEnvio = new() { Width = 420, PlaceholderText = "Dirección de envío" };
        
        // Nuevos controles de dirección
        private readonly CheckBox chkCambiarDireccion = new() { Text = "Usar otra dirección de envío", AutoSize = true };
        private readonly ComboBox cboProvinciaVenta = new() { Width = 200, Visible = false };
        private readonly ComboBox cboLocalidadVenta = new() { Width = 200, Visible = false };
        private int? _clienteProvinciaId;
        private int? _clienteLocalidadId;        
        private readonly Button btnAgregar = new() { Text = "Agregar", AutoSize = true };
        private readonly Button btnQuitar = new() { Text = "Quitar", AutoSize = true };
        private readonly Button btnGuardar = new() { Text = "Guardar", AutoSize = true };

        //  Grilla (solo lectura + legible) 
        private readonly DataGridView _gridDetalles = new()
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            ReadOnly = true,                     
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            EnableHeadersVisualStyles = false,
            BorderStyle = BorderStyle.None,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single,
            GridColor = Color.FromArgb(203, 213, 225),
            BackgroundColor = Color.White
        };

        // Totales (más grandes)
        private readonly Label lblTotal = new()
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 13.5F, FontStyle.Bold),
            Text = "Total: $0,00"
        };
        private readonly Label lblItems = new()
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 10.5F, FontStyle.Regular),
            Text = "Ítems: 0"
        };

        private readonly BindingSource bs = new();
        private readonly Venta _ventaActual = new();
        private List<Cliente> _clientes = new();
        private List<Producto> _productos = new();
        private readonly CultureInfo _culture = new("es-AR");
        

        public NuevaVentaForm()
        {
            Text = "Nueva Venta";
            Width = 1200;
            Height = 680;
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 10F);

            // --- Fila 1: cliente / canal / dirección
            var fila1 = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 56, Padding = new Padding(12, 10, 12, 6), WrapContents = false };
            fila1.Controls.AddRange(new Control[]
            {
                Etiqueta("Cliente"), cboCliente
            });

            // --- Fila 1.5: Dirección
            var fila1_5 = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 56, Padding = new Padding(12, 6, 12, 10), WrapContents = false };
            fila1_5.Controls.AddRange(new Control[]
            {
                Etiqueta("Dirección"), txtDireccionEnvio,
                Separador(16),
                Etiqueta("Provincia"), cboProvinciaVenta,
                Separador(16),
                Etiqueta("Localidad"), cboLocalidadVenta,
                Separador(16),
                chkCambiarDireccion
            });


            // --- Fila 2: producto / cantidad / acciones
            var fila2 = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 56, Padding = new Padding(12, 6, 12, 10), WrapContents = false };
            fila2.Controls.AddRange(new Control[]
            {
                Etiqueta("Producto"), cboProducto,
                Separador(16),
                Etiqueta("Cant."), numCant,
                Separador(16),
                Etiqueta("Canal"), cboCanal,
                Separador(16),
                btnAgregar, btnQuitar, btnGuardar
            });

            // --- Pie (totales a la derecha)
            var pie = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 54,
                Padding = new Padding(12, 8, 16, 10),
                FlowDirection = FlowDirection.RightToLeft
            };
            pie.Controls.Add(lblTotal);
            pie.Controls.Add(Separador(24));
            pie.Controls.Add(lblItems);

            Controls.Add(_gridDetalles);
            Controls.Add(pie);
            Controls.Add(fila2);
            Controls.Add(fila1_5);
            Controls.Add(fila1);

            // Estilo de DataGridView accesible
            GridHelper.Estilizar(_gridDetalles);

            // Columnas
            _gridDetalles.Columns.Clear();
            _gridDetalles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Producto",
                HeaderText = "Producto",
                DataPropertyName = "Producto",
                FillWeight = 48,
                ReadOnly = true
            });

            var colCant = new DataGridViewTextBoxColumn
            {
                Name = "Cant",
                HeaderText = "Cant",
                DataPropertyName = "Cantidad",
                FillWeight = 12,
                ReadOnly = true
            };
            colCant.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            var colPrecio = new DataGridViewTextBoxColumn
            {
                Name = "Precio",
                HeaderText = "Precio (ARS)",
                DataPropertyName = "Precio",
                FillWeight = 20,
                ReadOnly = true
            };
            colPrecio.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colPrecio.DefaultCellStyle.FormatProvider = _culture;
            colPrecio.DefaultCellStyle.Format = "C2";

            var colSubtotal = new DataGridViewTextBoxColumn
            {
                Name = "Subtotal",
                HeaderText = "Subtotal (ARS)",
                DataPropertyName = "Subtotal",
                FillWeight = 20,
                ReadOnly = true
            };
            colSubtotal.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colSubtotal.DefaultCellStyle.FormatProvider = _culture;
            colSubtotal.DefaultCellStyle.Format = "C2";

            _gridDetalles.Columns.AddRange(colCant, colPrecio, colSubtotal);
            _gridDetalles.DataSource = bs;

            // Datos
            _clientes = Repository.ListarClientes(false);
            cboCliente.ValueMember = nameof(Cliente.Id);
            cboCliente.DisplayMember = nameof(Cliente.Nombre);
            cboCliente.DataSource = _clientes;
            if (_clientes.Count > 0) cboCliente.SelectedIndex = 0;

            _productos = Repository.ListarProductos(false);
            cboProducto.ValueMember = nameof(Producto.Id);
            cboProducto.DisplayMember = nameof(Producto.Nombre);
            cboProducto.DataSource = _productos;
            if (_productos.Count > 0) cboProducto.SelectedIndex = 0;

            cboCanal.Items.AddRange(new[] { "WhatsApp", "Instagram", "Facebook", "MercadoLibre", "Local", "Otro" });
            cboCanal.SelectedIndex = 0;

            // --- Lógica de Dirección ---
            cboCliente.SelectedIndexChanged += (s, e) => OnClienteChanged();
            chkCambiarDireccion.CheckedChanged += (_, __) => ActualizarVisibilidadDireccion();

            // Disparar el primer cambio para cargar datos del cliente inicial
            OnClienteChanged();
            // Configurar los manejadores de eventos para los botones de acción
            SetupActionHandlers();
        }

        private void OnClienteChanged()
        {
            if (cboCliente.SelectedItem is not Cliente cli) return;

            txtDireccionEnvio.Text = cli.Direccion;
            _clienteLocalidadId = cli.LocalidadId;
            
            // Determinar la provincia del cliente a partir de su localidad
            if (_clienteLocalidadId.HasValue)
            {
                var loc = Repository.ObtenerLocalidad(_clienteLocalidadId.Value);
                _clienteProvinciaId = loc?.ProvinciaId;
            }
            else
            {
                _clienteProvinciaId = null;
            }

            chkCambiarDireccion.Checked = false; // Siempre resetear a la dirección del cliente
            ActualizarVisibilidadDireccion();
        }

        private void ActualizarVisibilidadDireccion()
        {
            bool cambiar = chkCambiarDireccion.Checked;
            cboProvinciaVenta.Visible = cambiar;
            cboLocalidadVenta.Visible = cambiar;
            cboProvinciaVenta.Enabled = cambiar;
            cboLocalidadVenta.Enabled = cambiar;
            txtDireccionEnvio.ReadOnly = !cambiar;

            // Desacoplar/Acoplar eventos para evitar bucles
            cboProvinciaVenta.SelectedValueChanged -= CboProvinciaVenta_SelectedValueChanged;
            cboLocalidadVenta.SelectedValueChanged -= CboLocalidadVenta_SelectedValueChanged;

            if (cambiar)
            {
                // Modo "Otra dirección": Cargar provincias activas y dejar que el usuario elija
                var provinciasActivas = Repository.ListarProvincias(false);
                FormUtils.BindCombo(cboProvinciaVenta, provinciasActivas);
                CboProvinciaVenta_SelectedValueChanged(null, EventArgs.Empty); // Cargar localidades de la primera provincia
                
                cboProvinciaVenta.SelectedValueChanged += CboProvinciaVenta_SelectedValueChanged;
                cboLocalidadVenta.SelectedValueChanged += CboLocalidadVenta_SelectedValueChanged;
            }
            else
            {
                // Modo "Dirección del cliente": Cargar y seleccionar los datos del cliente (incluso si están inactivos)
                var provincias = Repository.ListarProvincias(false);
                if (_clienteProvinciaId.HasValue && !provincias.Any(p => p.Id == _clienteProvinciaId))
                {
                    var provInactiva = Repository.ObtenerProvincia(_clienteProvinciaId.Value);
                    if (provInactiva != null) provincias.Add(provInactiva);
                }
                FormUtils.BindCombo(cboProvinciaVenta, provincias.OrderBy(p => p.Nombre).ToList(), selectedValue: _clienteProvinciaId);

                var localidades = Repository.ListarLocalidades(_clienteProvinciaId, false);
                if (_clienteLocalidadId.HasValue && !localidades.Any(l => l.Id == _clienteLocalidadId))
                {
                    var locInactiva = Repository.ObtenerLocalidad(_clienteLocalidadId.Value);
                    if (locInactiva != null) localidades.Add(locInactiva);
                }
                FormUtils.BindCombo(cboLocalidadVenta, localidades.OrderBy(l => l.Nombre).ToList(), selectedValue: _clienteLocalidadId);
            }
        }

        private void CboProvinciaVenta_SelectedValueChanged(object? sender, EventArgs e)
        {
            if (cboProvinciaVenta.SelectedValue is int provId)
            {
                var localidades = Repository.ListarLocalidades(provId, false);
                FormUtils.BindCombo(cboLocalidadVenta, localidades);
            }
            else
            {
                FormUtils.BindCombo<Localidad>(cboLocalidadVenta, null);
            }
        }

        private void CboLocalidadVenta_SelectedValueChanged(object? sender, EventArgs e)
        {
            // Este evento es principalmente para futuras validaciones si fueran necesarias.
            // La lógica de guardado ya lee el valor final.
        }

        private void SetupActionHandlers()
        {
            btnAgregar.Click += (s, e) =>
            {
                if (cboProducto.SelectedItem is not Producto p) return;
                int cant = (int)numCant.Value;
                if (cant <= 0) return;

                int yaEnCarrito = _ventaActual.Detalles.Where(d => d.ProductoId == p.Id).Sum(d => d.Cantidad);
                int disponible = p.Stock - yaEnCarrito;

                if (disponible <= 0 || cant > disponible)
                {
                    MessageBox.Show(
                        $"No hay stock suficiente de \"{p.Nombre}\".\nDisponibles: {Math.Max(disponible, 0)} • Pedidos: {cant}",
                        "Stock insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                var linea = _ventaActual.Detalles.FirstOrDefault(d => d.ProductoId == p.Id);
                if (linea == null)
                {
                    _ventaActual.Detalles.Add(new DetalleVenta
                    {
                        ProductoId = p.Id,
                        ProductoNombre = p.Nombre,
                        Cantidad = cant,
                        PrecioUnitario = p.Precio
                    });
                }
                else
                {
                    linea.Cantidad += cant;
                }

                if (_ventaActual.Detalles.Count == 1) cboCliente.Enabled = false;
                RefrescarGrid();
            };

            btnQuitar.Click += (s, e) =>
            {
                if (_gridDetalles.CurrentRow?.DataBoundItem is DetalleView det)
                {
                    var toRemove = _ventaActual.Detalles.FirstOrDefault(x => x.ProductoId == det.ProductoId);
                    if (toRemove != null)
                    {
                        _ventaActual.Detalles.Remove(toRemove);
                        RefrescarGrid();
                        if (_ventaActual.Detalles.Count == 0) cboCliente.Enabled = true;
                    }
                }
            };

            btnGuardar.Click += (s, e) => // Este es el manejador de click del botón Guardar
            {
                if (_ventaActual.Detalles.Count == 0)
                {
                    MessageBox.Show("Debe agregar al menos un producto a la venta.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (cboCliente.SelectedItem is not Cliente cli) { MessageBox.Show("Seleccioná un cliente.", "Aviso"); return; }

                foreach (var linea in _ventaActual.Detalles)
                {
                    var prod = _productos.FirstOrDefault(p => p.Id == linea.ProductoId);
                    if (prod == null) continue; // Producto no encontrado, debería ser raro
                    int pedida = _ventaActual.Detalles.Where(d => d.ProductoId == prod.Id).Sum(d => d.Cantidad);
                    if (pedida > prod.Stock)
                    {
                        MessageBox.Show($"No hay stock suficiente de \"{prod.Nombre}\".\nDisponibles: {prod.Stock} • Pedidos: {pedida}",
                            "Stock insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                _ventaActual.FechaVenta = DateTime.Now;
                _ventaActual.Vendedor = Sesion.Usuario;
                _ventaActual.Canal = (string)cboCanal.SelectedItem!;
                _ventaActual.ClienteId = cli.Id;
                _ventaActual.ClienteNombre = cli.Nombre;

                if (chkCambiarDireccion.Checked)
                {
                    if (string.IsNullOrWhiteSpace(txtDireccionEnvio.Text) || cboProvinciaVenta.SelectedValue == null || cboLocalidadVenta.SelectedValue == null)
                    {
                        MessageBox.Show("Si usa otra dirección, debe completar la Dirección, Provincia y Localidad de envío.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    _ventaActual.LocalidadId = (int)cboLocalidadVenta.SelectedValue;
                    _ventaActual.DireccionEnvio = txtDireccionEnvio.Text.Trim();
                }
                else
                {
                    _ventaActual.LocalidadId = _clienteLocalidadId;
                    _ventaActual.DireccionEnvio = txtDireccionEnvio.Text.Trim(); // Usar el texto por si lo editó sin marcar el check
                    if (string.IsNullOrWhiteSpace(_ventaActual.DireccionEnvio) || !_ventaActual.LocalidadId.HasValue)
                    {
                        MessageBox.Show("El cliente no tiene una dirección completa (dirección, localidad, provincia). Por favor, actualice los datos del cliente o seleccione 'Usar otra dirección de envío'.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                
                try
                {
                    var id = Repository.InsertarVenta(_ventaActual);
                    MessageBox.Show($"Venta guardada (Id {id}).", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                catch (SqlException ex) when (
                    ex.Number == 547 ||
                    (ex.Message.Contains("CHECK", StringComparison.OrdinalIgnoreCase) && ex.Message.Contains("Stock", StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("El stock cambió y ya no alcanza.\nActualizá o ajustá cantidades.",
                        "Stock insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ocurrió un error al guardar la venta.\n\n" + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
        }

        private void GuardarVenta()
        {
            // Este método ahora está vacío. La lógica se movió a SetupActionHandlers.
        }

        //  Datos en grilla + totales 
        private void RefrescarGrid()
        {
            var view = _ventaActual.Detalles.Select(d => new DetalleView
            {
                ProductoId = d.ProductoId,
                Producto = d.ProductoNombre,
                Cantidad = d.Cantidad,
                Precio = d.PrecioUnitario,
            }).ToList();

            bs.DataSource = view;
            bs.ResetBindings(false);

            lblTotal.Text = $"Total: {view.Sum(v => v.Subtotal).ToString("C2", _culture)}";
            lblItems.Text = $"Ítems: {view.Sum(v => v.Cantidad)}";
        }

        // DTO para la grilla
        public class DetalleView
        {
            public int ProductoId { get; set; }
            public string Producto { get; set; } = "";
            public int Cantidad { get; set; }
            public decimal Precio { get; set; }
            public decimal Subtotal => Math.Round(Precio * Cantidad, 2);
        }

        // ===== Helpers UI mínimos =====
        private static Label Etiqueta(string texto) =>
            new() { Text = texto, AutoSize = true, Padding = new Padding(0, 8, 0, 0), TextAlign = ContentAlignment.MiddleLeft };

        private static Control Separador(int width) => new Label { Width = width };
    }
}
