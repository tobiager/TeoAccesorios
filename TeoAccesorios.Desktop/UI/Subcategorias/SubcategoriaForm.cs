using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class SubcategoriasForm : Form
    {
        private readonly DataGridView grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
        private readonly BindingSource bs = new BindingSource();

        // Filtro por categoría
        private readonly Label lblCat = new Label { Text = "Categoría:", AutoSize = true, Padding = new Padding(0, 10, 6, 0) };
        private readonly ComboBox cboFiltroCat = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };

        // Botones (mismo set que CategoriasForm)
        private readonly Button btnNuevo;
        private readonly Button btnEditar;
        private readonly Button btnEliminar;
        private readonly Button btnVerInactivas;

        private readonly int? _categoriaInicial;
        private readonly bool tienePermisos;

        public SubcategoriasForm() : this(null) { }

        public SubcategoriasForm(int? categoriaId)
        {
            // El Gerente ahora también tiene todos los permisos en subcategorías
            tienePermisos = Sesion.Rol == RolUsuario.Admin || Sesion.Rol == RolUsuario.Gerente;
            _categoriaInicial = categoriaId;

            Text = "Subcategorías";
            Width = 900;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;

            // Botones con el mismo estilo que CategoriasForm
            btnNuevo = CrearBoton("Nuevo");
            btnEditar = CrearBoton("Editar");
            btnEliminar = CrearBoton("Eliminar");
            btnVerInactivas = CrearBoton("Ver inactivas");

            var top = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                Padding = new Padding(6),
                WrapContents = false,     // una sola línea, no rompas el layout
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = false
            };

            // Orden idéntico al de CategoriasForm: botones primero y luego filtro
            top.Controls.Add(btnNuevo);
            top.Controls.Add(btnEditar);
            top.Controls.Add(btnEliminar);
            top.Controls.Add(btnVerInactivas);

            // Un pequeño margen para que el combo no tape el botón de "Ver inactivas"
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

            // Cargar categorías para el filtro
            CargarCategoriasEnCombo();
            if (_categoriaInicial.HasValue)
                SeleccionarCategoriaEnCombo(_categoriaInicial.Value);

            // Eventos
            cboFiltroCat.SelectedIndexChanged += (s, e) => LoadSubcategorias();

            btnVerInactivas.Click += (s, e) =>
            {
                using var f = new SubcategoriasInactivasForm();
                f.ShowDialog(this);
                LoadSubcategorias();
            };

            if (!tienePermisos)
            {
                // Solo vista para vendedor
                btnNuevo.Visible = btnEditar.Visible = btnEliminar.Visible = btnVerInactivas.Visible = false;
                grid.CellDoubleClick += (_, __) => { /* sin acción */ };
            }
            else
            {
                // Acciones para admin y gerente
                btnNuevo.Click += (s, e) =>
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
                        sub.Id = Repository.InsertarSubcategoria(sub);
                        LoadSubcategorias();
                    }
                };

                btnEditar.Click += (s, e) =>
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
                            Repository.ActualizarSubcategoria(tmp);
                            LoadSubcategorias();
                        }
                    }
                };

                btnEliminar.Click += (s, e) =>
                {
                    if (grid.CurrentRow?.DataBoundItem is Subcategoria sel && sel.Activo)
                    {
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
                        LoadSubcategorias();
                    }
                };

                grid.CellDoubleClick += (s, e) => btnEditar.PerformClick();
            }

            LoadSubcategorias();
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

        private void CargarCategoriasEnCombo()
        {
            cboFiltroCat.Items.Clear();
            cboFiltroCat.Items.Add(new ComboItem { Text = "Todas", Value = null });
            foreach (var c in Repository.ListarCategorias(true).OrderBy(x => x.Nombre))
                cboFiltroCat.Items.Add(new ComboItem { Text = $"{c.Id} - {c.Nombre}", Value = c.Id });
            cboFiltroCat.SelectedIndex = 0;
        }

        private void SeleccionarCategoriaEnCombo(int categoriaId)
        {
            for (int i = 0; i < cboFiltroCat.Items.Count; i++)
            {
                if ((cboFiltroCat.Items[i] as ComboItem)?.Value == categoriaId)
                {
                    cboFiltroCat.SelectedIndex = i;
                    break;
                }
            }
        }

        private void LoadSubcategorias()
        {
            int? catId = (cboFiltroCat.SelectedItem as ComboItem)?.Value;
            var data = Repository.ListarSubcategorias(catId, false)
                                 .OrderBy(s => s.CategoriaNombre)
                                 .ThenBy(s => s.Nombre)
                                 .ToList();
            bs.DataSource = data;
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
