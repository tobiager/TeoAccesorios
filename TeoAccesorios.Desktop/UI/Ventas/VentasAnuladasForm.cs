using System.Linq;
using System.Globalization;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class VentasAnuladasForm : InactivosBaseForm<Venta>
    {
        public VentasAnuladasForm() : base(
            titulo: "Ventas anuladas",
            loader: () => Repository.ListarVentas(true).Where(v => v.Anulada).OrderByDescending(v => v.FechaVenta),
            restaurarAction: (venta) => Repository.SetVentaAnulada(venta.Id, false),
            configurarGrid: ConfigurarGrid)
        {
            // El constructor de la clase base se encarga de toda la configuración.
            // El botón "Restaurar" de la base ejecutará la acción de "Reabrir".
        }
        
        /// <summary>
        /// Define las columnas específicas para la grilla de ventas anuladas.
        /// </summary>
        private static void ConfigurarGrid(DataGridView grid)
        {
            GridHelper.AddText(grid, "Id", "Id", width: 60);
            GridHelper.AddText(grid, "Fecha", "FechaVenta", width: 140, format: "dd/MM/yyyy HH:mm");
            GridHelper.AddText(grid, "Cliente", "ClienteNombre", fill: true);
            GridHelper.AddText(grid, "Vendedor", "Vendedor", width: 180);
            // Opción A (preferida): Usar AddNumber con formato de moneda AR.
            GridHelper.AddNumber(grid, "Total", "Total", width: 120, format: "C0", culture: new CultureInfo("es-AR"));
        }
    }
}