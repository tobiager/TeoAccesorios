using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using TeoAccesorios.Desktop.Models;
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

        private readonly Button btnAgregar = new() { Text = "Agregar", AutoSize = true };
        private readonly Button btnQuitar = new() { Text = "Quitar", AutoSize = true };
        private readonly Button btnGuardar = new() { Text = "Guardar", AutoSize = true };

        //  Grilla (solo lectura + legible) 
        private readonly DataGridView gridDetalles = new()
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
        private readonly List<DetalleVenta> carrito = new();
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
                Etiqueta("Cliente"), cboCliente,
                Separador(16),
                Etiqueta("Canal"), cboCanal,
                Separador(16),
                Etiqueta("Dirección envío"), txtDireccionEnvio
            });

            // --- Fila 2: producto / cantidad / acciones
            var fila2 = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 56, Padding = new Padding(12, 6, 12, 10), WrapContents = false };
            fila2.Controls.AddRange(new Control[]
            {
                Etiqueta("Producto"), cboProducto,
                Separador(16),
                Etiqueta("Cant."), numCant,
                Separador(18),
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

            Controls.Add(gridDetalles);
            Controls.Add(pie);
            Controls.Add(fila2);
            Controls.Add(fila1);

            // Estilo de DataGridView accesible
            GridHelper.Estilizar(gridDetalles);

            // Columnas
            gridDetalles.Columns.Clear();
            gridDetalles.Columns.Add(new DataGridViewTextBoxColumn
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
                DataPropertyName = "Cant",
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

            gridDetalles.Columns.AddRange(colCant, colPrecio, colSubtotal);
            gridDetalles.DataSource = bs;

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

            cboCliente.SelectedIndexChanged += (_, __) =>
            {
                if (cboCliente.SelectedItem is Cliente cli) txtDireccionEnvio.Text = cli.Direccion ?? "";
            };
            if (cboCliente.SelectedItem is Cliente cli0) txtDireccionEnvio.Text = cli0.Direccion ?? "";

            RefrescarGrid();

            //  Acciones 
            btnAgregar.Click += (s, e) =>
            {
                if (cboProducto.SelectedItem is not Producto p) return;
                int cant = (int)numCant.Value;
                if (cant <= 0) return;

                int yaEnCarrito = carrito.Where(d => d.ProductoId == p.Id).Sum(d => d.Cantidad);
                int disponible = p.Stock - yaEnCarrito;

                if (disponible <= 0 || cant > disponible)
                {
                    MessageBox.Show(
                        $"No hay stock suficiente de \"{p.Nombre}\".\nDisponibles: {Math.Max(disponible, 0)} • Pedidos: {cant}",
                        "Stock insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var linea = carrito.FirstOrDefault(d => d.ProductoId == p.Id);
                if (linea == null)
                {
                    carrito.Add(new DetalleVenta
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

                if (carrito.Count == 1) cboCliente.Enabled = false;
                RefrescarGrid();
            };

            btnQuitar.Click += (s, e) =>
            {
                if (gridDetalles.CurrentRow?.DataBoundItem is DetalleView det)
                {
                    var toRemove = carrito.FirstOrDefault(x => x.ProductoNombre == det.Producto && x.PrecioUnitario == det.Precio);
                    if (toRemove != null)
                    {
                        carrito.Remove(toRemove);
                        if (carrito.Count == 0) cboCliente.Enabled = true;
                        RefrescarGrid();
                    }
                }
            };

            btnGuardar.Click += (s, e) =>
            {
                if (carrito.Count == 0) { MessageBox.Show("Agregá al menos un producto.", "Aviso"); return; }
                if (cboCliente.SelectedItem is not Cliente cli) { MessageBox.Show("Seleccioná un cliente.", "Aviso"); return; }

                foreach (var linea in carrito)
                {
                    var prod = _productos.First(p => p.Id == linea.ProductoId);
                    int pedida = carrito.Where(d => d.ProductoId == prod.Id).Sum(d => d.Cantidad);
                    if (pedida > prod.Stock)
                    {
                        MessageBox.Show($"No hay stock suficiente de \"{prod.Nombre}\".\nDisponibles: {prod.Stock} • Pedidos: {pedida}",
                            "Stock insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                var venta = new Venta
                {
                    FechaVenta = DateTime.Now,
                    Vendedor = Sesion.Usuario,
                    Canal = (string)cboCanal.SelectedItem!,
                    ClienteId = cli.Id,
                    ClienteNombre = cli.Nombre,
                    DireccionEnvio = txtDireccionEnvio.Text?.Trim() ?? "",
                    Detalles = carrito.Select(d => new DetalleVenta
                    {
                        ProductoId = d.ProductoId,
                        ProductoNombre = d.ProductoNombre,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario
                    }).ToList()
                };

                try
                {
                    var id = Repository.InsertarVenta(venta);
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

        //  Datos en grilla + totales 
        private void RefrescarGrid()
        {
            var view = carrito.Select(d => new DetalleView
            {
                Producto = d.ProductoNombre,
                Cant = d.Cantidad,
                Precio = d.PrecioUnitario,
                Subtotal = d.Cantidad * d.PrecioUnitario
            }).ToList();

            bs.DataSource = view;

            lblTotal.Text = $"Total: {view.Sum(v => v.Subtotal).ToString("C2", _culture)}";
            lblItems.Text = $"Ítems: {view.Sum(v => v.Cant)}";
        }

        // DTO para la grilla
        private class DetalleView
        {
            public string Producto { get; set; } = "";
            public int Cant { get; set; }
            public decimal Precio { get; set; }
            public decimal Subtotal { get; set; }
            public decimal PrecioUnitario => Precio; 
        }

        // ===== Helpers UI mínimos =====
        private static Label Etiqueta(string texto) =>
            new() { Text = texto, AutoSize = true, Padding = new Padding(0, 10, 8, 0) };

        private static Control Separador(int width) => new Label { Width = width };
    }
}
