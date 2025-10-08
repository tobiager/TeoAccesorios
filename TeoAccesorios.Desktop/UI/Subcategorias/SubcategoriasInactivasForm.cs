using System.Linq;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class SubcategoriasInactivasForm : InactivosBaseForm<Subcategoria>
    {
        public SubcategoriasInactivasForm()
            : base("Subcategorías inactivas",
                  () => Repository.ListarSubcategorias(null, true).Where(s => !s.Activo),
                  (subcategoria) => Repository.SetSubcategoriaActiva(subcategoria.Id, true),
                  (grid) =>
                  {
                      GridHelper.AddText(grid, "Id", "Id", 80);
                      GridHelper.AddText(grid, "Nombre", "Nombre", 250);
                      GridHelper.AddText(grid, "Categoría", "CategoriaNombre", 250);
                  })
        {
        }
    }
}