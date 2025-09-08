using System.Windows.Forms;

namespace TeoAccesorios.Desktop
{
    public class CategoriasForm : Form
    {
        public CategoriasForm()
        {
            Text = "Categorías";
            Width = 700; Height = 500; StartPosition = FormStartPosition.CenterParent;

            var grid = new DataGridView{ Dock=DockStyle.Fill, ReadOnly=true, AutoGenerateColumns=true, DataSource = Repository.ListarCategorias() };
            Controls.Add(grid);
        }
    }
}
