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
                  (producto) =>
                  {
                      // Verificar conflicto por nombre con productos activos (comparación de strings, case-insensitive)
                      var nombre = (producto.Nombre ?? "").Trim();
                      var conflicto = Repository.ListarProductos(false)
                                    .Select(p => (p.Nombre ?? "").Trim())
                                    .Any(n => string.Equals(n, nombre, System.StringComparison.OrdinalIgnoreCase));

                      if (conflicto)
                      {
                          MessageBox.Show(
                              $"No se puede restaurar \"{producto.Nombre}\" porque ya existe un producto activo con el mismo nombre.",
                              "Restauración cancelada",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Warning);
                          return;
                      }

                      var resp = MessageBox.Show(
                          $"¿Confirmás restaurar el producto \"{producto.Nombre}\"?",
                          "Confirmar restauración",
                          MessageBoxButtons.YesNo,
                          MessageBoxIcon.Question);

                      if (resp == DialogResult.Yes)
                          Repository.RestaurarProducto(producto.Id);
                  },
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