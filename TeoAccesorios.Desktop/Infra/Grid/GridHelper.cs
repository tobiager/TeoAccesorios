using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TeoAccesorios.Desktop
{
    public static class GridHelper
    {
        // Guarda el estado de orden para cada DataGridView (no provoca memory leak gracias a ConditionalWeakTable)
        private class SortState { public string Property = null!; public SortOrder Order = SortOrder.None; }
        private static readonly ConditionalWeakTable<DataGridView, SortState> _sortStates = new();

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

            // Wire global de orden/triángulo blanco para que se aplique en toda la app
            WireWhiteSortGlyph(g);
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

        // ------------------ Ordenación con glifo blanco ------------------

        /// <summary>
        /// Activa el manejo de ordenamiento con glifo blanco en el header y preserva estado entre rebinds.
        /// Llamar tras Estilizar para que el aspecto quede consistente.
        /// </summary>
        public static void WireWhiteSortGlyph(DataGridView g)
        {
            if (g == null) return;

            // evitar suscripciones múltiples
            g.ColumnAdded -= Grid_ColumnAdded_SetProgrammatic;
            g.ColumnAdded += Grid_ColumnAdded_SetProgrammatic;

            g.DataBindingComplete -= Grid_DataBindingComplete_ApplyState;
            g.DataBindingComplete += Grid_DataBindingComplete_ApplyState;

            g.ColumnHeaderMouseClick -= Grid_ColumnHeaderMouseClick_SortAndMark;
            g.ColumnHeaderMouseClick += Grid_ColumnHeaderMouseClick_SortAndMark;

            g.CellPainting -= Grid_CellPainting_DrawGlyph;
            g.CellPainting += Grid_CellPainting_DrawGlyph;

            // asegurar columnas actuales como programmatic
            foreach (DataGridViewColumn c in g.Columns)
            {
                if (c.SortMode != DataGridViewColumnSortMode.NotSortable)
                    c.SortMode = DataGridViewColumnSortMode.Programmatic;
            }
        }

        private static void Grid_ColumnAdded_SetProgrammatic(object? sender, DataGridViewColumnEventArgs e)
        {
            if (sender is DataGridView g)
            {
                if (e.Column.SortMode != DataGridViewColumnSortMode.NotSortable)
                    e.Column.SortMode = DataGridViewColumnSortMode.Programmatic;

                // reaplicar estado de orden si existe para la nueva columna
                if (_sortStates.TryGetValue(g, out var st) && st.Order != SortOrder.None && !string.IsNullOrEmpty(st.Property))
                {
                    if (string.Equals(e.Column.DataPropertyName, st.Property, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(e.Column.Name, st.Property, StringComparison.OrdinalIgnoreCase))
                    {
                        e.Column.HeaderCell.Tag = st.Order;
                    }
                }
            }
        }

        private static void Grid_DataBindingComplete_ApplyState(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (sender is DataGridView g)
            {
                // marcar todas las columnas como programmatic
                foreach (DataGridViewColumn c in g.Columns)
                {
                    if (c.SortMode != DataGridViewColumnSortMode.NotSortable)
                        c.SortMode = DataGridViewColumnSortMode.Programmatic;
                    c.HeaderCell.Tag = SortOrder.None;
                }

                // si hay estado guardado, aplicarlo a la columna que corresponda
                if (_sortStates.TryGetValue(g, out var st) && st.Order != SortOrder.None && !string.IsNullOrEmpty(st.Property))
                {
                    var active = g.Columns.Cast<DataGridViewColumn>()
                        .FirstOrDefault(c => string.Equals(c.DataPropertyName, st.Property, StringComparison.OrdinalIgnoreCase)
                                          || string.Equals(c.Name, st.Property, StringComparison.OrdinalIgnoreCase));
                    if (active != null)
                    {
                        active.HeaderCell.Tag = st.Order;
                    }
                }
            }
        }

        private static void Grid_ColumnHeaderMouseClick_SortAndMark(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (!(sender is DataGridView g)) return;
            if (e.ColumnIndex < 0) return;

            var col = g.Columns[e.ColumnIndex];
            var current = col.HeaderCell.Tag is SortOrder t ? t : SortOrder.None;
            var next = current == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;

            var prop = string.IsNullOrEmpty(col.DataPropertyName) ? col.Name : col.DataPropertyName;

            // Determinar la fuente subyacente
            object? underlying = null;
            var bs = g.DataSource as BindingSource;
            if (bs != null)
                underlying = bs.DataSource;
            else
                underlying = g.DataSource;

            bool handled = false;

            // intentar BindingSource.Sort si underlying lo soporta
            if (underlying is IBindingList || underlying is IBindingListView)
            {
                if (bs != null)
                {
                    try
                    {
                        bs.Sort = $"{prop} {(next == SortOrder.Ascending ? "ASC" : "DESC")}";
                        handled = true;
                    }
                    catch
                    {
                        // fallback
                    }
                }
            }

            // si no, intentar ordenar enumerable y reasignar BindingSource
            if (!handled && underlying is IEnumerable enumerable)
            {
                var list = enumerable.Cast<object>().ToList();
                if (list.Count > 0)
                {
                    var itemType = list[0].GetType();
                    var pi = itemType.GetProperty(prop);
                    if (pi != null)
                    {
                        var sorted = (next == SortOrder.Ascending)
                            ? list.OrderBy(x => pi.GetValue(x, null)).ToList()
                            : list.OrderByDescending(x => pi.GetValue(x, null)).ToList();

                        g.DataSource = new BindingSource { DataSource = sorted };
                        handled = true;
                    }
                }
            }

            // fallback: DataGridView.Sort (para grids no enlazados)
            if (!handled)
            {
                try
                {
                    g.Sort(col, next == SortOrder.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending);
                    handled = true;
                }
                catch
                {
                    // no hacer nada
                }
            }

            // localizar la columna actual en la colección (puede haberse regenerado)
            DataGridViewColumn? activeColumn = g.Columns.Cast<DataGridViewColumn>()
                .FirstOrDefault(c => string.Equals(c.DataPropertyName, prop, StringComparison.OrdinalIgnoreCase)
                                  || string.Equals(c.Name, prop, StringComparison.OrdinalIgnoreCase)
                                  || c.Index == e.ColumnIndex);

            // resetear tags y setear el de la columna activa
            foreach (DataGridViewColumn c in g.Columns)
                c.HeaderCell.Tag = (c == activeColumn) ? next : SortOrder.None;

            // guardar estado en la tabla para rebind futuros
            var state = _sortStates.GetOrCreateValue(g);
            state.Property = prop;
            state.Order = next;

            g.Invalidate(); // repintar headers
        }

        private static void Grid_CellPainting_DrawGlyph(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (!(sender is DataGridView g)) return;

            if (e.RowIndex == -1 && e.ColumnIndex >= 0)
            {
                e.Paint(e.ClipBounds, DataGridViewPaintParts.All);

                var col = g.Columns[e.ColumnIndex];
                var order = col.HeaderCell.Tag is SortOrder ord ? ord : SortOrder.None;
                if (order != SortOrder.None)
                {
                    DrawWhiteSortTriangle(e.Graphics!, e.CellBounds, order);
                }

                e.Handled = true;
            }
        }

        private static void DrawWhiteSortTriangle(Graphics gfx, Rectangle cell, SortOrder order)
        {
            int w = 10, h = 6;
            int paddingRight = 10;
            int centerX = cell.Right - paddingRight - w / 2;
            int centerY = cell.Top + cell.Height / 2;

            Point[] pts = (order == SortOrder.Ascending)
                ? new[] {
                    new Point(centerX - w/2, centerY + h/2),
                    new Point(centerX + w/2, centerY + h/2),
                    new Point(centerX,       centerY - h/2),
                }
                : new[] {
                    new Point(centerX - w/2, centerY - h/2),
                    new Point(centerX + w/2, centerY - h/2),
                    new Point(centerX,       centerY + h/2),
                };

            using (var brush = new SolidBrush(Color.White))
                gfx.FillPolygon(brush, pts);
        }
    }
}