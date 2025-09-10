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

            // Tema oscuro
            BackColor = Color.FromArgb(10, 14, 28);
            ForeColor = Color.White;

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 3 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            var card = new Panel
            {
                Width = 460,
                Height = 260,
                BackColor = Color.FromArgb(18, 22, 35),
                Padding = new Padding(24),
                BorderStyle = BorderStyle.FixedSingle
            };

            // ===== Grid principal dentro del card =====
            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F)); // izquierda: textos+inputs
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F)); // derecha: logo

            // ==== Panel izquierdo: título + subtítulo + campos ====
            var leftPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(0, 20, 0, 0), 
                AutoScroll = false
            };

            var title = new Label
            {
                Text = "TeoAccesorios",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Height = 40,
                ForeColor = Color.White,
                AutoSize = true
            };

            var subtitle = new Label
            {
                Text = "Iniciá sesión para continuar",
                Height = 22,
                ForeColor = Color.Gainsboro,
                AutoSize = true
            };

            var formGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(0, 10, 0, 0)
            };
            formGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            formGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            formGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            formGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            formGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

            StylizeInput(txtUser);
            StylizeInput(txtPass);
            StylizeButton(btnLogin);

            formGrid.Controls.Add(new Label { Text = "Usuario", ForeColor = Color.Gainsboro, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            formGrid.Controls.Add(txtUser, 1, 0);
            formGrid.Controls.Add(new Label { Text = "Contraseña", ForeColor = Color.Gainsboro, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
            formGrid.Controls.Add(txtPass, 1, 1);
            formGrid.Controls.Add(btnLogin, 1, 2);

            leftPanel.Controls.Add(title);
            leftPanel.Controls.Add(subtitle);
            leftPanel.Controls.Add(formGrid);

            // ==== Panel derecho: logo ====
            var picLogo = new PictureBox
            {
                Image = global::TeoAccesorios.Desktop.Properties.Resources.logo,
                SizeMode = PictureBoxSizeMode.Zoom,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 10, 0, 0) 
            };

            grid.Controls.Add(leftPanel, 0, 0);
            grid.Controls.Add(picLogo, 1, 0);

            card.Controls.Add(grid);

            layout.Controls.Add(card, 1, 1);
            Controls.Add(layout);

            // Eventos
            btnLogin.Click += (_, __) => DoLogin();
            AcceptButton = btnLogin;
            Shown += (_, __) => txtUser.Focus();
        }

        private static void StylizeInput(TextBox tb)
        {
            tb.BorderStyle = BorderStyle.FixedSingle;
            tb.BackColor = Color.FromArgb(28, 32, 48);
            tb.ForeColor = Color.White;
            tb.Width = 200;
        }

        private static void StylizeButton(Button b)
        {
            b.Height = 34;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.BackColor = Color.FromArgb(26, 139, 156);
            b.ForeColor = Color.White;
            b.Width = 120;
        }

        private void DoLogin()
        {
            var usuario = (txtUser.Text ?? string.Empty).Trim();
            var pass = txtPass.Text ?? string.Empty;

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
