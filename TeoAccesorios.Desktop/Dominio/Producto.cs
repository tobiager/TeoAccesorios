namespace TeoAccesorios.Desktop.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public int StockMinimo { get; set; }
        public bool Activo { get; set; } = true;

        public int CategoriaId { get; set; }
        public string CategoriaNombre { get; set; } = "";

        
        public int? SubcategoriaId { get; set; }
        public string SubcategoriaNombre { get; set; } = "";
    }
}
