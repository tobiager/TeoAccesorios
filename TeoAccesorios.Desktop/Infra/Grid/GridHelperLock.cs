using System;
using System.Windows.Forms;
using TeoAccesorios.Desktop;

namespace TeoAccesorios.Desktop
{
    public static class GridHelperLock
    {
        /// <summary>
        /// Método unificado: pone la grilla en solo lectura y deja fijado un handler
        /// para que el bloqueo se reaplique automáticamente tras cada DataBinding.
        /// </summary>
        public static void Apply(DataGridView g)
            => Bloquear(g);

        /// <summary>
        /// Aplica el modo de solo lectura y se asegura de que se mantenga
        /// incluso si el origen de datos de la grilla cambia.
        /// </summary>
        public static void Bloquear(DataGridView g)
        {
            if (g == null) return;
            GridHelper.SoloLectura(g, true);
            // Asegura que el bloqueo se reaplique después de enlazar datos o agregar columnas.
            // Se remueve antes de agregar para evitar suscripciones múltiples.
            g.DataBindingComplete -= ReapplyLockHandler;
            g.DataBindingComplete += ReapplyLockHandler;
            g.ColumnAdded -= ReapplyLockHandler;
            g.ColumnAdded += ReapplyLockHandler;
        }

        // === Wrappers de compatibilidad (NO tocar Forms) ===

        /// <summary>Alias de la firma antigua usada en varios Forms para mantener compatibilidad.</summary>
        public static void WireDataBindingLock(DataGridView g) => Bloquear(g);

        /// <summary>Alias para llamadas antiguas que iban a GridHelperLock.SoloLectura(...) para mantener compatibilidad.</summary>
        public static void SoloLectura(DataGridView g, bool bloquearTamano = true) => GridHelper.SoloLectura(g, bloquearTamano);

        private static void ReapplyLockHandler(object? sender, EventArgs e)
        {
            if (sender is DataGridView g)
            {
                GridHelper.SoloLectura(g, bloquearTamano: true);
            }
        }
    }
}
