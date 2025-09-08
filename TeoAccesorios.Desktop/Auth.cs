using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace TeoAccesorios.Desktop
{
    public enum RolUsuario { Admin, Vendedor }

    public static class Sesion
    {
        public static string Usuario { get; set; } = "Invitado";
        public static RolUsuario Rol { get; set; } = RolUsuario.Vendedor;
    }

    public static class AuthService
    {
        public static bool Login(string usuario, string password, out RolUsuario rol)
        {
            rol = RolUsuario.Vendedor;

            using var cn = new SqlConnection(Db.ConnectionString);
            using var cmd = new SqlCommand(@"
                SELECT TOP 1 rol, activo
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
            rol = r.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                ? RolUsuario.Admin
                : RolUsuario.Vendedor;

            Sesion.Usuario = usuario;
            Sesion.Rol = rol;
            return true;
        }
    }
}
