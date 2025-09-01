namespace TeoAccesorios.Desktop.Models;
public class Cliente
{
    public bool Activo { get; set; } = true;
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? Localidad { get; set; }
    public string? Provincia { get; set; }
}
