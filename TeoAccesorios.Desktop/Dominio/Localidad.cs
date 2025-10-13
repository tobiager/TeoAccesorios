namespace TeoAccesorios.Desktop.Models
{
    public class Localidad
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public int ProvinciaId { get; set; }
        // Conveniencia (solo lectura) para joins
        public string ProvinciaNombre { get; set; } = "";
        public bool? Activo { get; set; }
    }
}