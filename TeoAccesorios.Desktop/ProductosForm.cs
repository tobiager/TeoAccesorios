using System;
using System.Linq;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class ProductosForm : Form
    {
        private DataGridView grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
        private TextBox txtBuscar = new TextBox { PlaceholderText = "Buscar por nombre...", Width = 220 };
        private ComboBox cboCategoria = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200 };
        private CheckBox chkInactivos = new CheckBox { Text = "Ver inactivos" };
        private BindingSource bs = new BindingSource();

        public ProductosForm()
        {
            Text = "Productos";
            Width = 900;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(8), AutoSize = false };
            top.Controls.Add(txtBuscar);
            top.Controls.Add(cboCategoria);
            top.Controls.Add(chkInactivos);
            var btnFiltrar = new Button { Text = "Filtrar" };
            top.Controls.Add(btnFiltrar);

            // ABM solo para Admin
            if (Sesion.Rol == RolUsuario.Admin)
            {
                var btnNuevo = new Button { Text = "Nuevo" };
                var btnEditar = new Button { Text = "Editar" };
                var btnEliminar = new Button { Text = "Eliminar" };
                var btnRestaurar = new Button { Text = "Restaurar" };

                top.Controls.Add(btnNuevo);
                top.Controls.Add(btnEditar);
                top.Controls.Add(btnEliminar);
                top.Controls.Add(btnRestaurar);

                btnNuevo.Click += (s, e) =>
                {
                    var pr = new Producto();
                    using (var f = new ProductoEditForm(pr))
                    {
                        if (f.ShowDialog(this) == DialogResult.OK)
                        {
                            pr.Id = Repository.InsertarProducto(pr);
                            LoadData();
                        }
                    }
                };

                btnEditar.Click += (s, e) =>
                {
                    if (grid.CurrentRow != null && grid.CurrentRow.DataBoundItem is Producto sel)
                    {
                        var tmp = new Producto
                        {
                            Id = sel.Id,
                            Nombre = sel.Nombre,
                            Descripcion = sel.Descripcion,
                            Precio = sel.Precio,
                            Stock = sel.Stock,
                            StockMinimo = sel.StockMinimo,
                            CategoriaId = sel.CategoriaId
                        };

                        using (var f = new ProductoEditForm(tmp))
                        {
                            if (f.ShowDialog(this) == DialogResult.OK)
                            {
                                sel.Nombre = tmp.Nombre;
                                sel.Descripcion = tmp.Descripcion;
                                sel.Precio = tmp.Precio;
                                sel.Stock = tmp.Stock;
                                sel.StockMinimo = tmp.StockMinimo;
                                sel.CategoriaId = tmp.CategoriaId;
                                LoadData();
                            }
                        }
                    }
                };

                btnEliminar.Click += (s, e) =>
                {
                    if (grid.CurrentRow != null && grid.CurrentRow.DataBoundItem is Producto sel)
                    {
                        sel.Activo = false;
                        LoadData();
                    }
                };

                btnRestaurar.Click += (s, e) =>
                {
                    if (grid.CurrentRow != null && grid.CurrentRow.DataBoundItem is Producto sel)
                    {
                        sel.Activo = true;
                        LoadData();
                    }
                };
            }
            else
            {
                grid.ReadOnly = true; // vendedor: solo ver
            }

            // Filtros
            btnFiltrar.Click += (s, e) => LoadData();
            chkInactivos.CheckedChanged += (s, e) => LoadData();
            txtBuscar.TextChanged += (s, e) => LoadData();
            cboCategoria.SelectedIndexChanged += (s, e) => LoadData();

            // Categorías
            cboCategoria.Items.Clear();
            cboCategoria.Items.Add("Todas las categorías");
            foreach (var c in Repository.ListarCategorias()) cboCategoria.Items.Add(string.Format("{0} - {1}", c.Id, c.Nombre));
            cboCategoria.SelectedIndex = 0;

            Controls.Add(grid);
            Controls.Add(top);
           
            GridHelper.Estilizar(grid);
            LoadData();
        }

        private void LoadData()
        {
            var data = Repository.ListarProductos(chkInactivos.Checked).AsEnumerable();

            var q = txtBuscar.Text != null ? txtBuscar.Text.Trim() : string.Empty;
            if (!string.IsNullOrWhiteSpace(q))
                data = data.Where(p => p.Nombre != null && p.Nombre.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0);

            if (cboCategoria.SelectedIndex > 0)
            {
                var sel = cboCategoria.SelectedItem.ToString();
                var id = int.Parse(sel.Split('-')[0].Trim());
                data = data.Where(p => p.CategoriaId == id);
            }

            /* activo ya filtrado arriba */

            bs.DataSource = data.ToList();
            grid.DataSource = bs;
        }
    }
}
