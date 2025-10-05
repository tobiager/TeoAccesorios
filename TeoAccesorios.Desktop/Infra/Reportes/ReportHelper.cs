using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace TeoAccesorios.Desktop.Infra.Reportes
{
    public static class ReportHelper
    {
        /*
         * Permite convertir la lista de objetos con los datos que queremos mostrar
         * en Highchart,, a formato JSON que es el formato entendido por Highcharts
         * 
         * Parámetros:
         * 
         * datos: Una lista de objetos con los datos que se mostrarán
         * 
         * Retorno:
         * 
         * Retorna un archivo en formato JSON con los que se usarán en las
         * gráficas de Highcharts. El formato del JSON variará dependiendo
         * el tipo de gráfico a mostrar.
         */
        public static string ConvertirDatosAJson(List<object[]> datos)
        {
            return JsonSerializer.Serialize(datos);
        }

        /*
         * Permite retornar un código html y javascript que mostrará un gráfico
         * de barras con los datos pasados. 
         * 
         * Parámetros:
         * 
         * datosJson: Datos en formato JSON que servirán para armar la gráfica
         * tituloEjeX: El título que tendrá el eje X en el gráfico
         * tituloEjeY: El título que tendrá el eje Y en el gráfico
         * tituloGrafico: El título general del gráfico
         * 
         * Retorno:
         * 
         * Retorna un string en formato HTML con el gráfico de barras.
         */
        public static string GenerarGraficoBarras(string datosJson, string tituloEjeX, string tituloEjeY, string tituloGrafico)
        {
            string rutaBase = Path.Combine(Application.StartupPath, "Recursos", "Highcharts");
            string highchartsJs = Path.Combine(rutaBase, "highcharts.js");
            string highchartsScript = File.ReadAllText(highchartsJs);

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <script>{highchartsScript}</script>
</head>
<body>
<div id='container' style='width:100%; height:400px;'></div>
<script>
    var data = {datosJson};

    Highcharts.chart('container', {{
        chart: {{ type: 'column' }},
        title: {{ text: '{tituloGrafico}' }},
        xAxis: {{
            type: 'category',
            title: {{ text: '{tituloEjeX}' }}
        }},
        yAxis: {{
            title: {{ text: '{tituloEjeY}' }}
        }},
        series: [{{
            name: '{tituloEjeY}',
            data: data
        }}]
    }});
</script>
</body>
</html>";
        }
    }
}