using System.Linq;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class ProductosInactivosForm : InactivosBaseForm<Producto>
    {
        public ProductosInactivosForm()
            : base("Productos inactivos",
                  () => Repository.ListarProductos(true).Where(p => !p.Activo),
                  (producto) => Repository.RestaurarProducto(producto.Id),
                  (grid) =>
                  {
                      GridHelper.AddText(grid, "Id", "Id", width: 70);
                      GridHelper.AddText(grid, "Nombre", "Nombre", fill: true);
                      GridHelper.AddText(grid, "Categoría", "CategoriaNombre", width: 200);
                      GridHelper.AddText(grid, "Subcategoría", "SubcategoriaNombre", width: 200);
                  })
        {
        }
    }
}