using System;
using System.Drawing;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class ClienteEditForm : Form
    {
        private readonly TextBox txtNombre = new();
        private readonly TextBox txtEmail = new();
        private readonly TextBox txtTel = new();
        private readonly TextBox txtDir = new();
        private readonly TextBox txtLoc = new();
        private readonly TextBox txtProv = new();
        private readonly CheckBox chkActivo = new() { Text = "Activo" };
        private readonly Button btnGuardar = new() { Text = "Guardar", Width = 100, Height = 30 };
        private readonly Button btnCancel = new() { Text = "Cancelar", Width = 100, Height = 30 };
        private readonly ErrorProvider ep = new();

        private readonly Cliente model;

        public ClienteEditForm(Cliente c)
        {
            model = c ?? new Cliente { Activo = true };

            Text = "Cliente";
            Width = 520; Height = 380;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;

            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(12) };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 7; i++) grid.RowStyles.Add(new RowStyle(SizeType.Absolute, i == 6 ? 46 : 36));

            grid.Controls.Add(new Label { Text = "Nombre *", TextAlign = ContentAlignment.MiddleLeft }, 0, 0); grid.Controls.Add(txtNombre, 1, 0);
            grid.Controls.Add(new Label { Text = "Email", TextAlign = ContentAlignment.MiddleLeft }, 0, 1); grid.Controls.Add(txtEmail, 1, 1);
            grid.Controls.Add(new Label { Text = "Teléfono", TextAlign = ContentAlignment.MiddleLeft }, 0, 2); grid.Controls.Add(txtTel, 1, 2);
            grid.Controls.Add(new Label { Text = "Dirección *", TextAlign = ContentAlignment.MiddleLeft }, 0, 3); grid.Controls.Add(txtDir, 1, 3);
            grid.Controls.Add(new Label { Text = "Localidad *", TextAlign = ContentAlignment.MiddleLeft }, 0, 4); grid.Controls.Add(txtLoc, 1, 4);
            grid.Controls.Add(new Label { Text = "Provincia *", TextAlign = ContentAlignment.MiddleLeft }, 0, 5); grid.Controls.Add(txtProv, 1, 5);

            var panelButtons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            panelButtons.Controls.Add(btnGuardar);
            panelButtons.Controls.Add(btnCancel);
            panelButtons.Controls.Add(chkActivo);

            grid.Controls.Add(panelButtons, 0, 6);
            grid.SetColumnSpan(panelButtons, 2);

            Controls.Add(grid);

            AcceptButton = btnGuardar;
            CancelButton = btnCancel;

            // Prefill
            txtNombre.Text = c?.Nombre ?? "";
            txtEmail.Text = c?.Email ?? "";
            txtTel.Text = c?.Telefono ?? "";
            txtDir.Text = c?.Direccion ?? "";
            txtLoc.Text = c?.Localidad ?? "";
            txtProv.Text = c?.Provincia ?? "";
            chkActivo.Checked = c?.Activo ?? true;

            // Eventos
            txtNombre.TextChanged += (_, __) => btnGuardar.Enabled = Validar();
            txtEmail.TextChanged += (_, __) => btnGuardar.Enabled = Validar();
            txtTel.TextChanged += (_, __) => btnGuardar.Enabled = Validar();
            txtDir.TextChanged += (_, __) => btnGuardar.Enabled = Validar();
            txtLoc.TextChanged += (_, __) => btnGuardar.Enabled = Validar();
            txtProv.TextChanged += (_, __) => btnGuardar.Enabled = Validar();

            btnGuardar.Click += (_, __) =>
            {
                if (!Validar()) return;

                model.Nombre = txtNombre.Text.Trim();
                model.Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim();
                model.Telefono = string.IsNullOrWhiteSpace(txtTel.Text) ? null : txtTel.Text.Trim();
                model.Direccion = txtDir.Text.Trim();
                model.Localidad = txtLoc.Text.Trim();
                model.Provincia = txtProv.Text.Trim();
                model.Activo = chkActivo.Checked;

                DialogResult = DialogResult.OK;
                Close();
            };

            btnCancel.Click += (_, __) => { DialogResult = DialogResult.Cancel; Close(); };

            btnGuardar.Enabled = Validar();
        }

        private bool Validar()
        {
            bool ok = true;
            ok &= FormValidator.Require(txtNombre, ep, "Nombre requerido", 2, 120);
            ok &= FormValidator.Require(txtDir, ep, "Dirección requerida", 3, 120);
            ok &= FormValidator.Require(txtLoc, ep, "Localidad requerida", 2, 80);
            ok &= FormValidator.Require(txtProv, ep, "Provincia requerida", 2, 80);
            ok &= FormValidator.OptionalEmail(txtEmail, ep, "Email inválido");
            ok &= FormValidator.OptionalPhone(txtTel, ep, "Teléfono inválido");
            return ok;
        }
    }
}
