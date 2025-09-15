using System;
using System.Linq;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class CategoriasForm : Form
    {
        // ---- Estado
        private enum Modo { Categorias, Subcategorias }
        private Modo _modo = Modo.Categorias;

        // ---- Controles compartidos
        private readonly DataGridView grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
        private readonly BindingSource bs = new BindingSource();

        // ---- Barra superior: siempre visibles
        private readonly CheckBox chkInactivas = new CheckBox { Text = "Ver inactivas", AutoSize = true };
        private readonly Button btnNuevo;
        private readonly Button btnEditar;
        private readonly Button btnEliminar;
        private readonly Button btnRestaurar;
        private readonly Button btnSwitch;

        // ---- Solo en modo Subcategorías
        private readonly Label lblCat = new Label { Text = "Categoría:", AutoSize = true, Padding = new Padding(0, 10, 6, 0), Visible = false };
        private readonly ComboBox cboFiltroCat = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220, Visible = false };

        public CategoriasForm()
        {
            Text = "Categorías";
            Width = 900;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;

            // Crear botones con estilo angosto
            btnNuevo = CrearBoton("Nuevo");
            btnEditar = CrearBoton("Editar");
            btnEliminar = CrearBoton("Eliminar");
            btnRestaurar = CrearBoton("Restaurar");
            btnSwitch = CrearBoton("Subcategorías");

            // Barra superior
            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(6), WrapContents = false };
            top.Controls.Add(chkInactivas);
            top.Controls.Add(btnNuevo);
            top.Controls.Add(btnEditar);
            top.Controls.Add(btnEliminar);
            top.Controls.Add(btnRestaurar);
            top.Controls.Add(btnSwitch);
            // Controles visibles solo en Subcategorías
            top.Controls.Add(lblCat);
            top.Controls.Add(cboFiltroCat);

            Controls.Add(grid);
            Controls.Add(top);

            // Estilo
            GridHelper.Estilizar(grid);
            GridHelperLock.SoloLectura(grid);
            grid.DataSource = bs;

            // Eventos globales
            chkInactivas.CheckedChanged += (s, e) => LoadData();
            btnSwitch.Click += (s, e) => SwitchMode();

            // Handlers de acción
            btnNuevo.Click += (s, e) => OnNuevo();
            btnEditar.Click += (s, e) => OnEditar();
            btnEliminar.Click += (s, e) => OnEliminar();
            btnRestaurar.Click += (s, e) => OnRestaurar();

            // Eventos de subcategorías
            cboFiltroCat.SelectedIndexChanged += (s, e) => { if (_modo == Modo.Subcategorias) LoadData(); };

            // Inicial
            SetMode(Modo.Categorias);
        }

        // ===== Helper para crear botones angostos =====
        private Button CrearBoton(string texto)
        {
            return new Button
            {
                Text = texto,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(3, 1, 3, 1), // menos alto
                Margin = new Padding(2),
                MinimumSize = new Size(0, 24)       // altura más baja
            };
        }

        // ===== Alternar modo =====
        private void SwitchMode()
        {
            var nuevo = _modo == Modo.Categorias ? Modo.Subcategorias : Modo.Categorias;
            SetMode(nuevo);
        }

        private void SetMode(Modo m)
        {
            _modo = m;

            if (_modo == Modo.Categorias)
            {
                Text = "Categorías";
                btnSwitch.Text = "Subcategorías";
                lblCat.Visible = false;
                cboFiltroCat.Visible = false;
            }
            else
            {
                Text = "Subcategorías";
                btnSwitch.Text = "Categorías";
                lblCat.Visible = true;
                cboFiltroCat.Visible = true;

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

        // ===== Cargar datos =====
        private void LoadData()
        {
            if (_modo == Modo.Categorias)
            {
                var data = Repository.ListarCategorias(chkInactivas.Checked)
                                     .OrderBy(c => c.Nombre)
                                     .ToList();
                bs.DataSource = data;
                grid.DataSource = bs;
            }
            else
            {
                int? catId = (cboFiltroCat.SelectedItem as ComboItem)?.Value;
                var data = Repository.ListarSubcategorias(catId, chkInactivas.Checked)
                                     .OrderBy(s => s.CategoriaNombre)
                                     .ThenBy(s => s.Nombre)
                                     .ToList();
                bs.DataSource = data;
                grid.DataSource = bs;
            }
        }

        // ===== Acciones =====
        private void OnNuevo()
        {
            if (_modo == Modo.Categorias)
            {
                var cat = new Categoria { Activo = true };
                using var f = new CategoriaEditForm(cat);
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    cat.Id = Repository.InsertarCategoria(cat);
                    LoadData();
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
                    sub.Id = Repository.InsertarSubcategoria(sub);
                    LoadData();
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
                        Repository.ActualizarCategoria(tmp);
                        LoadData();
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
                        Repository.ActualizarSubcategoria(tmp);
                        LoadData();
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

        private void OnRestaurar()
        {
            if (_modo == Modo.Categorias)
            {
                if (grid.CurrentRow?.DataBoundItem is Categoria sel && !sel.Activo)
                {
                    Repository.SetCategoriaActiva(sel.Id, true);
                    LoadData();
                }
            }
            else
            {
                if (grid.CurrentRow?.DataBoundItem is Subcategoria sel && !sel.Activo)
                {
                    Repository.SetSubcategoriaActiva(sel.Id, true);
                    LoadData();
                }
            }
        }

        // ===== Helper combo =====
        private class ComboItem
        {
            public string Text { get; set; } = "";
            public int? Value { get; set; }
            public override string ToString() => Text;
        }
    }
}
