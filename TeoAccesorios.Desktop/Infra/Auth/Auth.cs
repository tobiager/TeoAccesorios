using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace TeoAccesorios.Desktop
{
    public enum RolUsuario { Gerente, Admin, Vendedor }

    public static class Sesion
    {
        public static string Usuario { get; set; } = "Invitado";
        public static int? UsuarioId { get; set; }
        public static RolUsuario Rol { get; set; } = RolUsuario.Vendedor;
    }

    public static class AuthService
    {
        public static bool Login(string usuario, string password, out RolUsuario rol)
        {
            rol = RolUsuario.Vendedor;

            using var cn = new SqlConnection(Db.ConnectionString);
            using var cmd = new SqlCommand(@"
                SELECT TOP 1 Id, rol, activo
                FROM dbo.Usuarios
                WHERE nombreUsuario = @u AND contrasenia = @p;", cn);

            cmd.Parameters.AddWithValue("@u", usuario);
            cmd.Parameters.AddWithValue("@p", password);

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

            // Guardar datos en la sesión
            Sesion.Usuario = usuario;
            Sesion.UsuarioId = id;
            Sesion.Rol = rol;
            return true;
        }
    }
}
