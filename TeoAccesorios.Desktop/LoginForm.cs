using System;
using System.Drawing;
using System.Windows.Forms;

namespace TeoAccesorios.Desktop
{
    public class LoginForm : Form
    {
        TextBox txtUser = new() { PlaceholderText = "Usuario" };
        TextBox txtPass = new() { PlaceholderText = "Contraseña", UseSystemPasswordChar = true };
        Button btnLogin = new() { Text = "Ingresar" };

        public LoginForm()
        {
            Text = "TeoAccesorios — Login";
            Width = 520;
            Height = 360;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            KeyPreview = true;

            // Tema oscuro simple
            BackColor = Color.FromArgb(10, 14, 28);
            ForeColor = Color.White;

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 3 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            var card = new Panel { Width = 440, Height = 240, BackColor = Color.FromArgb(18, 22, 35), Padding = new Padding(24), BorderStyle = BorderStyle.FixedSingle };

            var title = new Label
            {
                Text = "TeoAccesorios",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 46,
                ForeColor = Color.White
            };

            var subtitle = new Label
            {
                Text = "Iniciá sesión para continuar",
                Dock = DockStyle.Top,
                Height = 22,
                ForeColor = Color.Gainsboro
            };

            var formGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(4)
            };
            formGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            formGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            formGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            formGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            formGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));

            StylizeInput(txtUser);
            StylizeInput(txtPass);
            StylizeButton(btnLogin);

            formGrid.Controls.Add(new Label { Text = "Usuario", TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.Gainsboro }, 0, 0);
            formGrid.Controls.Add(txtUser, 1, 0);
            formGrid.Controls.Add(new Label { Text = "Contraseña", TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.Gainsboro }, 0, 1);
            formGrid.Controls.Add(txtPass, 1, 1);
            formGrid.Controls.Add(btnLogin, 1, 2);

            btnLogin.Click += (_, __) => DoLogin();
            AcceptButton = btnLogin; // Enter para ingresar
            Shown += (_, __) => txtUser.Focus();

            card.Controls.Add(formGrid);
            card.Controls.Add(subtitle);
            card.Controls.Add(title);

            layout.Controls.Add(card, 1, 1);
            Controls.Add(layout);
        }

        private static void StylizeInput(TextBox tb)
        {
            tb.BorderStyle = BorderStyle.FixedSingle;
            tb.BackColor = Color.FromArgb(28, 32, 48);
            tb.ForeColor = Color.White;
        }

        private static void StylizeButton(Button b)
        {
            b.Height = 34;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.BackColor = Color.FromArgb(94, 234, 212);
            b.ForeColor = Color.White;
        }

        private void DoLogin()
        {
            var usuario = (txtUser.Text ?? string.Empty).Trim();
            var pass = txtPass.Text ?? string.Empty;

            // Usa AuthService (parametrizado) y valida activo
            if (!AuthService.Login(usuario, pass, out var rol))
            {
                MessageBox.Show(this,
                    "Usuario y/o contraseña inválidos, o el usuario está inactivo.",
                    "No se pudo iniciar sesión",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPass.SelectAll();
                txtPass.Focus();
                return;
            }

            // Set sesión y abrir dashboard
            Sesion.Usuario = string.IsNullOrWhiteSpace(usuario) ? "Invitado" : usuario;
            Sesion.Rol = rol;

            Hide();
            using (var dash = new DashboardForm())
            {
                dash.ShowDialog(this);
            }
            Close();
        }
    }
}
