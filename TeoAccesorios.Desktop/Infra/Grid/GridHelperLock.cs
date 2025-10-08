using System;
using System.Windows.Forms;

namespace TeoAccesorios.Desktop
{
    public static class GridHelperLock
    {
      
        /// Método unificado: pone la grilla en solo lectura y deja fijados
        /// los handlers para que el lock se re-aplique tras cada databind.
       
        public static void Apply(DataGridView g)
        {
            if (g == null) return;
            SoloLectura(g);
            WireDataBindingLock(g);
        }

     
        /// Configura la grilla en solo lectura y bloquea cambios de usuario.
        
        public static void SoloLectura(DataGridView g)
        {
            if (g == null) return;

            g.ReadOnly = true;
            g.EditMode = DataGridViewEditMode.EditProgrammatically;

            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            g.AllowUserToOrderColumns = false;
            g.AllowUserToResizeColumns = false;
            g.AllowUserToResizeRows = false;
            g.RowHeadersVisible = false;

            g.MultiSelect = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            foreach (DataGridViewColumn c in g.Columns)
                c.ReadOnly = true;
        }

        
        /// Suscribe handlers (evitando duplicados) para mantener el lock.
 
        public static void WireDataBindingLock(DataGridView g)
        {
            if (g == null) return;

            // evitar suscripciones duplicadas
            g.DataBindingComplete -= OnDataBindingComplete;
            g.DataBindingComplete += OnDataBindingComplete;

            g.CellBeginEdit -= CancelCellBeginEdit;
            g.CellBeginEdit += CancelCellBeginEdit;

            g.CurrentCellDirtyStateChanged -= CancelDirtyEdit;
            g.CurrentCellDirtyStateChanged += CancelDirtyEdit;
        }

        private static void OnDataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (sender is DataGridView g) SoloLectura(g);
        }

        private static void CancelCellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            e.Cancel = true;
        }

        private static void CancelDirtyEdit(object sender, EventArgs e)
        {
            if (sender is DataGridView g && g.IsCurrentCellDirty)
                g.CancelEdit();
        }
    }
}
