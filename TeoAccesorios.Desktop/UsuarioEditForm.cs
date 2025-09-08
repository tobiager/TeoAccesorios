using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class UsuarioEditForm : Form
    {
        private readonly TextBox txtUser = new();
        private readonly ComboBox cboRol = new() { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly CheckBox chkActivo = new() { Text = "Activo" };

        private readonly Usuario model;
        public Usuario Result => model; // <-- para leer desde el form padre

        public UsuarioEditForm(Usuario u)
        {
            model = u;

            Text = "Usuario";
            Width = 420; Height = 240;
            StartPosition = FormStartPosition.CenterParent;

            cboRol.Items.AddRange(new object[] { "Admin", "Vendedor" });

            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(12) };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));

            grid.Controls.Add(new Label { Text = "Usuario" }, 0, 0);
            grid.Controls.Add(txtUser, 1, 0);
            grid.Controls.Add(new Label { Text = "Rol" }, 0, 1);
            grid.Controls.Add(cboRol, 1, 1);
            grid.Controls.Add(chkActivo, 1, 2);

            var ok = new Button { Text = "Guardar", Dock = DockStyle.Bottom, Height = 36 };
            ok.Click += (_, __) =>
            {
                model.NombreUsuario = txtUser.Text;
                model.Rol = (cboRol.SelectedItem?.ToString() ?? "Vendedor");
                model.Activo = chkActivo.Checked;
                DialogResult = DialogResult.OK;
                Close();
            };

            Controls.Add(ok);
            Controls.Add(grid);

            // Inicial
            txtUser.Text = u.NombreUsuario;
            cboRol.SelectedItem = string.IsNullOrWhiteSpace(u.Rol) ? "Vendedor" : u.Rol;
            chkActivo.Checked = u.Activo;
        }
    }
}
