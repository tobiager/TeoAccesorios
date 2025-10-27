namespace TeoAccesorios.Desktop.Models;
public class Usuario
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = "";
    public string Correo { get; set; } = "";
    public string Contrasenia { get; set; } = "";
    public string Rol { get; set; } = "Vendedor";
    public bool Activo { get; set; } = true;
    
    // La columna ContraseniaEstado se calcula en SQL, esta propiedad es solo placeholder
    public string ContraseniaEstado { get; set; } = "Personalizada";
}