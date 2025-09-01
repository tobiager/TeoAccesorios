using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class ClienteEditForm : Form
    {
        private TextBox txtNombre = new TextBox();
        private TextBox txtEmail = new TextBox();
        private TextBox txtTel = new TextBox();
        private TextBox txtDir = new TextBox();
        private TextBox txtLoc = new TextBox();
        private TextBox txtProv = new TextBox();
        private CheckBox chkActivo = new CheckBox{ Text="Activo" };
        private Cliente model;

        public ClienteEditForm(Cliente c)
        {
            model = c;
            Text = "Cliente"; Width=520; Height=300; StartPosition=FormStartPosition.CenterParent;
            var grid = new TableLayoutPanel{ Dock=DockStyle.Fill, ColumnCount=2, Padding=new Padding(12) };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120)); grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            grid.Controls.Add(new Label{Text="Nombre"},0,0); grid.Controls.Add(txtNombre,1,0);
            grid.Controls.Add(new Label{Text="Email"},0,1); grid.Controls.Add(txtEmail,1,1);
            grid.Controls.Add(new Label{Text="Teléfono"},0,2); grid.Controls.Add(txtTel,1,2);
            grid.Controls.Add(new Label{Text="Dirección"},0,3); grid.Controls.Add(txtDir,1,3);
            grid.Controls.Add(new Label{Text="Localidad"},0,4); grid.Controls.Add(txtLoc,1,4);
            grid.Controls.Add(new Label{Text="Provincia"},0,5); grid.Controls.Add(txtProv,1,5);
            var ok = new Button{ Text="Guardar", Dock=DockStyle.Bottom, Height=36 }; var extra = new FlowLayoutPanel{ Dock=DockStyle.Bottom, Height=34 }; extra.Controls.Add(chkActivo); Controls.Add(extra);
            ok.Click += (_,__) => { model.Nombre=txtNombre.Text; model.Email=txtEmail.Text; model.Telefono=txtTel.Text; model.Direccion=txtDir.Text; model.Localidad=txtLoc.Text; model.Provincia=txtProv.Text; model.Activo=chkActivo.Checked; DialogResult=DialogResult.OK; Close(); };
            Controls.Add(ok); Controls.Add(grid);
            txtNombre.Text=c.Nombre; txtEmail.Text=c.Email; txtTel.Text=c.Telefono; txtDir.Text=c.Direccion; txtLoc.Text=c.Localidad; txtProv.Text=c.Provincia; chkActivo.Checked=c.Activo;
        }
    }
}
