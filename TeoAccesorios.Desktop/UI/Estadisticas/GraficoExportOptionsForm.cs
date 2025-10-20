using System.Drawing;
using System.Windows.Forms;

namespace TeoAccesorios.Desktop.UI.Estadisticas
{
    public class GraficoExportOptionsForm : Form
    {
        public enum ExportFormat { None, Pdf, Excel }
        public ExportFormat SelectedFormat { get; private set; } = ExportFormat.None;

        public GraficoExportOptionsForm()
        {
            Text = "Opciones de Exportación";
            Size = new Size(350, 200);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = Color.White;

            var lblInfo = new Label
            {
                Text = "¿En qué formato desea exportar el gráfico?",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 10)
            };

            var btnPdf = new Button
            {
                Text = "📄 Exportar a PDF",
                Location = new Point(20, 60),
                Size = new Size(140, 40),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            var btnExcel = new Button
            {
                Text = "📊 Exportar a Excel",
                Location = new Point(170, 60),
                Size = new Size(140, 40),
                BackColor = Color.FromArgb(33, 136, 56),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            var btnCancel = new Button
            {
                Text = "Cancelar",
                Location = new Point(230, 120),
                DialogResult = DialogResult.Cancel
            };

            btnPdf.Click += (s, e) => { SelectedFormat = ExportFormat.Pdf; DialogResult = DialogResult.OK; Close(); };
            btnExcel.Click += (s, e) => { SelectedFormat = ExportFormat.Excel; DialogResult = DialogResult.OK; Close(); };

            Controls.Add(lblInfo);
            Controls.Add(btnPdf);
            Controls.Add(btnExcel);
            Controls.Add(btnCancel);

            AcceptButton = btnPdf;
            CancelButton = btnCancel;
        }
    }
}