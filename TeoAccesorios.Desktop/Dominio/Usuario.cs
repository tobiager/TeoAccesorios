namespace TeoAccesorios.Desktop.Models;
public class Usuario
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = "";
    public string Correo { get; set; } = "";
    public string Contrasenia { get; set; } = "";
    public string Rol { get; set; } = "Vendedor";
    public bool Activo { get; set; } = true;
    
    // Nueva propiedad calculada para mostrar en la grilla
    public string ContraseniaEstado => Contrasenia == "default123" ? "Por defecto" : "Personalizada";
}