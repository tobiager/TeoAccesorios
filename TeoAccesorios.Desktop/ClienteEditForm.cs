using System;
using System.Drawing;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class ClienteEditForm : Form
    {
        private readonly TextBox txtNombre = new TextBox();
        private readonly TextBox txtEmail = new TextBox();
        private readonly TextBox txtTel = new TextBox();
        private readonly TextBox txtDir = new TextBox();
        private readonly TextBox txtLoc = new TextBox();
        private readonly TextBox txtProv = new TextBox();
        private readonly CheckBox chkActivo = new CheckBox { Text = "Activo" };
        private readonly Cliente model;

        public ClienteEditForm(Cliente c)
        {
            model = c;

            Text = "Cliente";
            Width = 520;
            Height = 360;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(12) };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 7; i++)
                grid.RowStyles.Add(new RowStyle(SizeType.Absolute, i == 6 ? 46 : 36));

            grid.Controls.Add(new Label { Text = "Nombre", TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            grid.Controls.Add(txtNombre, 1, 0);

            grid.Controls.Add(new Label { Text = "Email", TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
            grid.Controls.Add(txtEmail, 1, 1);

            grid.Controls.Add(new Label { Text = "Teléfono", TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
            grid.Controls.Add(txtTel, 1, 2);

            grid.Controls.Add(new Label { Text = "Dirección", TextAlign = ContentAlignment.MiddleLeft }, 0, 3);
            grid.Controls.Add(txtDir, 1, 3);

            grid.Controls.Add(new Label { Text = "Localidad", TextAlign = ContentAlignment.MiddleLeft }, 0, 4);
            grid.Controls.Add(txtLoc, 1, 4);

            grid.Controls.Add(new Label { Text = "Provincia", TextAlign = ContentAlignment.MiddleLeft }, 0, 5);
            grid.Controls.Add(txtProv, 1, 5);

            var panelButtons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            var btnOk = new Button { Text = "Guardar", Width = 100, Height = 30 };
            var btnCancel = new Button { Text = "Cancelar", Width = 100, Height = 30 };

            btnOk.Click += (s, e) =>
            {
                model.Nombre = (txtNombre.Text ?? "").Trim();
                model.Email = (txtEmail.Text ?? "").Trim();
                model.Telefono = (txtTel.Text ?? "").Trim();
                model.Direccion = (txtDir.Text ?? "").Trim();
                model.Localidad = (txtLoc.Text ?? "").Trim();
                model.Provincia = (txtProv.Text ?? "").Trim();
                model.Activo = chkActivo.Checked;

                DialogResult = DialogResult.OK;
                Close();
            };
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            panelButtons.Controls.Add(btnOk);
            panelButtons.Controls.Add(btnCancel);
            panelButtons.Controls.Add(chkActivo);

            grid.Controls.Add(panelButtons, 0, 6);
            grid.SetColumnSpan(panelButtons, 2);

            Controls.Add(grid);

            // Cargar datos del modelo
            txtNombre.Text = c.Nombre;
            txtEmail.Text = c.Email;
            txtTel.Text = c.Telefono;
            txtDir.Text = c.Direccion;
            txtLoc.Text = c.Localidad;
            txtProv.Text = c.Provincia;
            chkActivo.Checked = c.Activo;
        }
    }
}
