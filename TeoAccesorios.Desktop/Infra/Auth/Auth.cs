using System;
using System.Data;
using Microsoft.Data.SqlClient;
using TeoAccesorios.Desktop.Infra.Auth;

namespace TeoAccesorios.Desktop
{
    public enum RolUsuario
    {
        Vendedor,
        Gerente,
        Admin
    }

    public static class Sesion
    {
        public static string Usuario { get; set; } = "Invitado";
        public static RolUsuario Rol { get; set; } = RolUsuario.Vendedor;
        public static int UsuarioId { get; set; } = -1;

        // Nueva bandera para obligar cambio de contraseña si la contraseña en BD es la predeterminada
        public static bool MustChangePassword { get; set; } = false;
    }

    public static class AuthService
    {
        public static bool Login(string usuario, string password, out RolUsuario rol)
        {
            rol = RolUsuario.Vendedor;

            // Hashear la contraseña ingresada
            byte[] passwordHash = PasswordHelper.HashPassword(password);

            using var cn = new SqlConnection(Db.ConnectionString);
            using var cmd = new SqlCommand(@"
                SELECT TOP 1 Id, nombreUsuario, rol, activo, contrasenia
                FROM dbo.Usuarios
                WHERE nombreUsuario = @u AND contrasenia = @p;", cn);

            cmd.Parameters.AddWithValue("@u", usuario);
            cmd.Parameters.Add("@p", System.Data.SqlDbType.VarBinary, 32).Value = passwordHash;

            cn.Open();
            using var rd = cmd.ExecuteReader();
            if (!rd.Read()) return false;

            var activo = Convert.ToBoolean(rd["activo"]);
            if (!activo) return false;

            var r = (rd["rol"]?.ToString() ?? "Vendedor");
            rol = r switch
            {
                _ when r.Equals("Gerente", StringComparison.OrdinalIgnoreCase) => RolUsuario.Gerente,
                _ when r.Equals("Admin", StringComparison.OrdinalIgnoreCase) => RolUsuario.Admin,
                _ => RolUsuario.Vendedor
            };

            var id = Convert.ToInt32(rd["Id"]);
            var nombreUsuarioReal = rd["nombreUsuario"]?.ToString() ?? usuario;

            // Determinar si la contraseña almacenada es la contraseña por defecto
            Sesion.MustChangePassword = false;
            try
            {
                if (rd["contrasenia"] != DBNull.Value)
                {
                    var dbHash = rd["contrasenia"] as byte[];
                    if (dbHash != null)
                    {
                        // Si la contraseña almacenada coincide con la predeterminada, forzar cambio
                        if (PasswordHelper.VerifyPassword(dbHash, "default123"))
                        {
                            Sesion.MustChangePassword = true;
                        }
                    }
                }
            }
            catch
            {
                // En caso de error al evaluar el hash, no forzar cambio (comportamiento conservador).
                Sesion.MustChangePassword = false;
            }

            // Guardar datos en la sesión con el nombre correcto desde la BD
            Sesion.Usuario = nombreUsuarioReal;
            Sesion.UsuarioId = id;
            Sesion.Rol = rol;
            return true;
        }
    }
}