using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace TeoAccesorios.Desktop
{
    public static class GridHelper
    {
     

        
        /// Aplica un estilo visual consistente y legible a la grilla.
       
        public static void Estilizar(DataGridView g)
        {
            if (g == null) return;

            g.EnableHeadersVisualStyles = false;
            g.RowHeadersVisible = false;
            g.MultiSelect = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            g.BackgroundColor = Color.White;
            g.BorderStyle = BorderStyle.None;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.GridColor = Color.FromArgb(226, 232, 240);

            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235); // #2563EB
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            g.ColumnHeadersHeight = 38;

            g.DefaultCellStyle.Font = new Font("Segoe UI", 12F);
            g.DefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 242, 254);
            g.DefaultCellStyle.SelectionForeColor = Color.FromArgb(2, 102, 159);

            g.RowTemplate.Height = 34;
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

            // Habilita doble buffer para un scroll más suave
            typeof(DataGridView).InvokeMember(
                "DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null, g, new object[] { true });
        }

        
        /// Ajusta columnas específicas para que ocupen más espacio.
        
        public static void WideColumns(DataGridView grid, params string[] columnNames)
        {
            foreach (string name in columnNames)
            {
                if (grid.Columns[name] != null)
                {
                    grid.Columns[name].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
            }
        }

       

       
        /// Agrega una columna de texto estándar a la grilla.
        /// Mantiene el estilo global aplicado por Estilizar, no cambia estética.
     
        public static DataGridViewTextBoxColumn AddText(
            DataGridView grid,
            string header,
            string dataProperty,
            int? width = null,
            bool fill = false,
            string format = null,
            DataGridViewContentAlignment? alignment = null)
        {
            var col = new DataGridViewTextBoxColumn
            {
                HeaderText = header,
                DataPropertyName = dataProperty,
                ReadOnly = true
            };

            if (width.HasValue) col.Width = width.Value;
            if (fill) col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            if (!string.IsNullOrWhiteSpace(format)) col.DefaultCellStyle.Format = format;
            if (alignment.HasValue) col.DefaultCellStyle.Alignment = alignment.Value;

            grid.Columns.Add(col);
            return col;
        }

        
        /// Atajo: agrega columna de texto que llena espacio restante.
   
        public static DataGridViewTextBoxColumn AddTextFill(
            DataGridView grid,
            string header,
            string dataProperty,
            string format = null)
        {
            return AddText(grid, header, dataProperty, width: null, fill: true, format: format);
        }

    
        /// Columna numérica alineada a la derecha (int/decimal/moneda).
    
        public static DataGridViewTextBoxColumn AddNumber(
            DataGridView grid,
            string header,
            string dataProperty,
            int? width = 100,
            string format = null,
            System.Globalization.CultureInfo culture = null)
        {
            var col = AddText(
                grid,
                header,
                dataProperty,
                width: width,
                fill: false,
                format: format,
                alignment: DataGridViewContentAlignment.MiddleRight
            );
            if (culture != null)
            {
                col.DefaultCellStyle.FormatProvider = culture;
            }
            return col;
        }

        
        public static DataGridViewCheckBoxColumn AddCheck(
            DataGridView grid,
            string header,
            string dataProperty,
            int? width = 60)
        {
            var col = new DataGridViewCheckBoxColumn
            {
                HeaderText = header,
                DataPropertyName = dataProperty,
                ReadOnly = true,
                ThreeState = false
            };
            if (width.HasValue) col.Width = width.Value;
            grid.Columns.Add(col);
            return col;
        }
    }
}