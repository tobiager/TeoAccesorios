using System.Drawing;
using System.Windows.Forms;

namespace TeoAccesorios.Desktop
{
    public static class GridHelper
    {
        public static void Estilizar(DataGridView g)
        {
            // Bordes y grilla
            g.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            g.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            g.GridColor = Color.FromArgb(203, 213, 225);
            g.BorderStyle = BorderStyle.None;
            g.RowHeadersVisible = false;

            // Ajustes generales
            g.EnableHeadersVisualStyles = false;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.MultiSelect = false;
            g.BackgroundColor = Color.White;

            // Fuente global 
            g.DefaultCellStyle.Font = new Font("Segoe UI", 11); 
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold); 

            // Encabezados
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = g.ColumnHeadersDefaultCellStyle.BackColor;
            g.ColumnHeadersDefaultCellStyle.SelectionForeColor = g.ColumnHeadersDefaultCellStyle.ForeColor;

            // Celdas
            g.DefaultCellStyle.BackColor = Color.White;
            g.DefaultCellStyle.ForeColor = Color.Black;
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(14, 165, 233);
            g.DefaultCellStyle.SelectionForeColor = Color.White;
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

            // Altura de filas 
            g.RowTemplate.Height = 32;
            g.ColumnHeadersHeight = 36;

            
            g.DataBindingComplete += (_, __) =>
            {
                try { g.CurrentCell = null; } catch { }
                g.ClearSelection();
            };
            g.SelectionChanged += (_, __) =>
            {
                if (!g.Focused) g.ClearSelection();
            };
            g.Leave += (_, __) => { g.ClearSelection(); };
        }

        public static void WideColumns(DataGridView g)
        {
            if (g == null) return;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            foreach (DataGridViewColumn c in g.Columns)
            {
                c.MinimumWidth = 60;
                if (string.Equals(c.Name, "Nombre", StringComparison.OrdinalIgnoreCase))
                    c.MinimumWidth = 160;
            }
        }

    }

}
