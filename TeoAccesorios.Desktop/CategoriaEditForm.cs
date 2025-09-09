using System;
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

        private readonly Categoria _model;

        public CategoriaEditForm(Categoria model)
        {
            _model = model;
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

            // bind
            txtNombre.Text = model.Nombre ?? "";
            txtDesc.Text = model.Descripcion ?? "";
            chkActivo.Checked = model.Activo;

            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;
            btnOk.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtNombre.Text))
                {
                    MessageBox.Show("Completá el nombre.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtNombre.Focus();
                    return;
                }
                _model.Nombre = txtNombre.Text.Trim();
                _model.Descripcion = string.IsNullOrWhiteSpace(txtDesc.Text) ? null : txtDesc.Text.Trim();
                _model.Activo = chkActivo.Checked;

                DialogResult = DialogResult.OK;
            };
        }
    }
}
