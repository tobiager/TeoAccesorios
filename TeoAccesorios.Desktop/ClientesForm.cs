using System.Linq;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class ClientesForm : Form
    {
        private CheckBox chkInactivos = new CheckBox{ Text="Ver inactivos" };
        private BindingSource bs = new BindingSource();
        private DataGridView grid = new DataGridView{ Dock=DockStyle.Fill, ReadOnly=true, AutoGenerateColumns=true };
        public ClientesForm()
        {
            Text = "Clientes";
            Width = 800; Height = 500; StartPosition = FormStartPosition.CenterParent;

            var top = new FlowLayoutPanel{ Dock=DockStyle.Top, Height=44, Padding = new Padding(8) };
            var btnNuevo = new Button{ Text="Nuevo"};
            var btnEditar = new Button{ Text="Editar"};
            var btnEliminar = new Button{ Text="Eliminar"};
            var btnRestaurar = new Button{ Text="Restaurar"};
            
            btnNuevo.Click += (_,__) => { var cli = new Cliente(); using var f = new ClienteEditForm(cli); if(f.ShowDialog(this)==DialogResult.OK){ cli.Id = (MockData.Clientes.LastOrDefault()?.Id ?? 0)+1; MockData.Clientes.Add(cli); LoadData();
            btnEliminar.Click += (_,__) => { if(grid.CurrentRow?.DataBoundItem is Cliente sel){ sel.Activo=false; LoadData(); } };
            btnRestaurar.Click += (_,__) => { if(grid.CurrentRow?.DataBoundItem is Cliente sel){ sel.Activo=true; LoadData(); } };
            chkInactivos.CheckedChanged += (_,__) => LoadData(); } };
            btnEditar.Click += (_,__) => {
                if(grid.CurrentRow?.DataBoundItem is Cliente sel){ var tmp = new Cliente{ Id=sel.Id, Nombre=sel.Nombre, Email=sel.Email, Telefono=sel.Telefono, Direccion=sel.Direccion };
                    using var f = new ClienteEditForm(tmp); if(f.ShowDialog(this)==DialogResult.OK){ sel.Nombre=tmp.Nombre; sel.Email=tmp.Email; sel.Telefono=tmp.Telefono; sel.Direccion=tmp.Direccion; LoadData(); } }
            };
            top.Controls.Add(btnNuevo); top.Controls.Add(btnEditar); top.Controls.Add(btnEliminar); top.Controls.Add(btnRestaurar); top.Controls.Add(chkInactivos);

            Controls.Add(grid);
            Controls.Add(top);
            LoadData();
        }
        private void LoadData()
        {
            var list = MockData.Clientes.Where(c => chkInactivos.Checked ? true : c.Activo).ToList(); bs.DataSource = list;
            grid.DataSource = bs;
        }
    }
}
