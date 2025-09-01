using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class ReportesForm : Form
    {
        private RadioButton rbSemana = new RadioButton { Text = "Semanal", Checked = true, AutoSize = true };
        private RadioButton rbMes = new RadioButton { Text = "Mensual", AutoSize = true };
        private RadioButton rbRango = new RadioButton { Text = "Rango", AutoSize = true };
        private DateTimePicker dpSemana = new DateTimePicker { Value = DateTime.Today, Width = 120 };
        private DateTimePicker dpMes = new DateTimePicker { Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1), Format = DateTimePickerFormat.Custom, CustomFormat = "MM/yyyy", ShowUpDown = true, Width = 90 };
        private DateTimePicker dpDesde = new DateTimePicker { Value = DateTime.Today.AddDays(-7), Width = 120 };
        private DateTimePicker dpHasta = new DateTimePicker { Value = DateTime.Today, Width = 120 };

        private ComboBox cboVendedor = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
        private ComboBox cboCliente = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
        private CheckBox chkExcluirAnuladas = new CheckBox { Text = "Excluir anuladas", Checked = true, AutoSize = true };

        private Button btnAplicar = new Button { Text = "Aplicar" };
        private Button btnExport = new Button { Text = "Exportar" };

        private Label kpiIngresos = new Label();
        private Label kpiVentas = new Label();
        private Label kpiClientes = new Label();
        private Label kpiProductos = new Label();

        private DataGridView grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };

        public ReportesForm()
        {
            Text = "Reportes";

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));    // filtros
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));   // KPIs
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));    // tabla

            // --- Filtros ---
            var filtros = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(8), AutoSize = false, Height = 56 };
            filtros.Controls.Add(new Label { Text = "Rango:", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(0, 8, 0, 0) });
            filtros.Controls.Add(rbSemana); filtros.Controls.Add(dpSemana);
            filtros.Controls.Add(rbMes); filtros.Controls.Add(dpMes);
            filtros.Controls.Add(rbRango);
            filtros.Controls.Add(new Label { Text = "Desde:", AutoSize = true, Padding = new Padding(10, 8, 0, 0) });
            filtros.Controls.Add(dpDesde);
            filtros.Controls.Add(new Label { Text = "Hasta:", AutoSize = true, Padding = new Padding(6, 8, 0, 0) });
            filtros.Controls.Add(dpHasta);
            filtros.Controls.Add(new Label { Text = "Vendedor:", AutoSize = true, Padding = new Padding(10, 8, 0, 0) }); filtros.Controls.Add(cboVendedor);
            filtros.Controls.Add(new Label { Text = "Cliente:", AutoSize = true, Padding = new Padding(10, 8, 0, 0) }); filtros.Controls.Add(cboCliente);
            filtros.Controls.Add(chkExcluirAnuladas);
            filtros.Controls.Add(btnAplicar);
            filtros.Controls.Add(btnExport);

            // combos
            cboVendedor.Items.Add("Todos");
            foreach (var u in MockData.Usuarios.Where(u => u.Activo)) cboVendedor.Items.Add(u.NombreUsuario);
            cboVendedor.SelectedIndex = 0;
            cboCliente.Items.Add("Todos");
            foreach (var c in MockData.Clientes) cboCliente.Items.Add(c.Nombre);
            cboCliente.SelectedIndex = 0;

            // --- KPIs ---
            var kpis = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, Padding = new Padding(12) };
            for (int i = 0; i < 4; i++) kpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            kpis.Controls.Add(Card("Ingresos totales", kpiIngresos), 0, 0);
            kpis.Controls.Add(Card("Cantidad de ventas", kpiVentas), 1, 0);
            kpis.Controls.Add(Card("Clientes (únicos)", kpiClientes), 2, 0);
            kpis.Controls.Add(Card("Productos vendidos", kpiProductos), 3, 0);

            // --- Tabla ---
            grid.CellDoubleClick += (s, e) => {
                if (e.RowIndex >= 0)
                {
                    var cell = grid.Rows[e.RowIndex].Cells["IdVenta"];
                    if (cell != null && int.TryParse(cell.Value?.ToString(), out int id))
                    {
                        var venta = MockData.Ventas.FirstOrDefault(x => x.Id == id);
                        if (venta != null) new VentaDetalleForm(venta).ShowDialog(this);
                    }
                }
            };

            root.Controls.Add(filtros, 0, 0);
            root.Controls.Add(kpis, 0, 1);
            root.Controls.Add(grid, 0, 2);
            Controls.Add(root);

            btnAplicar.Click += (_, __) => LoadData();
            btnExport.Click += (_, __) => Exportar();

            LoadData();
        }

        private Control Card(string titulo, Label valueLabel)
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(18, 22, 35), Padding = new Padding(10) };
            var t = new Label { Text = titulo, ForeColor = Color.Gainsboro, Dock = DockStyle.Top, Height = 22 };
            valueLabel.Text = "-";
            valueLabel.ForeColor = Color.White;
            valueLabel.Dock = DockStyle.Fill;
            valueLabel.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            valueLabel.TextAlign = ContentAlignment.MiddleLeft;
            panel.Controls.Add(valueLabel); panel.Controls.Add(t);
            return panel;
        }

        private (DateTime start, DateTime end) GetRange()
        {
            if (rbSemana.Checked)
            {
                // semana ISO: lunes (inclusive) a lunes siguiente (exclusivo)
                var d = dpSemana.Value.Date;
                // Reemplaza la línea problemática en el método GetRange():
                // var start = d.AddDays(-(int)((d.DayOfWeek + 6) % 7)); // lleva al lunes

                var start = d.AddDays(-(((int)d.DayOfWeek + 6) % 7)); // lleva al lunes
                var end = start.AddDays(7);
                return (start, end);
            }
            if (rbMes.Checked)
            {
                var start = new DateTime(dpMes.Value.Year, dpMes.Value.Month, 1);
                var end = start.AddMonths(1);
                return (start, end);
            }
            // rango custom
            var s = dpDesde.Value.Date;
            var e = dpHasta.Value.Date.AddDays(1);
            return (s, e);
        }

        private void LoadData()
        {
            var (start, end) = GetRange();
            IEnumerable<Venta> q = MockData.Ventas.Where(v => v.Fecha >= start && v.Fecha < end);

            if (chkExcluirAnuladas.Checked) q = q.Where(v => !v.Anulada);

            if (cboVendedor.SelectedIndex > 0)
            {
                var vend = cboVendedor.SelectedItem!.ToString()!;
                q = q.Where(v => string.Equals(v.Vendedor, vend, StringComparison.OrdinalIgnoreCase));
            }
            if (cboCliente.SelectedIndex > 0)
            {
                var cli = cboCliente.SelectedItem!.ToString()!;
                q = q.Where(v => string.Equals(v.ClienteNombre, cli, StringComparison.OrdinalIgnoreCase));
            }

            var detallesCount = q.SelectMany(v => v.Detalles).Sum(d => d.Cantidad);

            // KPIs
            var ingresos = q.Sum(v => v.Total);
            var ventas = q.Count();
            var clientesUnicos = q.Select(v => v.ClienteId).Distinct().Count();

            kpiIngresos.Text = "$ " + ingresos.ToString("N0");
            kpiVentas.Text = ventas.ToString();
            kpiClientes.Text = clientesUnicos.ToString();
            kpiProductos.Text = detallesCount.ToString();

            // Tabla
            var tabla = q
                .OrderByDescending(v => v.Fecha)
                .Select(v => new {
                    Vendedor = v.Vendedor,
                    Cliente = v.ClienteNombre,
                    IdVenta = v.Id,
                    Fecha = v.Fecha.ToString("dd/MM/yyyy HH:mm"),
                    Total = v.Total.ToString("N0")
                })
                .ToList();

            grid.DataSource = tabla;
        }

        // ====== EXPORTES ======
        private void Exportar()
        {
            using var dlg = new Form
            {
                Text = "Exportar",
                StartPosition = FormStartPosition.CenterParent,
                Width = 360,
                Height = 180,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };
            var p = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12) };
            var b1 = new Button { Text = "CSV (coma)", Width = 140, Height = 34 };
            var b2 = new Button { Text = "CSV (punto y coma)", Width = 140, Height = 34 };
            var b3 = new Button { Text = "TSV", Width = 140, Height = 34 };
            var b4 = new Button { Text = "JSON", Width = 140, Height = 34 };
            b1.Click += (_, __) => { dlg.Close(); ExportCsvLike("CSV (*.csv)", ","); };
            b2.Click += (_, __) => { dlg.Close(); ExportCsvLike("CSV (*.csv)", ";"); };
            b3.Click += (_, __) => { dlg.Close(); ExportCsvLike("TSV (*.tsv)", "\t"); };
            b4.Click += (_, __) => { dlg.Close(); ExportJson(); };
            p.Controls.AddRange(new Control[] { b1, b2, b3, b4 });
            dlg.Controls.Add(p);
            dlg.ShowDialog(this);
        }

        private void ExportCsvLike(string filterName, string delimiter)
        {
            if (grid.DataSource == null) return;

            using var sfd = new SaveFileDialog { Filter = $"{filterName}|*.*", FileName = "reporte.csv" };
            if (sfd.ShowDialog(this) != DialogResult.OK) return;

            using var fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write);
            using var w = new StreamWriter(fs, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)); // UTF-8 BOM

            // Encabezados
            for (int i = 0; i < grid.Columns.Count; i++)
            {
                if (i > 0) w.Write(delimiter);
                w.Write(grid.Columns[i].HeaderText);
            }
            w.WriteLine();

            // Filas
            foreach (DataGridViewRow r in grid.Rows)
            {
                if (r.IsNewRow) continue;
                for (int i = 0; i < grid.Columns.Count; i++)
                {
                    if (i > 0) w.Write(delimiter);
                    var val = r.Cells[i].Value?.ToString() ?? "";
                    if (delimiter != "\t" && (val.Contains(delimiter) || val.Contains("\"") || val.Contains('\n')))
                    {
                        val = "\"" + val.Replace("\"", "\"\"") + "\"";
                    }
                    w.Write(val);
                }
                w.WriteLine();
            }
        }

        private void ExportJson()
        {
            if (grid.DataSource == null) return;

            using var sfd = new SaveFileDialog { Filter = "JSON (*.json)|*.json", FileName = "reporte.json" };
            if (sfd.ShowDialog(this) != DialogResult.OK) return;

            var rows = new List<Dictionary<string, object?>>();
            foreach (DataGridViewRow r in grid.Rows)
            {
                if (r.IsNewRow) continue;
                var dict = new Dictionary<string, object?>();
                foreach (DataGridViewColumn c in grid.Columns)
                    dict[c.HeaderText] = r.Cells[c.Index].Value;
                rows.Add(dict);
            }

            var json = JsonSerializer.Serialize(rows, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(sfd.FileName, json, new UTF8Encoding(true)); // UTF-8 con BOM
        }
    }
}
