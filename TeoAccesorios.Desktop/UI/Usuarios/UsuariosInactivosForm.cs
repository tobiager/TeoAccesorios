using System.Linq;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class UsuariosInactivosForm : InactivosBaseForm<Usuario>
    {
        public UsuariosInactivosForm() : base(
            titulo: "Usuarios inactivos",
            loader: () => Repository.ListarUsuarios().Where(u => !u.Activo),
            restaurarAction: (usuario) => Repository.SetUsuarioActivo(usuario.Id, true),
            configurarGrid: ConfigurarGrid)
        {
            // El constructor de la clase base se encarga de toda la configuración.
        }

        private static void ConfigurarGrid(DataGridView grid)
        {
            GridHelper.AddText(grid, "Id", "Id", width: 60);
            GridHelper.AddText(grid, "Nombre", "NombreUsuario", width: 180);
            GridHelper.AddText(grid, "Correo", "Correo", width: 240, fill: true);
            GridHelper.AddText(grid, "Rol", "Rol", width: 140);
        }
    }
}