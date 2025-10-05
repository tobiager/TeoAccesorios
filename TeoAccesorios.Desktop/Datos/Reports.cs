using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeoAccesorios.Desktop.Datos
{
    /*
     * Clase empleada para establecer la estructura y filtros de las ventas. 
     * Los métodos aquí presentes permitirán establecer qué campos se mostrarán en los diferentes
     * gráficos de los reportes.
     */
   public static class Reports
    {
        /*
         * Permite obtener las ventas de los productos según su categoría
         * 
         * Retorno
         * 
         * Retorna una lista de objetos que contendrá la información sobre cada categoría y sus
         * montos totales de ventas.
         */
        public static List<object[]> ObtenerVentasPorCategoria(DateTime fechaInicio, DateTime fechaFin, bool ordenAscendente = false)
        {
            try
            {
                // Validar que las fechas sean válidas
                if (fechaInicio > fechaFin)
                    throw new ArgumentException("La fecha de inicio no puede ser mayor que la fecha fin");

                // Validar que no sea un rango muy amplio (opcional)
                if ((fechaFin - fechaInicio).TotalDays > 365)
                    throw new ArgumentException("El rango de fechas no puede ser mayor a 1 año");

                var ventasPorCategoria = Repository.ListarVentasPorCategoriaConFechas(fechaInicio, fechaFin, false);

                // Aplicar ordenamiento según el parámetro
                var categoriasOrdenadas = ordenAscendente
                    ? ventasPorCategoria.OrderBy(x => x.TotalVentas).ToList()
                    : ventasPorCategoria.OrderByDescending(x => x.TotalVentas).ToList();

                // Convertir a formato para Highcharts
                var datos = categoriasOrdenadas
                    .Select(v => new object[] { v.Categoria, Math.Round((double)v.TotalVentas, 2) })
                    .ToList();

                return datos;
            }
            catch (ArgumentException ex)
            {
                throw; // Relanzar excepciones de validación
            }
            catch (Exception ex)
            {
                throw new Exception("Error al recuperar las ventas por categorias en el rango de fechas especificado");
            }
        }

        /*
         * Método que permite agrupar las ventas por nombre de vendedor y establecer el monto total
         * para cada vendedor según las ventas que realizó.
         * 
         * Retorno:
         * 
         * Retorna una lista de objetos que representan el nombre de usuario del vendedor y el monto
         * total vendido.
         */


        public static List<object[]> ObtenerVentasPorVendedor(DateTime fechaInicio, DateTime fechaFin, bool ordenAscendente = false)
        {
            try
            {
                // Validar fechas
                if (fechaInicio > fechaFin)
                    throw new ArgumentException("La fecha de inicio no puede ser mayor que la fecha fin");

                var ventas = Repository.ListarVentas(false);

                // Filtrar ventas por el rango de fechas
                var ventasFiltradas = ventas
                    .Where(v => v.FechaVenta.Date >= fechaInicio.Date &&
                               v.FechaVenta.Date <= fechaFin.Date)
                    .ToList();

                // Si no hay ventas en el rango, retornar lista vacía
                if (ventasFiltradas.Count == 0)
                    return new List<object[]>();

                var ventasPorVendedor = ventasFiltradas
                    .GroupBy(v => v.Vendedor)
                    .Select(g => new
                    {
                        Vendedor = string.IsNullOrEmpty(g.Key) ? "Sin Vendedor" : g.Key,
                        TotalVentas = g.Sum(v => v.Total)
                    })
                    .ToList();

                // Aplicar ordenamiento según el parámetro
                var vendedoresOrdenados = ordenAscendente
                    ? ventasPorVendedor.OrderBy(x => x.TotalVentas).ToList()
                    : ventasPorVendedor.OrderByDescending(x => x.TotalVentas).ToList();

                var datos = vendedoresOrdenados
                    .Select(v => new object[] { v.Vendedor, Math.Round((double)v.TotalVentas, 2) })
                    .ToList();

                return datos;
            }
            catch (ArgumentException ex)
            {
                throw; // Relanzar excepciones de validación
            }
            catch (Exception ex)
            {
                throw new Exception("Error al recuperar las ventas por vendedor en el rango de fechas especificado");
            }
        }

        /*
         * Permite recuperar los datos de las ventas y reagruparlas por temporadas: verano, otoño, invierno 
         * y primavera.
         * Esto se realiza mediante los meses del campo fecha de las ventas:
         * Meses: 12, 1 y 2 corresponden al verano.
         * Meses: 3, 4 y 5 corresponden al otoño.
         * Meses: 6, 7 y 8 corresponden al invierno.
         * Meses: 9, 10 y 11 corresponden a la primavera.
         * 
         * Retorno:
         * 
         * Retorna una lista de objetos que tendrá los datos del monto de venta total para cada
         * temporada.La estructura es (nombreTemporada, monto). Por ejemplo, ("Primavera", 500000)
         */
        public static List<object[]> ObtenerVentasPorTemporada()
        {
            try
            {
                var ventas = Repository.ListarVentas(false);
                var ventasPorTemporada = ventas
                    .GroupBy(v => ObtenerTemporada(v.FechaVenta.Month))
                    .Select(g => new
                    {
                        Temporada = g.Key,
                        TotalVentas = g.Sum(v => v.Total)
                    })
                    .OrderBy(x => OrdenTemporada(x.Temporada))
                    .ToList();

                var datos = ventasPorTemporada
                    .Select(v => new object[] { v.Temporada, Math.Round((double)v.TotalVentas, 2) })
                    .ToList();

                return datos;
            }
            catch (Exception ex) {
                throw new Exception("Error al recuperar las ventas por temporada");
            }
           
        }

        public static List<object[]> ObtenerVentasPorMes()
        {
            try
            {
                var ventas = Repository.ListarVentas(false);
                var ventasPorMes = ventas
                    .GroupBy(v => ObtenerMes(v.FechaVenta.Month))
                    .Select(g => new
                    {
                        Mes = g.Key,
                        TotalVentas = g.Sum(v => v.Total)
                    })
                    .OrderBy(x => OrdenMes(x.Mes))
                    .ToList();

                var datos = ventasPorMes
                    .Select(v => new object[] { v.Mes, Math.Round((double)v.TotalVentas, 2) })
                    .ToList();

                return datos;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al recuperar las ventas por temporada");
            }

        }

        /*
         * Clase auxiliar para nombrar a las temporadas según el valor del mes y poder agrupar las
         * ventas según ese esa temporada
         * 
         * Parámetros:
         * mes: entero que representa el mes yendo desde 1-enero hasta 12-diciembre
         * 
         * Retorno:
         * Retorna un string que representa el nombre de la temporada
         */
        private static string ObtenerTemporada(int mes)
        {
            return mes switch
            {
                12 or 1 or 2 => "Verano",
                3 or 4 or 5 => "Otoño",
                6 or 7 or 8 => "Invierno",
                9 or 10 or 11 => "Primavera",
                _ => "Desconocido"
            };
        }

        /*
         * Método que permite ordenar las temparadas. Cada número presenta qué mes es considerado primero,
         * segundo y así hasta el último.
         * 
         * Parámetros:
         * 
         * temporada: string con el nombre de la temporada.
         * 
         * Retorno:
         * 
         * Retorna un entero que representa el orden de la posición para esa temporada.
         */

        private static int OrdenTemporada(string temporada)
        {
            return temporada switch
            {
                "Verano" => 1,
                "Otoño" => 2,
                "Invierno" => 3,
                "Primavera" => 4,
                _ => 5
            };
        }

        private static string ObtenerMes(int mes)
        {
            return mes switch
            {
                1  => "Enero",
                2=> "Febrero",
                3=> "Marzo",
                4=> "Abril",
                5 => "Mayo",
                6 => "Junio",
                7 => "Julio",
                8 => "Agosto",
                9 => "Septiembre",
                10 => "Octubre",
                11 => "Noviembre",
                12 => "Diciembre",
                _=> "Indefinido"
            };
        }

        private static int OrdenMes(string mes)
        {
            return mes switch
            {
                "Enero" => 1,
                "Febrero" => 2,
                "Marzo" => 3,
                "Abril" => 4,
                "Mayo" => 5,
                "Junio" => 6,
                "Julio" => 7,
                "Agosto" => 8,
                "Septiembre" => 9,
                "Octubre" => 10,
                "Noviembre" => 11,
                "Diciembre" => 12,
                _ => 13
            };
        }

        public static List<object[]> ObtenerVentasPorSubcategoria(bool ordenAscendente = false)
        {
            try
            {
                var ventasPorSubcategoria = Repository.ListarVentasPorSubcategoriaCompleto(false);

                // Aplicar ordenamiento según el parámetro
                var subcategoriasOrdenadas = ordenAscendente
                    ? ventasPorSubcategoria.OrderBy(x => x.TotalVentas).ToList()
                    : ventasPorSubcategoria.OrderByDescending(x => x.TotalVentas).ToList();

                // Usar solo el nombre de la subcategoría
                var datos = subcategoriasOrdenadas
                    .Select(v => new object[] {
                v.Subcategoria,
                Math.Round((double)v.TotalVentas, 2)
                    })
                    .ToList();

                return datos;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        // 4. FUNCIÓN AUXILIAR PARA OBTENER SUBCATEGORÍA (NUEVA)
        private static string ObtenerSubcategoria(string productoNombre)
        {
            // Esta es una implementación básica - debes adaptarla a tu estructura de datos
            // Por ahora, devuelve el nombre completo del producto como subcategoría
            return productoNombre;
        }
    }
}
