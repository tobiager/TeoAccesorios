using System.Linq;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class ProductoEditForm : Form
    {
        private TextBox txtNombre = new TextBox();
        private TextBox txtDesc = new TextBox();
        private NumericUpDown numPrecio = new NumericUpDown{ Minimum=0, Maximum=100000000, DecimalPlaces=0, Increment=1000 };
        private NumericUpDown numStock = new NumericUpDown{ Minimum=0, Maximum=100000 };
        private NumericUpDown numMin = new NumericUpDown{ Minimum=0, Maximum=100000, Value=5 };
        private ComboBox cboCat = new ComboBox{ DropDownStyle=ComboBoxStyle.DropDownList };
        private Producto model;

        public ProductoEditForm(Producto p)
        {
            model = p;
            Text = "Producto"; Width=560; Height=360; StartPosition=FormStartPosition.CenterParent;
            foreach(var c in Repository.ListarCategorias()) cboCat.Items.Add($"{c.Id} - {c.Nombre}");
            if(cboCat.Items.Count>0) cboCat.SelectedIndex=0;

            var grid = new TableLayoutPanel{ Dock=DockStyle.Fill, ColumnCount=2, Padding=new Padding(12) };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140)); grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for(int i=0;i<6;i++) grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            grid.Controls.Add(new Label{Text="Nombre"},0,0); grid.Controls.Add(txtNombre,1,0);
            grid.Controls.Add(new Label{Text="Descripción"},0,1); grid.Controls.Add(txtDesc,1,1);
            grid.Controls.Add(new Label{Text="Precio"},0,2); grid.Controls.Add(numPrecio,1,2);
            grid.Controls.Add(new Label{Text="Stock"},0,3); grid.Controls.Add(numStock,1,3);
            grid.Controls.Add(new Label{Text="Stock mínimo"},0,4); grid.Controls.Add(numMin,1,4);
            grid.Controls.Add(new Label{Text="Categoría"},0,5); grid.Controls.Add(cboCat,1,5);
            var ok = new Button{ Text="Guardar", Dock=DockStyle.Bottom, Height=36 };
            ok.Click += (_,__) => {
                model.Nombre=txtNombre.Text; model.Descripcion=txtDesc.Text; model.Precio=numPrecio.Value; model.Stock=(int)numStock.Value; model.StockMinimo=(int)numMin.Value;
                var id = int.Parse(cboCat.SelectedItem!.ToString()!.Split('-')[0].Trim());
                model.CategoriaId=id; model.CategoriaNombre=cboCat.SelectedItem!.ToString()!.Split('-',2)[1].Trim();
                DialogResult=DialogResult.OK; Close();
            };
            Controls.Add(ok); Controls.Add(grid);

            // Prefill
            txtNombre.Text=p.Nombre; txtDesc.Text=p.Descripcion; numPrecio.Value = p.Precio; numStock.Value=p.Stock; numMin.Value=p.StockMinimo;
            var idx = Repository.ListarCategorias().FindIndex(c=>c.Id==p.CategoriaId);
            if(idx>=0) cboCat.SelectedIndex = idx;
        }
    }
}
