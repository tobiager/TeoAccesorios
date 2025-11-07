using System;
using System.Linq;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class SubcategoriaEditForm : Form
    {
        private readonly ComboBox cboCategoria = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };
        private readonly TextBox txtNombre = new TextBox { Width = 260 };
        private readonly TextBox txtDesc = new TextBox { Width = 260 };
        private readonly CheckBox chkActivo = new CheckBox { Text = "Activo" };
        private readonly Button btnOk = new Button { Text = "Guardar", Width = 90 };
        private readonly Button btnCancel = new Button { Text = "Cancelar", Width = 90 };
        private readonly ErrorProvider ep = new ErrorProvider();

        private readonly Subcategoria _model;

        public SubcategoriaEditForm(Subcategoria model)
        {
            _model = model ?? new Subcategoria();
            Text = model.Id == 0 ? "Nueva subcategoría" : "Editar subcategoría";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = MinimizeBox = false;
            Width = 460; Height = 250;

            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 2, RowCount = 5 };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            grid.Controls.Add(new Label { Text = "Categoría *", AutoSize = true, Padding = new Padding(0, 6, 6, 0) }, 0, 0);
            grid.Controls.Add(cboCategoria, 1, 0);
            grid.Controls.Add(new Label { Text = "Nombre *", AutoSize = true, Padding = new Padding(0, 6, 6, 0) }, 0, 1);
            grid.Controls.Add(txtNombre, 1, 1);
            grid.Controls.Add(new Label { Text = "Descripción", AutoSize = true, Padding = new Padding(0, 6, 6, 0) }, 0, 2);
            grid.Controls.Add(txtDesc, 1, 2);
            grid.Controls.Add(chkActivo, 1, 3);

            var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Bottom, Height = 42, Padding = new Padding(12) };
            buttons.Controls.Add(btnOk);
            buttons.Controls.Add(btnCancel);

            Controls.Add(buttons);
            Controls.Add(grid);

            // cargar categorías
            var cats = Repository.ListarCategorias(true).OrderBy(x => x.Nombre).ToList();
            cboCategoria.ValueMember = nameof(Categoria.Id);
            cboCategoria.DisplayMember = nameof(Categoria.Nombre);
            cboCategoria.DataSource = cats;
            if (_model.CategoriaId != 0)
            {
                var idx = cats.FindIndex(c => c.Id == _model.CategoriaId);
                if (idx >= 0) cboCategoria.SelectedIndex = idx;
            }

            // bind
            txtNombre.Text = _model.Nombre ?? "";
            txtDesc.Text = _model.Descripcion ?? "";
            chkActivo.Checked = _model.Activo;

            // ErrorProvider UX
            ep.BlinkStyle = ErrorBlinkStyle.NeverBlink;

            // Eventos UX/validación
            CancelButton = btnCancel;
            txtNombre.TextChanged += (_, __) => UpdateGuardarState();
            cboCategoria.SelectedIndexChanged += (_, __) => UpdateGuardarState();
            txtDesc.TextChanged += (_, __) => UpdateGuardarState();

            // Evitar escribir símbolos o números: solo letras, espacios y teclas de control
            txtNombre.KeyPress += (_, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsLetter(e.KeyChar) && !char.IsWhiteSpace(e.KeyChar))
                    e.Handled = true;
            };

            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;
            btnOk.Click += (s, e) =>
            {
                if (!ValidarTodo()) return;

                var selCat = cboCategoria.SelectedItem as Categoria;
                _model.CategoriaId = selCat!.Id;
                _model.Nombre = txtNombre.Text.Trim();
                _model.Descripcion = string.IsNullOrWhiteSpace(txtDesc.Text) ? null : txtDesc.Text.Trim();
                _model.Activo = chkActivo.Checked;

                DialogResult = DialogResult.OK;
            };

            // Estado inicial
            UpdateGuardarState();
        }

        private bool ValidarTodo()
        {
            bool ok = true;
            ep.Clear();

            // Categoría requerida
            var selCat = cboCategoria.SelectedItem as Categoria;
            if (selCat is null)
            {
                ep.SetError(cboCategoria, "Seleccioná una categoría");
                ok = false;
            }

            var nombre = txtNombre.Text.Trim();

            // Requerido y longitud
            if (string.IsNullOrWhiteSpace(nombre) || nombre.Length < 2 || nombre.Length > 80)
            {
                ep.SetError(txtNombre, "Nombre requerido (2–80 caracteres)");
                ok = false;
            }
            else
            {
                // Sólo letras y espacios (defensivo, KeyPress ya lo filtra)
                foreach (var ch in nombre)
                {
                    if (!(char.IsLetter(ch) || char.IsWhiteSpace(ch)))
                    {
                        ep.SetError(txtNombre, "Solo se permiten letras y espacios");
                        ok = false;
                        break;
                    }
                }
            }

            // Unicidad por categoría (case-insensitive), excluir el propio registro al editar
            if (!string.IsNullOrEmpty(nombre) && selCat is not null)
            {
                var existe = Repository.ListarSubcategorias(selCat.Id, true)
                    .Where(s => s.Id != _model.Id)
                    .Select(s => (s.Nombre ?? "").Trim())
                    .Any(n => string.Equals(n, nombre, StringComparison.OrdinalIgnoreCase));

                if (existe)
                {
                    ep.SetError(txtNombre, "Ya existe otra subcategoría con el mismo nombre en la categoría seleccionada");
                    ok = false;
                }
            }

            return ok;
        }

        private void UpdateGuardarState()
        {
            bool valido = ValidarTodo();
            btnOk.Enabled = valido;
            AcceptButton = valido ? btnOk : null;
        }
    }
}
