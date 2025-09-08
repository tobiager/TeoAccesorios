public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? Localidad { get; set; }   // usado por ClienteEditForm
    public string? Provincia { get; set; }   // usado por ClienteEditForm
    public bool Activo { get; set; } = true; // usado por ClientesForm
}
