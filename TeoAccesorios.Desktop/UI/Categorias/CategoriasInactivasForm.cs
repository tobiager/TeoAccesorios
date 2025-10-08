using System.Linq;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class CategoriasInactivasForm : InactivosBaseForm<Categoria>
    {
        public CategoriasInactivasForm()
            : base("Categorías inactivas",
                  () => Repository.ListarCategorias(true).Where(c => !c.Activo),
                  (categoria) => Repository.SetCategoriaActiva(categoria.Id, true),
                  (grid) =>
                  {
                      GridHelper.AddText(grid, "Id", "Id", width: 70);
                      GridHelper.AddText(grid, "Nombre", "Nombre", width: 250);
                      GridHelper.AddText(grid, "Descripción", "Descripcion", fill: true);
                  })
        {
        }
    }
}