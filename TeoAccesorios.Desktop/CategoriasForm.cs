using System;
using System.Linq;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class CategoriasForm : Form
    {
        // ---- Categorías
        private readonly DataGridView gridCat = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
        private readonly BindingSource bsCat = new BindingSource();
        private readonly CheckBox chkCatInactivas = new CheckBox { Text = "Ver inactivas" };

        // ---- Subcategorías
        private readonly DataGridView gridSub = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
        private readonly BindingSource bsSub = new BindingSource();
        private readonly ComboBox cboFiltroCat = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
        private readonly CheckBox chkSubInactivas = new CheckBox { Text = "Ver inactivas" };

        public CategoriasForm()
        {
            Text = "Categorías y Subcategorías";
            Width = 900;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;

            var tabs = new TabControl { Dock = DockStyle.Fill };

            // === TAB CATEGORÍAS ===
            var tpCat = new TabPage("Categorías") { Padding = new Padding(6) };
            var topCat = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(6) };
            var btnCatNuevo = new Button { Text = "Nuevo" };
            var btnCatEditar = new Button { Text = "Editar" };
            var btnCatEliminar = new Button { Text = "Eliminar" };
            var btnCatRestaurar = new Button { Text = "Restaurar" };
            topCat.Controls.Add(chkCatInactivas);
            topCat.Controls.Add(btnCatNuevo);
            topCat.Controls.Add(btnCatEditar);
            topCat.Controls.Add(btnCatEliminar);
            topCat.Controls.Add(btnCatRestaurar);
            tpCat.Controls.Add(gridCat);
            tpCat.Controls.Add(topCat);

            // === TAB SUBCATEGORÍAS ===
            var tpSub = new TabPage("Subcategorías") { Padding = new Padding(6) };
            var topSub = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(6) };
            topSub.Controls.Add(new Label { Text = "Categoría:", AutoSize = true, Padding = new Padding(0, 8, 6, 0) });
            topSub.Controls.Add(cboFiltroCat);
            topSub.Controls.Add(chkSubInactivas);
            var btnSubNuevo = new Button { Text = "Nuevo" };
            var btnSubEditar = new Button { Text = "Editar" };
            var btnSubEliminar = new Button { Text = "Eliminar" };
            var btnSubRestaurar = new Button { Text = "Restaurar" };
            topSub.Controls.AddRange(new Control[] { btnSubNuevo, btnSubEditar, btnSubEliminar, btnSubRestaurar });
            tpSub.Controls.Add(gridSub);
            tpSub.Controls.Add(topSub);

            tabs.TabPages.Add(tpCat);
            tabs.TabPages.Add(tpSub);

            Controls.Add(tabs);

            // estilo
            GridHelper.Estilizar(gridCat);
            GridHelperLock.SoloLectura(gridCat);
            GridHelper.Estilizar(gridSub);
            GridHelperLock.SoloLectura(gridSub);

            gridCat.DataSource = bsCat;
            gridSub.DataSource = bsSub;

            // Cargar combos
            cboFiltroCat.Items.Clear();
            cboFiltroCat.Items.Add(new ComboItem { Text = "Todas", Value = null });
            foreach (var c in Repository.ListarCategorias(true).OrderBy(x => x.Nombre))
                cboFiltroCat.Items.Add(new ComboItem { Text = $"{c.Id} - {c.Nombre}", Value = c.Id });
            cboFiltroCat.SelectedIndex = 0;

            // Handlers
            chkCatInactivas.CheckedChanged += (s, e) => LoadCategorias();
            chkSubInactivas.CheckedChanged += (s, e) => LoadSubcategorias();
            cboFiltroCat.SelectedIndexChanged += (s, e) => LoadSubcategorias();

            // ABM Categorías
            btnCatNuevo.Click += (s, e) =>
            {
                var cat = new Categoria { Activo = true };
                using var f = new CategoriaEditForm(cat);
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    cat.Id = Repository.InsertarCategoria(cat);
                    LoadCategorias();
                }
            };

            btnCatEditar.Click += (s, e) =>
            {
                if (gridCat.CurrentRow?.DataBoundItem is Categoria sel)
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
                        LoadCategorias();
                        LoadSubcategorias(); // por si afectan nombres
                    }
                }
            };

            btnCatEliminar.Click += (s, e) =>
            {
                if (gridCat.CurrentRow?.DataBoundItem is Categoria sel && sel.Activo)
                {
                    Repository.SetCategoriaActiva(sel.Id, false);
                    LoadCategorias();
                    LoadSubcategorias();
                }
            };

            btnCatRestaurar.Click += (s, e) =>
            {
                if (gridCat.CurrentRow?.DataBoundItem is Categoria sel && !sel.Activo)
                {
                    Repository.SetCategoriaActiva(sel.Id, true);
                    LoadCategorias();
                    LoadSubcategorias();
                }
            };

            // ABM Subcategorías
            btnSubNuevo.Click += (s, e) =>
            {
                int? catId = (cboFiltroCat.SelectedItem as ComboItem)?.Value;
                var sub = new Subcategoria { Activo = true, CategoriaId = catId ?? Repository.ListarCategorias(true).First().Id };
                using var f = new SubcategoriaEditForm(sub);
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    sub.Id = Repository.InsertarSubcategoria(sub);
                    LoadSubcategorias();
                }
            };

            btnSubEditar.Click += (s, e) =>
            {
                if (gridSub.CurrentRow?.DataBoundItem is Subcategoria sel)
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

            btnSubEliminar.Click += (s, e) =>
            {
                if (gridSub.CurrentRow?.DataBoundItem is Subcategoria sel && sel.Activo)
                {
                    Repository.SetSubcategoriaActiva(sel.Id, false);
                    LoadSubcategorias();
                }
            };

            btnSubRestaurar.Click += (s, e) =>
            {
                if (gridSub.CurrentRow?.DataBoundItem is Subcategoria sel && !sel.Activo)
                {
                    Repository.SetSubcategoriaActiva(sel.Id, true);
                    LoadSubcategorias();
                }
            };

            // load
            LoadCategorias();
            LoadSubcategorias();
        }

        private void LoadCategorias()
        {
            var data = Repository.ListarCategorias(chkCatInactivas.Checked)
                                 .OrderBy(c => c.Nombre)
                                 .ToList();
            bsCat.DataSource = data;
            gridCat.DataSource = bsCat;
        }

        private void LoadSubcategorias()
        {
            int? catId = (cboFiltroCat.SelectedItem as ComboItem)?.Value;
            var data = Repository.ListarSubcategorias(catId, chkSubInactivas.Checked)
                                 .OrderBy(s => s.CategoriaNombre)
                                 .ThenBy(s => s.Nombre)
                                 .ToList();
            bsSub.DataSource = data;
            gridSub.DataSource = bsSub;
        }

        private class ComboItem
        {
            public string Text { get; set; } = "";
            public int? Value { get; set; }
            public override string ToString() => Text;
        }
    }
}
