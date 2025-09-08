using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class NuevaVentaForm : Form
    {
        private readonly ComboBox cboCliente = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };
        private readonly ComboBox cboProducto = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };
        private readonly NumericUpDown numCant = new NumericUpDown { Minimum = 1, Maximum = 1000, Value = 1, Width = 80 };
        private readonly Button btnAgregar = new Button { Text = "Agregar" };
        private readonly Button btnQuitar = new Button { Text = "Quitar" };
        private readonly Button btnGuardar = new Button { Text = "Guardar" };
        private readonly DataGridView gridDetalles = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false };
        private readonly BindingSource bs = new BindingSource();

        private readonly List<DetalleVenta> carrito = new List<DetalleVenta>();

        public NuevaVentaForm()
        {
            Text = "Nueva Venta";
            Width = 900;
            Height = 560;
            StartPosition = FormStartPosition.CenterParent;

            var fila1 = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 38, Padding = new Padding(8), AutoSize = false };
            fila1.Controls.Add(new Label { Text = "Cliente", AutoSize = true, Padding = new Padding(0, 8, 6, 0) });
            fila1.Controls.Add(cboCliente);
            fila1.Controls.Add(new Label { Text = "Producto", AutoSize = true, Padding = new Padding(12, 8, 6, 0) });
            fila1.Controls.Add(cboProducto);
            fila1.Controls.Add(new Label { Text = "Cant.", AutoSize = true, Padding = new Padding(12, 8, 6, 0) });
            fila1.Controls.Add(numCant);
            fila1.Controls.Add(btnAgregar);
            fila1.Controls.Add(btnQuitar);
            fila1.Controls.Add(btnGuardar);

            Controls.Add(gridDetalles);
            Controls.Add(fila1);

            // Estilo común de grillas
            GridHelper.Estilizar(gridDetalles);

            // Columnas de la grilla
            gridDetalles.Columns.Clear();
            gridDetalles.Columns.Add(new DataGridViewTextBoxColumn { Name = "Producto", HeaderText = "Producto", DataPropertyName = "Producto" });
            gridDetalles.Columns.Add(new DataGridViewTextBoxColumn { Name = "Cant", HeaderText = "Cant", DataPropertyName = "Cant", FillWeight = 15 });
            gridDetalles.Columns.Add(new DataGridViewTextBoxColumn { Name = "Precio", HeaderText = "Precio", DataPropertyName = "Precio", FillWeight = 20, DefaultCellStyle = { Format = "N2" } });
            gridDetalles.Columns.Add(new DataGridViewTextBoxColumn { Name = "Subtotal", HeaderText = "Subtotal", DataPropertyName = "Subtotal", FillWeight = 20, DefaultCellStyle = { Format = "N2" } });

            gridDetalles.DataSource = bs;

            // Cargar combos (tipados)
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

            RefrescarGrid();

            // Handlers
            btnAgregar.Click += (s, e) =>
            {
                if (cboProducto.SelectedItem is not Producto p) return;

                var cant = (int)numCant.Value;
                if (cant <= 0) return;

                // Si ya está en el carrito, sumo cantidades
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

                if (carrito.Count == 1) cboCliente.Enabled = false; // bloquear cliente al primer ítem
                RefrescarGrid();
            };

            btnQuitar.Click += (s, e) =>
            {
                // Quita la línea seleccionada
                if (gridDetalles.CurrentRow?.DataBoundItem is DetalleView det)
                {
                    var toRemove = carrito.FirstOrDefault(x => x.ProductoNombre == det.Producto && x.PrecioUnitario == det.Precio);
                    if (toRemove != null)
                    {
                        carrito.Remove(toRemove);
                        if (carrito.Count == 0) cboCliente.Enabled = true; // liberar si no hay ítems
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
                    Canal = "Instagram",
                    ClienteId = cli.Id,
                    ClienteNombre = cli.Nombre,
                    DireccionEnvio = cli.Direccion,
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
                    var id = Repository.InsertarVenta(venta); // descuenta stock y devuelve Id
                    MessageBox.Show("Venta guardada (Id " + id + ").", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No se pudo guardar la venta.\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // Previene cambio de cliente con ítems
            cboCliente.SelectedIndexChanged += (s, e) =>
            {
                if (carrito.Count > 0)
                {
                    cboCliente.Enabled = false;
                    MessageBox.Show("Para cambiar el cliente, quite los ítems primero.", "Aviso");
                }
            };
        }

        private void RefrescarGrid()
        {
            // Proyección para mostrar
            var view = carrito.Select(d => new DetalleView
            {
                Producto = d.ProductoNombre,
                Cant = d.Cantidad,
                Precio = d.PrecioUnitario,
                Subtotal = d.Subtotal
            }).ToList();

            bs.DataSource = view;
        }

        // DTO solo para la grilla
        private class DetalleView
        {
            public string Producto { get; set; }
            public int Cant { get; set; }
            public decimal Precio { get; set; }
            public decimal Subtotal { get; set; }
        }
    }
}
