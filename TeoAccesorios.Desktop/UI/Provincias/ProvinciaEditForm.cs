using System;
using System.Drawing;
using System.Windows.Forms;
using TeoAccesorios.Desktop;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop.UI.Provincias
{
    public partial class ProvinciaEditForm : Form
    {
        private readonly TextBox _txtNombre = new() { Dock = DockStyle.Fill };
        private readonly Button _btnGuardar = new() { Text = "Guardar" };
        private readonly Button _btnCancelar = new() { Text = "Cancelar", DialogResult = DialogResult.Cancel };
        private readonly Provincia _model;

        public ProvinciaEditForm(Provincia? model = null)
        {
            _model = model ?? new Provincia { Activo = true };

            Text = _model.Id == 0 ? "Nueva Provincia" : "Editar Provincia";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(10), ColumnCount = 2, RowCount = 2, AutoSize = true };
            layout.Controls.Add(new Label { Text = "Nombre:", AutoSize = true, Anchor = AnchorStyles.Left, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            layout.Controls.Add(_txtNombre, 1, 0);

            var pnlBotones = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, AutoSize = true };
            pnlBotones.Controls.Add(_btnGuardar);
            pnlBotones.Controls.Add(_btnCancelar);
            layout.Controls.Add(pnlBotones, 0, 1);
            layout.SetColumnSpan(pnlBotones, 2);

            Controls.Add(layout);

            _txtNombre.Text = _model.Nombre;

            _btnGuardar.Click += Guardar;
            AcceptButton = _btnGuardar;
            CancelButton = _btnCancelar;
        }

        private void Guardar(object? sender, EventArgs e)
        {
            var nombre = _txtNombre.Text.Trim();
            if (string.IsNullOrWhiteSpace(nombre))
            {
                MessageBox.Show("El nombre no puede estar vacío.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtNombre.Focus();
                return;
            }

            _model.Nombre = nombre;

            try
            {
                if (_model.Id > 0)
                {
                    Repository.ActualizarProvincia(_model);
                }
                else
                {
                    _model.Id = Repository.CrearProvincia(_model);
                }
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar la provincia: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}