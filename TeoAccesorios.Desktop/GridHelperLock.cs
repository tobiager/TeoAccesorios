using System.Windows.Forms;

namespace TeoAccesorios.Desktop
{
    public static class GridHelperLock
    {
        public static void SoloLectura(DataGridView g)
        {
            g.ReadOnly = true;                                  
            g.EditMode = DataGridViewEditMode.EditProgrammatically;

            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            g.AllowUserToResizeColumns = false;
            g.AllowUserToResizeRows = false;
            g.AllowUserToOrderColumns = false;

            g.MultiSelect = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.RowHeadersVisible = false;

            foreach (DataGridViewColumn c in g.Columns)
                c.ReadOnly = true; 
        }

        // Llama una sola vez por grilla 
        public static void WireDataBindingLock(DataGridView g)
        {
            // Cuando se vuelve a bindear, re-aplica solo-lectura
            g.DataBindingComplete += (_, __) => SoloLectura(g);

            // Bloquea cualquier intento de edición (incluye checkboxes)
            g.CellBeginEdit += (s, e) => { e.Cancel = true; };

            // Si alguna celda entra en estado "dirty", cancela edición
            g.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (g.IsCurrentCellDirty) g.CancelEdit();
            };
        }
    }
}
