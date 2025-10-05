using Microsoft.Data.SqlClient;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Datos;
using TeoAccesorios.Desktop.Infra.Reportes;

namespace TeoAccesorios.Desktop.UI.Reportes
{
    public partial class frmReportesVentas : Form
    {
        public frmReportesVentas()
        {
            InitializeComponent();
            webv_ventas.CoreWebView2InitializationCompleted += MostrarGrafico;
            webv_ventas.EnsureCoreWebView2Async();

            // Configurar evento del ComboBox
            cmb_filtro.SelectedIndexChanged += cmb_filtro_SelectedIndexChanged;
            cmb_filtro.SelectedIndex = 0;
            rdbAscendente.Checked = true;
        }

        // EVENTO DEL COMBOBOX - Actualizar gráfico cuando cambia la selección
        private void cmb_filtro_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        /*
         *Mostrará el gráfico según los criterios de filtros.
         *Aquí debe agregarse las demás opciones, en caso de que algo cambie
         */
        private void MostrarGraficoSegunFiltro()
        {
            List<object[]> datos;
            string tituloEjeX, tituloEjeY, tituloGrafico;

            switch (cmb_filtro.SelectedItem?.ToString())
            {
                case "Temporada":
                    datos = ObtenerVentasPorTemporada();
                    tituloEjeX = "Temporada";
                    tituloEjeY = "Total Ventas ($)";
                    tituloGrafico = "Ventas por Temporada";
                    break;

                case "Categoria":
                    datos = ObtenerVentasPorCategoria(dtpDesde.Value, dtpHasta.Value, rdbAscendente.Checked);
                    tituloEjeX = "Categoría";
                    tituloEjeY = "Total Ventas ($)";
                    tituloGrafico = "Ventas por Categoría";
                    break;

                case "Vendedor":
                    datos = ObtenerVentasPorVendedor(dtpDesde.Value, dtpHasta.Value, rdbAscendente.Checked);
                    tituloEjeX = "Vendedor";
                    tituloEjeY = "Total Ventas ($)";
                    tituloGrafico = "Ventas por Vendedor";
                    break;

                case "Subcategoria":
                    datos = ObtenerVentasPorSubcategoria();
                    tituloEjeX = "Subcategoría";
                    tituloEjeY = "Total Ventas ($)";
                    tituloGrafico = "Ventas por Subcategoría";
                    break;

                case "Meses":
                    datos = ObtenerVentasPorMes();
                    tituloEjeX = "Mes";
                    tituloEjeY = "Total Ventas ($)";
                    tituloGrafico = "Ventas por Mes";
                    break;

                default:
                    // Por defecto mostrar temporadas
                    datos = ObtenerVentasPorCategoria(dtpDesde.Value, dtpHasta.Value, rdbAscendente.Checked);
                    tituloEjeX = "Categoria";
                    tituloEjeY = "Total Ventas ($)";
                    tituloGrafico = "Ventas por Categoria";
                    break;
            }

            var datosJson = ReportHelper.ConvertirDatosAJson(datos);
            var html = ReportHelper.GenerarGraficoBarras(datosJson, tituloEjeX, tituloEjeY, tituloGrafico);
            webv_ventas.NavigateToString(html);
        }

        // 1. FUNCIÓN PARA VENTAS POR CATEGORÍA (NUEVA)
        private List<object[]> ObtenerVentasPorCategoria(DateTime fechaDesde, DateTime fechaHasta, bool ascendente = false)
        {

            try
            {
                
                return Reports.ObtenerVentasPorCategoria(fechaDesde, fechaHasta, ascendente);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
                return [];
            }

        }

        // 2. FUNCIÓN PARA VENTAS POR VENDEDOR (NUEVA)
        private List<object[]> ObtenerVentasPorVendedor(DateTime fechaDesde, DateTime fechaHasta, bool ascendente=false)
        {
            try
            {
                return Reports.ObtenerVentasPorVendedor(fechaDesde, fechaHasta, ascendente);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return [];
            }

        }

        // 3. FUNCIÓN PARA VENTAS POR SUBCATEGORÍA (NUEVA)
        private List<object[]> ObtenerVentasPorSubcategoria()
        {
            try
            {
                return Reports.ObtenerVentasPorSubcategoria();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return [];
            }

        }

        // 4. FUNCIÓN AUXILIAR PARA OBTENER SUBCATEGORÍA (NUEVA)
        private string ObtenerSubcategoria(string productoNombre)
        {
            // Esta es una implementación básica - debes adaptarla a tu estructura de datos
            // Por ahora, devuelve el nombre completo del producto como subcategoría
            return productoNombre;
        }

        // 5. FUNCIONES EXISTENTES (MANTENER)
        private List<object[]> ObtenerVentasPorTemporada()
        {
            try
            {
                return Reports.ObtenerVentasPorTemporada();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return [];
            }

        }

        private List<object[]> ObtenerVentasPorMes()
        {
            try
            {
                return Reports.ObtenerVentasPorMes();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return [];
            }

        }




        // ACTUALIZAR LA FUNCIÓN MostrarGrafico ORIGINAL
        private void MostrarGrafico(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            MostrarGraficoSegunFiltro();
        }

        private void rdbAscendente_CheckedChanged(object sender, EventArgs e)
        {
            if (rdbAscendente.Checked)
            {
                rdbDescendente.Checked = false;
            }
        }

        private void rdbDescendente_CheckedChanged(object sender, EventArgs e)
        {
            if (rdbDescendente.Checked)
            {
                rdbAscendente.Checked = false;
            }
        }

        private void btnFiltrar_Click(object sender, EventArgs e)
        {
            if (webv_ventas.CoreWebView2 != null)
            {
                MostrarGraficoSegunFiltro();
            }
        }
    }
}
