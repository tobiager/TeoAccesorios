using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class UsuarioEditForm : Form
    {
        private TextBox txtUser = new TextBox();
        private ComboBox cboRol = new ComboBox{ DropDownStyle=ComboBoxStyle.DropDownList };
        private CheckBox chkActivo = new CheckBox{ Text="Activo" };
        private Usuario model;
        public UsuarioEditForm(Usuario u)
        {
            model=u;
            Text="Usuario"; Width=420; Height=240; StartPosition=FormStartPosition.CenterParent;
            cboRol.Items.AddRange(new object[]{"Admin","Vendedor"}); cboRol.SelectedIndex=1;
            var grid = new TableLayoutPanel{ Dock=DockStyle.Fill, ColumnCount=2, Padding=new Padding(12)};
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120)); grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            grid.Controls.Add(new Label{Text="Usuario"},0,0); grid.Controls.Add(txtUser,1,0);
            grid.Controls.Add(new Label{Text="Rol"},0,1); grid.Controls.Add(cboRol,1,1);
            grid.Controls.Add(chkActivo,1,2);
            var ok = new Button{ Text="Guardar", Dock=DockStyle.Bottom, Height=36 };
            ok.Click += (_,__) => { model.NombreUsuario=txtUser.Text; model.Rol=cboRol.SelectedItem!.ToString()!; model.Activo=chkActivo.Checked; DialogResult=DialogResult.OK; Close(); };
            Controls.Add(ok); Controls.Add(grid);
            txtUser.Text=u.NombreUsuario; cboRol.SelectedItem=u.Rol; chkActivo.Checked=u.Activo;
        }
    }
}
