namespace TeoAccesorios.Desktop.Models
{
public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string Direccion { get; set; } = "";
    public int? LocalidadId { get; set; }
    public string LocalidadNombre { get; set; } = "";
    public string ProvinciaNombre { get; set; } = "";
    public bool Activo { get; set; }
}
}
