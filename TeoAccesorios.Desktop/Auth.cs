namespace TeoAccesorios.Desktop
{
    public enum RolUsuario
    {
        Admin,
        Vendedor
    }

    public static class Sesion
    {
        public static string Usuario { get; set; } = "Invitado";
        public static RolUsuario Rol { get; set; } = RolUsuario.Vendedor;
    }

    // Mock auth para demo: acepta cualquier usuario, pero si el usuario contiene 'admin' entra como Admin.
    public static class AuthService
    {
        public static bool Login(string usuario, string password, out RolUsuario rol)
        {
            // Busca por nombre en Usuarios; si contiene 'admin' o coincide con un usuario Admin, asigna Admin
rol = usuario.Trim().ToLower().Contains("admin") || MockData.Usuarios.Exists(u => u.NombreUsuario.Equals(usuario.Trim(), System.StringComparison.OrdinalIgnoreCase) && u.Rol=="Admin")
    ? RolUsuario.Admin : RolUsuario.Vendedor;
            return !string.IsNullOrWhiteSpace(usuario);
        }
    }
}
