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

            // Doble clic: abrir detalle de la venta anulada
            try
            {
                grid.CellDoubleClick += (_, e) =>
                {
                    if (e.RowIndex < 0) return;

                    var row = grid.Rows[e.RowIndex];

                    // 1) Preferimos obtener el objeto enlazado directamente
                    var venta = row.DataBoundItem as Venta;

                    // 2) Si no hay DataBoundItem (por seguridad), intentar localizar el Id en las celdas
                    if (venta == null)
                    {
                        // buscar columna cuyo DataPropertyName sea "Id"
                        var idCell = row.Cells
                                        .Cast<DataGridViewCell>()
                                        .FirstOrDefault(c => string.Equals(c.OwningColumn.DataPropertyName, "Id", System.StringComparison.OrdinalIgnoreCase));

                        // si no encontramos por DataPropertyName, tomar la primera celda como respaldo
                        var val = idCell?.Value ?? row.Cells.Cast<DataGridViewCell>().FirstOrDefault()?.Value;
                        if (val == null) return;

                        if (!int.TryParse(val.ToString(), out int id)) return;

                        venta = Repository.ListarVentas(true).FirstOrDefault(v => v.Id == id);
                        if (venta == null) return;
                    }

                    var cliente = Repository.Clientes.FirstOrDefault(c => c.Id == venta.ClienteId);
                    
                    new VentaDetalleForm(venta, cliente).ShowDialog(this);
                };
            }
            catch
            {
                // No fallar si por alguna razón la grilla no está lista aún.
            }
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
            
            GridHelper.AddNumber(grid, "Total", "Total", width: 120, format: "C0", culture: new CultureInfo("es-AR"));
        }
    }
}