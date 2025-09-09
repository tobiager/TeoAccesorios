namespace TeoAccesorios.Desktop.Models
{
    public class Subcategoria
    {
        public int Id { get; set; }

        // 👉 esta faltaba según el error
        public string Nombre { get; set; } = "";

        public string? Descripcion { get; set; }

        public int CategoriaId { get; set; }
        public string CategoriaNombre { get; set; } = "";

        public bool Activo { get; set; } = true;
    }
}
