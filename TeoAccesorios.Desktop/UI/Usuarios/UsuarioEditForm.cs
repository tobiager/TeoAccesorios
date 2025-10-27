using System;
using System.Linq;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class UsuarioEditForm : Form
    {
        private readonly TextBox txtUser = new();
        private readonly TextBox txtMail = new();
        private readonly TextBox txtPass = new() { UseSystemPasswordChar = true };
        private readonly ComboBox cboRol = new() { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly CheckBox chkActivo = new() { Text = "Activo" };

        private readonly Button btnGuardar = new() { Text = "Guardar", Dock = DockStyle.Bottom, Height = 36 };
        private readonly Button btnCancelar = new() { Text = "Cancelar", Dock = DockStyle.Bottom, Height = 32 };
        private readonly ErrorProvider ep = new();

        private readonly Usuario model;
        private readonly bool esNuevoUsuario;
        public Usuario Result => model;

        public UsuarioEditForm(Usuario u)
        {
            model = u ?? new Usuario();
            esNuevoUsuario = model.Id == 0;

            Text = "Usuario";
            Width = 480; Height = 300;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;

            // Configurar roles disponibles basado en si ya existe un gerente
            ConfigurarRolesDisponibles();

            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(12) };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 5; i++) grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));

            grid.Controls.Add(new Label { Text = "Usuario *" }, 0, 0); grid.Controls.Add(txtUser, 1, 0);
            grid.Controls.Add(new Label { Text = "Correo *" }, 0, 1); grid.Controls.Add(txtMail, 1, 1);
            grid.Controls.Add(new Label { Text = "Contraseña *" }, 0, 2); grid.Controls.Add(txtPass, 1, 2);
            grid.Controls.Add(new Label { Text = "Rol *" }, 0, 3); grid.Controls.Add(cboRol, 1, 3);
            grid.Controls.Add(chkActivo, 1, 4);

            Controls.Add(btnCancelar);
            Controls.Add(btnGuardar);
            Controls.Add(grid);

            AcceptButton = btnGuardar;
            CancelButton = btnCancelar;

            // Prefill
            txtUser.Text = u?.NombreUsuario ?? "";
            txtMail.Text = u?.Correo ?? "";
            
            // DESHABILITAR COMPLETAMENTE EL CAMPO DE CONTRASEÑA
            if (esNuevoUsuario)
            {
                txtPass.Text = "default123";
            }
            else
            {
                txtPass.Text = "********"; // Mostrar asteriscos para usuarios existentes
            }
            
            // Configurar el campo como no editable SIEMPRE
            txtPass.ReadOnly = true;
            txtPass.BackColor = System.Drawing.SystemColors.Control;
            txtPass.TabStop = false;
            txtPass.Cursor = Cursors.No; // Cursor que indica que no se puede editar
            
            cboRol.SelectedItem = string.IsNullOrWhiteSpace(u?.Rol) ? "Vendedor" : u.Rol;
            chkActivo.Checked = u?.Activo ?? true;

            
            txtUser.TextChanged += (_, __) => btnGuardar.Enabled = Validar();
            txtMail.TextChanged += (_, __) => btnGuardar.Enabled = Validar();
            cboRol.SelectedIndexChanged += (_, __) => btnGuardar.Enabled = Validar();

            btnCancelar.Click += (_, __) => DialogResult = DialogResult.Cancel;

            btnGuardar.Click += (_, __) =>
            {
                // --- Validaciones de permisos por rol ---
                var rolSeleccionado = (string)cboRol.SelectedItem;
                var esGerente = model.Rol?.Equals("Gerente", StringComparison.OrdinalIgnoreCase) ?? false;

                // Un Admin no puede crear/editar otros Admins o Gerentes
                if (Sesion.Rol == RolUsuario.Admin && (rolSeleccionado == "Admin" || rolSeleccionado == "Gerente"))
                {
                    MessageBox.Show("Un Administrador no puede crear o modificar usuarios de tipo Administrador o Gerente.", "Permiso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                
                if (rolSeleccionado == "Gerente" && Sesion.Rol != RolUsuario.Gerente)
                {
                    MessageBox.Show("Solo un Gerente puede crear o modificar a otro Gerente.", "Permiso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Vendedores no pueden crear/editar usuarios
                if (Sesion.Rol == RolUsuario.Vendedor)
                {
                    MessageBox.Show("No tiene permisos para crear o modificar usuarios.", "Permiso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // No se puede agregar otro gerente si ya existe uno
                if (rolSeleccionado == "Gerente" && model.Id == 0) // Es creación, no edición
                {
                    var gerentesExistentes = Repository.ListarUsuarios()
                        .Where(usr => usr.Activo && usr.Rol?.Equals("Gerente", StringComparison.OrdinalIgnoreCase) == true)
                        .ToList();

                    if (gerentesExistentes.Any())
                    {
                        MessageBox.Show("Ya existe un usuario con rol Gerente en el sistema. No se puede agregar otro.", "Restricción de gerente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // --- Fin validaciones de permisos ---

                if (!Validar()) return;

                model.NombreUsuario = txtUser.Text.Trim();
                model.Correo = txtMail.Text.Trim(); // Ya no puede ser null porque es obligatorio
                
                // LÓGICA DE CONTRASEÑA: Asignar solo para nuevos usuarios
                if (esNuevoUsuario)
                {
                    // Para nuevos usuarios, asignar contraseña por defecto
                    model.Contrasenia = "default123";
                }
                // Para usuarios existentes, NO tocar model.Contrasenia
                // La contraseña se maneja por separado (botón Restablecer en UsuariosForm)
                
                model.Rol = (string)cboRol.SelectedItem;
                model.Activo = chkActivo.Checked;

                // El Gerente no puede ser desactivado
                if (esGerente && !model.Activo)
                {
                    MessageBox.Show("El usuario con rol Gerente no puede ser desactivado.", "Acción no permitida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    model.Activo = true; 
                    chkActivo.Checked = true; 
                    return;
                }

                DialogResult = DialogResult.OK;
                Close();
            };

            btnGuardar.Enabled = Validar();
        }

        private void ConfigurarRolesDisponibles()
        {
            var rolesDisponibles = new List<string>();
            
            // Verificar si ya existe un gerente activo
            var existeGerente = Repository.ListarUsuarios()
                .Any(usr => usr.Activo && usr.Rol?.Equals("Gerente", StringComparison.OrdinalIgnoreCase) == true);
            
            // Si se está editando un usuario existente que es gerente, permitir mantener el rol
            var esGerenteExistente = !esNuevoUsuario && model.Rol?.Equals("Gerente", StringComparison.OrdinalIgnoreCase) == true;
            
            // Agregar "Gerente" solo si no existe uno O si estamos editando al gerente existente
            if (!existeGerente || esGerenteExistente)
            {
                rolesDisponibles.Add("Gerente");
            }
            
            rolesDisponibles.Add("Admin");
            rolesDisponibles.Add("Vendedor");
            
            cboRol.Items.AddRange(rolesDisponibles.ToArray());
        }

        private bool Validar()
        {
            bool ok = true;
            ok &= FormValidator.Require(txtUser, ep, "Usuario requerido (3–40)", 3, 40);
            
            // NO VALIDAR CONTRASEÑA - Se maneja por separado con el botón Restablecer
            ep.SetError(txtPass, ""); 
            
            // CAMBIO CRÍTICO: Hacer el correo obligatorio
            ok &= RequireEmail(txtMail, ep, "Correo requerido y válido");
            if (cboRol.SelectedIndex < 0) { ep.SetError(cboRol, "Seleccioná un rol"); ok = false; } else ep.SetError(cboRol, "");
            return ok;
        }
        
        /// <summary>
        /// Valida que el correo sea obligatorio y tenga formato válido
        /// </summary>
        private bool RequireEmail(TextBox tb, ErrorProvider ep, string msg)
        {
            string v = tb.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(v))
            {
                ep.SetError(tb, msg);
                return false;
            }

            // Validar formato de email
            var emailValid = System.Text.RegularExpressions.Regex.IsMatch(v, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!emailValid)
            {
                ep.SetError(tb, "Correo inválido");
                return false;
            }

            ep.SetError(tb, "");
            return true;
        }
    }
}
