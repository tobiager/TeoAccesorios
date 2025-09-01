using System.Linq;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class UsuariosForm : Form
    {
        private BindingSource bs = new BindingSource();
        private DataGridView grid = new DataGridView{ Dock=DockStyle.Fill, ReadOnly=true, AutoGenerateColumns=true };
        public UsuariosForm()
        {
            Text = "Usuarios (Vendedores / Admin)";
            Width = 700; Height = 480; StartPosition = FormStartPosition.CenterParent;

            var top = new FlowLayoutPanel{ Dock=DockStyle.Top, Height=44, Padding = new Padding(8) };
            var btnNuevo = new Button{ Text="Nuevo"};
            var btnEditar = new Button{ Text="Editar"};
            btnNuevo.Click += (_,__) => {
                var u = new Usuario();
                using var f = new UsuarioEditForm(u);
                if(f.ShowDialog(this)==DialogResult.OK){ u.Id=(MockData.Usuarios.LastOrDefault()?.Id??0)+1; MockData.Usuarios.Add(u); LoadData(); }
            };
            btnEditar.Click += (_,__) => {
                if(grid.CurrentRow?.DataBoundItem is Usuario sel){
                    var tmp = new Usuario{ Id=sel.Id, NombreUsuario=sel.NombreUsuario, Rol=sel.Rol, Activo=sel.Activo };
                    using var f = new UsuarioEditForm(tmp);
                    if(f.ShowDialog(this)==DialogResult.OK){ sel.NombreUsuario=tmp.NombreUsuario; sel.Rol=tmp.Rol; sel.Activo=tmp.Activo; LoadData(); }
                }
            };
            top.Controls.Add(btnNuevo); top.Controls.Add(btnEditar);
            Controls.Add(grid); Controls.Add(top);
            LoadData();
        }
        private void LoadData(){ bs.DataSource = MockData.Usuarios.ToList(); grid.DataSource = bs; }
    }
}
