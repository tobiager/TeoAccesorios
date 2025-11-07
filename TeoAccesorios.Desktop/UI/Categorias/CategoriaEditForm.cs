using System;
using System.Linq;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class CategoriaEditForm : Form
    {
        private readonly TextBox txtNombre = new TextBox { Width = 260 };
        private readonly TextBox txtDesc = new TextBox { Width = 260 };
        private readonly CheckBox chkActivo = new CheckBox { Text = "Activo" };
        private readonly Button btnOk = new Button { Text = "Guardar", Width = 90 };
        private readonly Button btnCancel = new Button { Text = "Cancelar", Width = 90 };
        private readonly ErrorProvider ep = new ErrorProvider();

        private readonly Categoria _model;

        public CategoriaEditForm(Categoria model)
        {
            _model = model ?? new Categoria();
            Text = model.Id == 0 ? "Nueva categoría" : "Editar categoría";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = MinimizeBox = false;
            Width = 420; Height = 210;

            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 2, RowCount = 4 };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            grid.Controls.Add(new Label { Text = "Nombre *", AutoSize = true, Padding = new Padding(0, 6, 6, 0) }, 0, 0);
            grid.Controls.Add(txtNombre, 1, 0);
            grid.Controls.Add(new Label { Text = "Descripción", AutoSize = true, Padding = new Padding(0, 6, 6, 0) }, 0, 1);
            grid.Controls.Add(txtDesc, 1, 1);
            grid.Controls.Add(chkActivo, 1, 2);

            var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Bottom, Height = 42, Padding = new Padding(12) };
            buttons.Controls.Add(btnOk);
            buttons.Controls.Add(btnCancel);

            Controls.Add(buttons);
            Controls.Add(grid);

            // Prefill
            txtNombre.Text = _model.Nombre ?? "";
            txtDesc.Text = _model.Descripcion ?? "";
            chkActivo.Checked = _model.Activo;

            // ErrorProvider setup (no parpadeo)
            ep.BlinkStyle = ErrorBlinkStyle.NeverBlink;

            // Eventos UX/validación
            CancelButton = btnCancel;
            txtNombre.TextChanged += (_, __) => UpdateGuardarState();
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

            var nombre = txtNombre.Text.Trim();

            // Requerido y longitud mínima/máxima
            if (string.IsNullOrWhiteSpace(nombre) || nombre.Length < 2 || nombre.Length > 80)
            {
                ep.SetError(txtNombre, "Nombre requerido (2–80 caracteres)");
                ok = false;
            }
            else
            {
                // Asegurar que el contenido sólo tenga letras y espacios
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

            // Unicidad (case-insensitive), excluyendo el propio registro al editar
            if (!string.IsNullOrEmpty(nombre))
            {
                var existe = Repository.ListarCategorias(true)
                    .Where(c => c.Id != _model.Id)
                    .Select(c => (c.Nombre ?? "").Trim())
                    .Any(n => string.Equals(n, nombre, StringComparison.OrdinalIgnoreCase));

                if (existe)
                {
                    ep.SetError(txtNombre, "Ya existe otra categoría con el mismo nombre");
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
