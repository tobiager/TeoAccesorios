using System;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using TeoAccesorios.Desktop.Infra.Auth;

namespace TeoAccesorios.Desktop.UI.Usuarios
{
    public class CambiarContraseniaForm : Form
    {
        private readonly TextBox txtActual = new() { UseSystemPasswordChar = true };
        private readonly TextBox txtNueva = new() { UseSystemPasswordChar = true };
        private readonly TextBox txtConfirmar = new() { UseSystemPasswordChar = true };

        private readonly Button btnGuardar = new() { Text = "Guardar", Dock = DockStyle.Bottom, Height = 36 };
        private readonly Button btnCancelar = new() { Text = "Cancelar", Dock = DockStyle.Bottom, Height = 32 };
        private readonly ErrorProvider ep = new();

        public CambiarContraseniaForm()
        {
            Text = "Cambiar Contraseña";
            Width = 480; Height = 240;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;

            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(12) };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 3; i++) grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));

            grid.Controls.Add(new Label { Text = "Contraseña actual *", AutoSize = true, Padding = new Padding(0, 6, 0, 0) }, 0, 0);
            grid.Controls.Add(txtActual, 1, 0);
            grid.Controls.Add(new Label { Text = "Nueva contraseña *", AutoSize = true, Padding = new Padding(0, 6, 0, 0) }, 0, 1);
            grid.Controls.Add(txtNueva, 1, 1);
            grid.Controls.Add(new Label { Text = "Confirmar contraseña *", AutoSize = true, Padding = new Padding(0, 6, 0, 0) }, 0, 2);
            grid.Controls.Add(txtConfirmar, 1, 2);

            Controls.Add(btnCancelar);
            Controls.Add(btnGuardar);
            Controls.Add(grid);

            AcceptButton = btnGuardar;
            CancelButton = btnCancelar;

            // Eventos
            btnCancelar.Click += (_, __) => DialogResult = DialogResult.Cancel;
            btnGuardar.Click += (_, __) => Guardar();

            // Habilitar botón al escribir
            txtActual.TextChanged += (_, __) => btnGuardar.Enabled = ValidarCamposNoVacios();
            txtNueva.TextChanged += (_, __) => btnGuardar.Enabled = ValidarCamposNoVacios();
            txtConfirmar.TextChanged += (_, __) => btnGuardar.Enabled = ValidarCamposNoVacios();

            btnGuardar.Enabled = false;
        }

        private bool ValidarCamposNoVacios()
        {
            return !string.IsNullOrWhiteSpace(txtActual.Text) &&
                   !string.IsNullOrWhiteSpace(txtNueva.Text) &&
                   !string.IsNullOrWhiteSpace(txtConfirmar.Text);
        }

        private void Guardar()
        {
            ep.Clear();
            bool ok = true;

            if (string.IsNullOrWhiteSpace(txtActual.Text)) { ep.SetError(txtActual, "Requerido"); ok = false; }
            if (string.IsNullOrWhiteSpace(txtNueva.Text)) { ep.SetError(txtNueva, "Requerido"); ok = false; }
            if (string.IsNullOrWhiteSpace(txtConfirmar.Text)) { ep.SetError(txtConfirmar, "Requerido"); ok = false; }

            if (txtNueva.Text.Length < 6) { ep.SetError(txtNueva, "La nueva contraseña debe tener al menos 6 caracteres."); ok = false; }
            if (txtNueva.Text != txtConfirmar.Text) { ep.SetError(txtConfirmar, "Las contraseñas no coinciden."); ok = false; }

            if (!ok) return;

            // Validar contraseña actual contra la BD
            try
            {
                // Hashear las contraseñas
                byte[] passwordActualHash = PasswordHelper.HashPassword(txtActual.Text);
                byte[] passwordNuevaHash = PasswordHelper.HashPassword(txtNueva.Text);

                using var cn = new SqlConnection(Db.ConnectionString);
                using var cmd = new SqlCommand("SELECT 1 FROM dbo.Usuarios WHERE Id = @id AND contrasenia = @p", cn);
                cmd.Parameters.AddWithValue("@id", Sesion.UsuarioId);
                cmd.Parameters.Add("@p", System.Data.SqlDbType.VarBinary, 32).Value = passwordActualHash;

                cn.Open();
                var result = cmd.ExecuteScalar();

                if (result == null)
                {
                    ep.SetError(txtActual, "La contraseña actual es incorrecta.");
                    return;
                }

                // Actualizar contraseña con hash
                Db.Exec("UPDATE dbo.Usuarios SET contrasenia = @p WHERE Id = @id",
                    new SqlParameter("@p", System.Data.SqlDbType.VarBinary) { Value = passwordNuevaHash },
                    new SqlParameter("@id", Sesion.UsuarioId)
                );

                MessageBox.Show("Su contraseña ha sido actualizada correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocurrió un error al intentar cambiar la contraseña.\n\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}