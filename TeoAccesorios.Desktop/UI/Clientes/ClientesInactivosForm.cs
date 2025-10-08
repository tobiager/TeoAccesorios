using System.Linq;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class ClientesInactivosForm : InactivosBaseForm<Cliente>
    {
        public ClientesInactivosForm()
            : base("Clientes inactivos",
                  () => Repository.ListarClientes(true).Where(c => !c.Activo),
                  (cliente) => Repository.RestaurarCliente(cliente.Id),
                  (grid) =>
                  {
                      GridHelper.AddText(grid, "Id", "Id", width: 70);
                      GridHelper.AddText(grid, "Nombre", "Nombre", width: 250);
                      GridHelper.AddText(grid, "Email", "Email", width: 250);
                      GridHelper.AddText(grid, "Teléfono", "Telefono", fill: true);
                  })
        {
        }
    }
}