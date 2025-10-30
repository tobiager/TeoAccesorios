using System;
using System.Drawing;
using System.Linq;
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

            // Centraliza el bloqueo de edición y tamaño
            SoloLectura(g, bloquearTamano: true);
        }

        /// <summary>
        /// Configura la grilla en modo solo lectura, deshabilita la edición, el redimensionamiento
        /// y blinda la grilla contra intentos de modificación por parte del usuario.
        /// </summary>
        /// <param name="g">La DataGridView a bloquear.</param>
        /// <param name="bloquearTamano">Si es true, también deshabilita el redimensionamiento de columnas y filas.</param>
        public static void SoloLectura(DataGridView g, bool bloquearTamano = true)
        {
            if (g == null) return;

            // Propiedades base de solo lectura
            g.ReadOnly = true;
            g.EditMode = DataGridViewEditMode.EditProgrammatically;
            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            g.AllowUserToOrderColumns = false;

            // Selección de fila completa
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.MultiSelect = false;

            // Bloqueo de tamaño 
            if (bloquearTamano)
            {
                g.AllowUserToResizeColumns = false;
                g.AllowUserToResizeRows = false;
                g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                g.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            }

            // Aplicar a columnas existentes
            foreach (DataGridViewColumn c in g.Columns)
            {
                c.ReadOnly = true;
                if (bloquearTamano)
                {
                    c.Resizable = DataGridViewTriState.False;
                }
            }

            // Blindaje contra eventos de edición (evitando suscripciones múltiples)
            g.CellBeginEdit -= CancelEditHandler;
            g.CellBeginEdit += CancelEditHandler;

            g.UserDeletingRow -= CancelDeleteHandler;
            g.UserDeletingRow += CancelDeleteHandler;

            g.KeyDown -= SuppressEditKeysHandler;
            g.KeyDown += SuppressEditKeysHandler;

            // Asegurar que las columnas agregadas dinámicamente también se bloqueen
            g.ColumnAdded -= ColumnAddedHandler;
            g.ColumnAdded += ColumnAddedHandler;
        }

        // Handlers para el blindaje (static para poder removerlos sin instancia)
        private static void CancelEditHandler(object sender, DataGridViewCellCancelEventArgs e) => e.Cancel = true;
        private static void CancelDeleteHandler(object sender, DataGridViewRowCancelEventArgs e) => e.Cancel = true;
        private static void SuppressEditKeysHandler(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back || e.KeyCode == Keys.F2)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }
        private static void ColumnAddedHandler(object sender, DataGridViewColumnEventArgs e)
        {
            e.Column.ReadOnly = true;
            // Asumimos que si se bloquea, se bloquea todo. El flag `bloquearTamano` se evalúa en el `sender`.
            if (sender is DataGridView dgv && !dgv.AllowUserToResizeColumns)
            {
                e.Column.Resizable = DataGridViewTriState.False;
            }
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

            if (width.HasValue)
            {
                col.Width = width.Value;
            }
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