using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TeoAccesorios.Desktop.UI.Estadisticas
{
    public class ExportOptionsForm : Form
    {
        private readonly CheckedListBox _chkListTops;
        private readonly Button _btnExportarExcel;
        private readonly Button _btnExportarPdf;
        private readonly Button _btnCancelar;

        public List<string> SelectedTops { get; private set; } = new List<string>();
        public ExportFormat SelectedFormat { get; private set; } = ExportFormat.None;

        public enum ExportFormat { None, Excel, Pdf }

        public ExportOptionsForm(IEnumerable<string> topNames)
        {
            Text = "Opciones de Exportación";
            Size = new Size(350, 500);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            MaximizeBox = false;
            MinimizeBox = false;

            var lblTitulo = new Label
            {
                Text = "Seleccione los rankings a exportar:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20),
                ForeColor = Color.FromArgb(33, 37, 41)
            };

            _chkListTops = new CheckedListBox
            {
                Location = new Point(20, 50),
                Size = new Size(300, 280),
                CheckOnClick = true,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9)
            };
            _chkListTops.Items.AddRange(topNames.ToArray());
            for (int i = 0; i < _chkListTops.Items.Count; i++)
            {
                _chkListTops.SetItemChecked(i, true); // Marcar todos por defecto
            }

            // Panel para los botones de exportación
            var panelBotones = new Panel
            {
                Location = new Point(20, 350),
                Size = new Size(300, 100),
                BackColor = Color.FromArgb(248, 249, 250)
            };

            var lblExportar = new Label
            {
                Text = "Seleccione el formato de exportación:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true,
                ForeColor = Color.FromArgb(33, 37, 41)
            };

            _btnExportarExcel = new Button
            {
                Text = "📊 Exportar a Excel",
                Location = new Point(10, 35),
                Size = new Size(135, 40),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            _btnExportarExcel.FlatAppearance.BorderSize = 0;

            _btnExportarPdf = new Button
            {
                Text = "📄 Exportar a PDF",
                Location = new Point(155, 35),
                Size = new Size(135, 40),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            _btnExportarPdf.FlatAppearance.BorderSize = 0;

            _btnCancelar = new Button
            {
                Text = "Cancelar",
                Location = new Point(115, 460),
                Size = new Size(100, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                DialogResult = DialogResult.Cancel
            };
            _btnCancelar.FlatAppearance.BorderSize = 0;

            panelBotones.Controls.Add(lblExportar);
            panelBotones.Controls.Add(_btnExportarExcel);
            panelBotones.Controls.Add(_btnExportarPdf);

            _btnExportarExcel.Click += (s, e) => ValidateAndExport(ExportFormat.Excel);
            _btnExportarPdf.Click += (s, e) => ValidateAndExport(ExportFormat.Pdf);

            Controls.Add(lblTitulo);
            Controls.Add(_chkListTops);
            Controls.Add(panelBotones);
            Controls.Add(_btnCancelar);
            
            CancelButton = _btnCancelar;
        }

        private void ValidateAndExport(ExportFormat format)
        {
            var selectedItems = _chkListTops.CheckedItems.Cast<string>().ToList();
            
            if (!selectedItems.Any())
            {
                MessageBox.Show("Debe seleccionar al menos un ranking para exportar.", 
                              "Selección Vacía", 
                              MessageBoxButtons.OK, 
                              MessageBoxIcon.Warning);
                return;
            }

            SelectedTops = selectedItems;
            SelectedFormat = format;
            DialogResult = DialogResult.OK;
        }
    }
}