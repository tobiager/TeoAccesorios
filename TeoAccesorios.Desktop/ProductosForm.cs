using System;
using System.Linq;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class ProductosForm : Form
    {
        private readonly DataGridView grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
        private readonly TextBox txtBuscar = new TextBox { PlaceholderText = "Buscar por nombre...", Width = 220 };
        private readonly ComboBox cboCategoria = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200 };
        private readonly ComboBox cboSubcategoria = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200 };
        private readonly CheckBox chkInactivos = new CheckBox { Text = "Ver inactivos" };
        private readonly BindingSource bs = new BindingSource();

        public ProductosForm()
        {
            Text = "Productos";
            Width = 1200;
            Height = 720;
            StartPosition = FormStartPosition.CenterParent;

            // Barra superior (filtros + ABM)
            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(8), AutoSize = false };
            top.Controls.Add(txtBuscar);
            top.Controls.Add(new Label { Text = "Categoría", AutoSize = true, Padding = new Padding(8, 8, 4, 0) });
            top.Controls.Add(cboCategoria);
            top.Controls.Add(new Label { Text = "Subcategoría", AutoSize = true, Padding = new Padding(8, 8, 4, 0) });
            top.Controls.Add(cboSubcategoria);
            top.Controls.Add(chkInactivos);
            var btnFiltrar = new Button { Text = "Filtrar" };
            top.Controls.Add(btnFiltrar);

            // ABM solo Admin
            if (Sesion.Rol == RolUsuario.Admin)
            {
                var btnNuevo = new Button { Text = "Nuevo" };
                var btnEditar = new Button { Text = "Editar" };
                var btnEliminar = new Button { Text = "Eliminar" };
                var btnRestaurar = new Button { Text = "Restaurar" };
                top.Controls.AddRange(new Control[] { btnNuevo, btnEditar, btnEliminar, btnRestaurar });

                btnNuevo.Click += (s, e) =>
                {
                    var pr = new Producto();
                    using var f = new ProductoEditForm(pr);
                    if (f.ShowDialog(this) == DialogResult.OK)
                    {
                        pr.Id = Repository.InsertarProducto(pr);
                        LoadData();
                    }
                };

                btnEditar.Click += (s, e) =>
                {
                    if (grid.CurrentRow?.DataBoundItem is Producto sel)
                    {
                        var tmp = new Producto
                        {
                            Id = sel.Id,
                            Nombre = sel.Nombre,
                            Descripcion = sel.Descripcion,
                            Precio = sel.Precio,
                            Stock = sel.Stock,
                            StockMinimo = sel.StockMinimo,
                            CategoriaId = sel.CategoriaId,
                            SubcategoriaId = sel.SubcategoriaId,
                            Activo = sel.Activo
                        };
                        using var f = new ProductoEditForm(tmp);
                        if (f.ShowDialog(this) == DialogResult.OK)
                        {
                            Repository.ActualizarProducto(tmp);
                            LoadData();
                        }
                    }
                };

                btnEliminar.Click += (s, e) =>
                {
                    if (grid.CurrentRow?.DataBoundItem is Producto sel)
                    {
                        Repository.EliminarProducto(sel.Id);
                        LoadData();
                    }
                };

                btnRestaurar.Click += (s, e) =>
                {
                    if (grid.CurrentRow?.DataBoundItem is Producto sel)
                    {
                        sel.Activo = true;
                        Repository.ActualizarProducto(sel);
                        LoadData();
                    }
                };
            }
            else
            {
                grid.ReadOnly = true; // vendedor: solo ver
            }

            // Layout principal: solo la grilla
            Controls.Add(grid);
            Controls.Add(top);

            // Estilo grilla
            GridHelper.Estilizar(grid);
            GridHelperLock.SoloLectura(grid);
            GridHelperLock.WireDataBindingLock(grid);

            // Filtros
            btnFiltrar.Click += (s, e) => LoadData();
            chkInactivos.CheckedChanged += (s, e) => LoadData();
            txtBuscar.TextChanged += (s, e) => LoadData();
            cboCategoria.SelectedIndexChanged += (s, e) => { CargarSubcategorias(); LoadData(); };
            cboSubcategoria.SelectedIndexChanged += (s, e) => LoadData();

            // Cargar combos
            cboCategoria.Items.Clear();
            cboCategoria.Items.Add(new ComboItem { Text = "Todas las categorías", Value = null });
            foreach (var c in Repository.ListarCategorias())
                cboCategoria.Items.Add(new ComboItem { Text = $"{c.Id} - {c.Nombre}", Value = c.Id });
            cboCategoria.SelectedIndex = 0;

            CargarSubcategorias();
            LoadData();
        }

        private void CargarSubcategorias()
        {
            cboSubcategoria.Items.Clear();
            cboSubcategoria.Items.Add(new ComboItem { Text = "Todas las subcategorías", Value = null });

            int? catId = (cboCategoria.SelectedItem as ComboItem)?.Value;
            var subs = Repository.ListarSubcategorias(catId);
            foreach (var s in subs)
                cboSubcategoria.Items.Add(new ComboItem { Text = $"{s.Id} - {s.Nombre}", Value = s.Id });

            cboSubcategoria.SelectedIndex = 0;
        }

        private void LoadData()
        {
            var data = Repository.ListarProductos(chkInactivos.Checked).AsEnumerable();

            var q = txtBuscar.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(q))
                data = data.Where(p => (p.Nombre ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0);

            int? catId = (cboCategoria.SelectedItem as ComboItem)?.Value;
            if (catId != null) data = data.Where(p => p.CategoriaId == catId.Value);

            int? subId = (cboSubcategoria.SelectedItem as ComboItem)?.Value;
            if (subId != null) data = data.Where(p => p.SubcategoriaId == subId.Value);

            bs.DataSource = data.ToList();
            grid.DataSource = bs;
        }

        private class ComboItem
        {
            public string Text { get; set; } = "";
            public int? Value { get; set; }
            public override string ToString() => Text;
        }
    }
}
