using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class NuevaVentaForm : Form
    {
        private readonly ComboBox cboCliente = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
        private readonly ComboBox cboProducto = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };
        private readonly NumericUpDown numCant = new NumericUpDown { Minimum = 1, Maximum = 1000, Value = 1, Width = 80 };

        private readonly ComboBox cboCanal = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
        private readonly TextBox txtDireccionEnvio = new TextBox { Width = 320, PlaceholderText = "Dirección de envío" };

        private readonly Button btnAgregar = new Button { Text = "Agregar" };
        private readonly Button btnQuitar = new Button { Text = "Quitar" };
        private readonly Button btnGuardar = new Button { Text = "Guardar" };

        private readonly DataGridView gridDetalles = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        private readonly BindingSource bs = new BindingSource();
        private readonly List<DetalleVenta> carrito = new List<DetalleVenta>();

        public NuevaVentaForm()
        {
            Text = "Nueva Venta";
            Width = 1100;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;

            var fila1 = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 38, Padding = new Padding(8), AutoSize = false };
            fila1.Controls.Add(new Label { Text = "Cliente", AutoSize = true, Padding = new Padding(0, 8, 6, 0) });
            fila1.Controls.Add(cboCliente);
            fila1.Controls.Add(new Label { Text = "Canal", AutoSize = true, Padding = new Padding(12, 8, 6, 0) });
            fila1.Controls.Add(cboCanal);
            fila1.Controls.Add(new Label { Text = "Dirección envío", AutoSize = true, Padding = new Padding(12, 8, 6, 0) });
            fila1.Controls.Add(txtDireccionEnvio);

            var fila2 = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 38, Padding = new Padding(8), AutoSize = false };
            fila2.Controls.Add(new Label { Text = "Producto", AutoSize = true, Padding = new Padding(0, 8, 6, 0) });
            fila2.Controls.Add(cboProducto);
            fila2.Controls.Add(new Label { Text = "Cant.", AutoSize = true, Padding = new Padding(12, 8, 6, 0) });
            fila2.Controls.Add(numCant);
            fila2.Controls.Add(btnAgregar);
            fila2.Controls.Add(btnQuitar);
            fila2.Controls.Add(btnGuardar);

            Controls.Add(gridDetalles);
            Controls.Add(fila2);
            Controls.Add(fila1);

            GridHelper.Estilizar(gridDetalles);

            // Configurar columnas
            gridDetalles.Columns.Clear();
            gridDetalles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Producto",
                HeaderText = "Producto",
                DataPropertyName = "Producto"
            });

            var colCant = new DataGridViewTextBoxColumn
            {
                Name = "Cant",
                HeaderText = "Cant",
                DataPropertyName = "Cant",
                FillWeight = 15
            };
            colCant.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            var colPrecio = new DataGridViewTextBoxColumn
            {
                Name = "Precio",
                HeaderText = "Precio",
                DataPropertyName = "Precio",
                FillWeight = 20
            };
            colPrecio.DefaultCellStyle.Format = "N2";
            colPrecio.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            var colSubtotal = new DataGridViewTextBoxColumn
            {
                Name = "Subtotal",
                HeaderText = "Subtotal",
                DataPropertyName = "Subtotal",
                FillWeight = 20
            };
            colSubtotal.DefaultCellStyle.Format = "N2";
            colSubtotal.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            gridDetalles.Columns.Add(colCant);
            gridDetalles.Columns.Add(colPrecio);
            gridDetalles.Columns.Add(colSubtotal);

            gridDetalles.DataSource = bs;

            // Combos
            var clientes = Repository.ListarClientes(false);
            cboCliente.ValueMember = nameof(Cliente.Id);
            cboCliente.DisplayMember = nameof(Cliente.Nombre);
            cboCliente.DataSource = clientes;
            if (clientes.Count > 0) cboCliente.SelectedIndex = 0;

            var productos = Repository.ListarProductos(false);
            cboProducto.ValueMember = nameof(Producto.Id);
            cboProducto.DisplayMember = nameof(Producto.Nombre);
            cboProducto.DataSource = productos;
            if (productos.Count > 0) cboProducto.SelectedIndex = 0;

            // Canal
            cboCanal.Items.AddRange(new[] { "WhatsApp", "Instagram", "Facebook", "MercadoLibre", "Local", "Otro" });
            cboCanal.SelectedIndex = 0;

            // Prefill dirección según cliente
            cboCliente.SelectedIndexChanged += (_, __) =>
            {
                if (cboCliente.SelectedItem is Cliente cli)
                    txtDireccionEnvio.Text = cli.Direccion ?? "";
            };
            if (cboCliente.SelectedItem is Cliente cli0) txtDireccionEnvio.Text = cli0.Direccion ?? "";

            RefrescarGrid();

            // Botones
            btnAgregar.Click += (s, e) =>
            {
                if (cboProducto.SelectedItem is not Producto p) return;

                var cant = (int)numCant.Value;
                if (cant <= 0) return;

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
                if (carrito.Count == 0)
                {
                    MessageBox.Show("Agregá al menos un producto.", "Aviso");
                    return;
                }
                if (cboCliente.SelectedItem is not Cliente cli)
                {
                    MessageBox.Show("Seleccioná un cliente.", "Aviso");
                    return;
                }

                var venta = new Venta
                {
                    FechaVenta = DateTime.Now,
                    Vendedor = Sesion.Usuario,
                    Canal = (string)cboCanal.SelectedItem,
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
                    MessageBox.Show("Venta guardada (Id " + id + ").", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No se pudo guardar la venta.\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
        }

        private void RefrescarGrid()
        {
            var view = carrito.Select(d => new DetalleView
            {
                Producto = d.ProductoNombre,
                Cant = d.Cantidad,
                Precio = d.PrecioUnitario,
                Subtotal = d.Subtotal
            }).ToList();

            bs.DataSource = view;
        }

        private class DetalleView
        {
            public string Producto { get; set; }
            public int Cant { get; set; }
            public decimal Precio { get; set; }
            public decimal Subtotal { get; set; }
        }
    }
}
