namespace TeoAccesorios.Desktop.Models;
public class Usuario
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string Rol { get; set; } = "Vendedor"; // Admin / Vendedor
    public bool Activo { get; set; } = true;
}
