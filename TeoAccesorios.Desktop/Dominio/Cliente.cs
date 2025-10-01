public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? Localidad { get; set; }   
    public string? Provincia { get; set; }  
    public bool Activo { get; set; } = true; 
}
