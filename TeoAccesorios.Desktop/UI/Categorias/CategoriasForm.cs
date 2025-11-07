using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class CategoriasForm : Form
    {
        private enum Modo { Categorias, Subcategorias }
        private Modo _modo = Modo.Categorias;

        private readonly DataGridView grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
        private readonly BindingSource bs = new BindingSource();

        private readonly Button btnNuevo;
        private readonly Button btnEditar;
        private readonly Button btnEliminar;
        private readonly Button btnSwitch;
        private readonly Button btnVerInactivas;

        private readonly Label lblCat = new Label { Text = "Categoría:", AutoSize = true, Padding = new Padding(0, 10, 6, 0), Visible = false };
        private readonly ComboBox cboFiltroCat = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220, Visible = false };

        private readonly bool tienePermisos;

        public CategoriasForm()
        {
            // El Gerente también tiene todos los permisos en categorías
            tienePermisos = Sesion.Rol == RolUsuario.Admin || Sesion.Rol == RolUsuario.Gerente;

            Text = "Categorías";
            Width = 900;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;

            btnNuevo = CrearBoton("Nuevo");
            btnEditar = CrearBoton("Editar");
            btnEliminar = CrearBoton("Eliminar");
            btnSwitch = CrearBoton("Subcategorías");
            btnVerInactivas = CrearBoton("Ver inactivas");

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(6), WrapContents = false };
            // Orden: acciones, switch, ver inactivas, y recién después el filtro
            top.Controls.Add(btnNuevo);
            top.Controls.Add(btnEditar);
            top.Controls.Add(btnEliminar);
            top.Controls.Add(btnSwitch);
            top.Controls.Add(btnVerInactivas);
            lblCat.Margin = new Padding(12, 8, 6, 0);
            cboFiltroCat.Margin = new Padding(0, 6, 0, 0);
            top.Controls.Add(lblCat);
            top.Controls.Add(cboFiltroCat);

            Controls.Add(grid);
            Controls.Add(top);

            GridHelper.Estilizar(grid);
            GridHelperLock.Apply(grid);
            grid.DataSource = bs;

            grid.DataBindingComplete += (s, e) =>
            {
                var colActivo = grid.Columns["Activo"];
                if (colActivo != null) colActivo.Visible = false;
            };

            btnSwitch.Click += (s, e) => SwitchMode();

            btnNuevo.Click += (s, e) => { if (tienePermisos) OnNuevo(); };
            btnEditar.Click += (s, e) => { if (tienePermisos) OnEditar(); };
            btnEliminar.Click += (s, e) => { if (tienePermisos) OnEliminar(); };

            btnVerInactivas.Click += (s, e) =>
            {
                if (_modo == Modo.Categorias)
                {
                    using var f = new CategoriasInactivasForm();
                    f.ShowDialog(this);
                }
                else
                {
                    // Si SubcategoriasInactivasForm acepta filtro pasalo
                    int? catId = (cboFiltroCat.SelectedItem as ComboItem)?.Value;
                    using var f = new SubcategoriasInactivasForm(/*catId*/);
                    f.ShowDialog(this);
                }
                LoadData();
            };

            cboFiltroCat.SelectedIndexChanged += (s, e) => { if (_modo == Modo.Subcategorias) LoadData(); };

            //  Permisos de vendedor: fuerza modo Subcategorías y oculta acciones 
            if (!tienePermisos)
            {
                _modo = Modo.Subcategorias;
                btnSwitch.Visible = false;

                btnNuevo.Visible = btnEditar.Visible = btnEliminar.Visible = btnVerInactivas.Visible = false;
            }

            SetMode(_modo);
        }

        private Button CrearBoton(string texto) => new Button
        {
            Text = texto,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(3, 1, 3, 1),
            Margin = new Padding(2),
            MinimumSize = new Size(0, 24)
        };

        private void SwitchMode()
        {
            if (!tienePermisos) return;
            var nuevo = _modo == Modo.Categorias ? Modo.Subcategorias : Modo.Categorias;
            SetMode(nuevo);
        }

        private void SetMode(Modo m)
        {
            if (!tienePermisos && m == Modo.Categorias)
                m = Modo.Subcategorias;

            _modo = m;

            if (_modo == Modo.Categorias)
            {
                Text = "Categorías";
                btnSwitch.Text = "Subcategorías";
                lblCat.Visible = false;
                cboFiltroCat.Visible = false;
                btnVerInactivas.Visible = true;   
            }
            else
            {
                Text = "Subcategorías";
                btnSwitch.Text = "Categorías";
                lblCat.Visible = true;
                cboFiltroCat.Visible = true;
                btnVerInactivas.Visible = true;   

                if (cboFiltroCat.Items.Count == 0)
                {
                    cboFiltroCat.Items.Clear();
                    cboFiltroCat.Items.Add(new ComboItem { Text = "Todas", Value = null });
                    foreach (var c in Repository.ListarCategorias(true).OrderBy(x => x.Nombre))
                        cboFiltroCat.Items.Add(new ComboItem { Text = $"{c.Id} - {c.Nombre}", Value = c.Id });
                    cboFiltroCat.SelectedIndex = 0;
                }
            }

            LoadData();
        }

        private void LoadData()
        {
            if (_modo == Modo.Categorias)
            {
                var data = Repository.ListarCategorias(false)
                                     .OrderBy(c => c.Nombre)
                                     .ToList();
                bs.DataSource = data;
                grid.DataSource = bs;
            }
            else
            {
                int? catId = (cboFiltroCat.SelectedItem as ComboItem)?.Value;
                var data = Repository.ListarSubcategorias(catId, false)
                                     .OrderBy(s => s.CategoriaNombre)
                                     .ThenBy(s => s.Nombre)
                                     .ToList();
                bs.DataSource = data;
                grid.DataSource = bs;
            }
        }

        private void OnNuevo()
        {
            if (_modo == Modo.Categorias)
            {
                var cat = new Categoria { Activo = true };
                using var f = new CategoriaEditForm(cat);
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    // Confirmación antes de crear
                    var resp = MessageBox.Show(
                        $"¿Confirmás crear la categoría \"{cat.Nombre}\"?",
                        "Confirmar creación",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (resp == DialogResult.Yes)
                    {
                        cat.Id = Repository.InsertarCategoria(cat);
                        LoadData();
                    }
                }
            }
            else
            {
                int? catId = (cboFiltroCat.SelectedItem as ComboItem)?.Value;
                var primeraCat = Repository.ListarCategorias(true).OrderBy(x => x.Nombre).FirstOrDefault();
                if (!catId.HasValue && primeraCat is null)
                {
                    MessageBox.Show("No hay categorías disponibles. Creá una categoría primero.", "Atención",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int catParaNueva = catId ?? primeraCat!.Id;
                var sub = new Subcategoria { Activo = true, CategoriaId = catParaNueva };
                using var f = new SubcategoriaEditForm(sub);
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    // Obtener nombre de categoría para el mensaje (si está disponible)
                    var catNombre = Repository.ListarCategorias(true).FirstOrDefault(c => c.Id == sub.CategoriaId)?.Nombre ?? "";

                    var resp = MessageBox.Show(
                        $"¿Confirmás crear la subcategoría \"{sub.Nombre}\" en la categoría \"{catNombre}\"?",
                        "Confirmar creación",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (resp == DialogResult.Yes)
                    {
                        sub.Id = Repository.InsertarSubcategoria(sub);
                        LoadData();
                    }
                }
            }
        }

        private void OnEditar()
        {
            if (_modo == Modo.Categorias)
            {
                if (grid.CurrentRow?.DataBoundItem is Categoria sel)
                {
                    var tmp = new Categoria
                    {
                        Id = sel.Id,
                        Nombre = sel.Nombre,
                        Descripcion = sel.Descripcion,
                        Activo = sel.Activo
                    };
                    using var f = new CategoriaEditForm(tmp);
                    if (f.ShowDialog(this) == DialogResult.OK)
                    {
                        var resp = MessageBox.Show(
                            $"¿Confirmás modificar la categoría \"{sel.Nombre}\"?\n\nSe guardarán los cambios.",
                            "Confirmar modificación",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                        if (resp == DialogResult.Yes)
                        {
                            Repository.ActualizarCategoria(tmp);
                            LoadData();
                        }
                    }
                }
            }
            else
            {
                if (grid.CurrentRow?.DataBoundItem is Subcategoria sel)
                {
                    var tmp = new Subcategoria
                    {
                        Id = sel.Id,
                        Nombre = sel.Nombre,
                        Descripcion = sel.Descripcion,
                        CategoriaId = sel.CategoriaId,
                        Activo = sel.Activo
                    };
                    using var f = new SubcategoriaEditForm(tmp);
                    if (f.ShowDialog(this) == DialogResult.OK)
                    {
                        var resp = MessageBox.Show(
                            $"¿Confirmás modificar la subcategoría \"{sel.Nombre}\"?",
                            "Confirmar modificación",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                        if (resp == DialogResult.Yes)
                        {
                            Repository.ActualizarSubcategoria(tmp);
                            LoadData();
                        }
                    }
                }
            }
        }

        private void OnEliminar()
        {
            if (_modo == Modo.Categorias)
            {
                if (grid.CurrentRow?.DataBoundItem is Categoria sel && sel.Activo)
                {
                    // Confirmación antes de intentar desactivar
                    var resp = MessageBox.Show(
                        $"¿Confirmás desactivar la categoría \"{sel.Nombre}\"?",
                        "Confirmar desactivación",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (resp != DialogResult.Yes) return;

                    if (!Repository.TryDesactivarCategoria(sel.Id, out int cant))
                    {
                        MessageBox.Show(
                            $"No se puede desactivar la categoría \"{sel.Nombre}\" porque tiene {cant} producto(s) asignado(s).\n\n" +
                            "Primero reasigná o quitá esos productos.",
                            "Acción no permitida",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }
                    LoadData();
                }
            }
            else
            {
                if (grid.CurrentRow?.DataBoundItem is Subcategoria sel && sel.Activo)
                {
                    var resp = MessageBox.Show(
                        $"¿Confirmás desactivar la subcategoría \"{sel.Nombre}\"?",
                        "Confirmar desactivación",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (resp != DialogResult.Yes) return;

                    if (!Repository.TryDesactivarSubcategoria(sel.Id, out int cant))
                    {
                        MessageBox.Show(
                            $"No se puede desactivar la subcategoría \"{sel.Nombre}\" porque tiene {cant} producto(s) asignado(s).\n\n" +
                            "Primero reasigná o quitá esos productos.",
                            "Acción no permitida",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }
                    LoadData();
                }
            }
        }

        private class ComboItem
        {
            public string Text { get; set; } = "";
            public int? Value { get; set; }
            public override string ToString() => Text;
        }
    }
}
