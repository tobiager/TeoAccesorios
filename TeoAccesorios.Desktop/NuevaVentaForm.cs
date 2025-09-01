using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class NuevaVentaForm : Form
    {
        private ComboBox cboCliente = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };
        private ComboBox cboProducto = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };
        private NumericUpDown numCant = new NumericUpDown { Minimum = 1, Maximum = 100, Value = 1, Width = 80 };
        private Button btnAgregar = new Button { Text = "Agregar" };
        private Button btnQuitar = new Button { Text = "Quitar" };
        private Button btnGuardar = new Button { Text = "Guardar" };
        private DataGridView gridDetalles = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
        private BindingSource bs = new BindingSource();

        private List<DetalleVenta> carrito = new List<DetalleVenta>();

        public NuevaVentaForm()
        {
            Text = "Nueva Venta";
            Width = 820;
            Height = 520;
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

            // Cargar combos
            foreach (var c in MockData.Clientes.Where(x => x.Activo))
                cboCliente.Items.Add(string.Format("{0} - {1}", c.Id, c.Nombre));
            if (cboCliente.Items.Count > 0) cboCliente.SelectedIndex = 0;

            foreach (var p in MockData.Productos.Where(x => x.Activo))
                cboProducto.Items.Add(string.Format("{0} - {1}", p.Id, p.Nombre));
            if (cboProducto.Items.Count > 0) cboProducto.SelectedIndex = 0;

            gridDetalles.DataSource = bs;
            RefrescarGrid();

            // Handlers
            btnAgregar.Click += (s, e) =>
            {
                if (cboProducto.SelectedIndex < 0) return;

                var sel = cboProducto.SelectedItem as string;
                if (string.IsNullOrWhiteSpace(sel)) return;
                if (!int.TryParse(sel.Split('-')[0].Trim(), out var idProd)) return;

                var p = MockData.Productos.First(x => x.Id == idProd);

                carrito.Add(new DetalleVenta
                {
                    ProductoId = p.Id,
                    ProductoNombre = p.Nombre,
                    Cantidad = (int)numCant.Value,
                    PrecioUnitario = p.Precio
                });

                if (carrito.Count == 1) cboCliente.Enabled = false; // bloquear cliente al primer ítem
                RefrescarGrid();
            };

            btnQuitar.Click += (s, e) =>
            {
                if (gridDetalles.CurrentRow?.DataBoundItem is DetalleVenta det)
                {
                    carrito.Remove(det);
                    if (carrito.Count == 0) cboCliente.Enabled = true; // liberar si no hay ítems
                    RefrescarGrid();
                }
            };

            btnGuardar.Click += (s, e) =>
            {
                if (carrito.Count == 0)
                {
                    MessageBox.Show("Agregá al menos un producto.", "Aviso");
                    return;
                }
                if (cboCliente.SelectedIndex < 0)
                {
                    MessageBox.Show("Seleccioná un cliente.", "Aviso");
                    return;
                }

                var selCli = cboCliente.SelectedItem as string;
                if (string.IsNullOrWhiteSpace(selCli)) { MessageBox.Show("Seleccioná un cliente.", "Aviso"); return; }
                if (!int.TryParse(selCli.Split('-')[0].Trim(), out var idCli)) { MessageBox.Show("Cliente inválido.", "Aviso"); return; }

                var cli = MockData.Clientes.First(x => x.Id == idCli);

                var venta = new Venta
                {
                    Id = (MockData.Ventas.LastOrDefault() != null ? MockData.Ventas.Last().Id : 2000) + 1,
                    Fecha = DateTime.Now,
                    Vendedor = Sesion.Usuario,
                    Canal = "Instagram",
                    ClienteId = cli.Id,
                    ClienteNombre = cli.Nombre,
                    DireccionEnvio = cli.Direccion,
                    Detalles = carrito.ToList()
                };

                MockData.Ventas.Add(venta);
                DialogResult = DialogResult.OK;
                Close();
            };

            // Previene cambio de cliente con ítems
            cboCliente.SelectedIndexChanged += (s, e) =>
            {
                if (carrito.Count > 0)
                {
                    MessageBox.Show("No se puede cambiar el cliente después de agregar productos. Quite los ítems para cambiarlo.", "Aviso");
                    cboCliente.Enabled = false;
                }
            };
        }

        private void RefrescarGrid()
        {
            bs.DataSource = carrito.Select(d => new
            {
                Producto = d.ProductoNombre,
                Cant = d.Cantidad,
                Precio = d.PrecioUnitario,
                Subtotal = d.Subtotal
            }).ToList();
        }
    }
}
