using System;
using System.Drawing;
using System.Windows.Forms;
using TeoAccesorios.Desktop;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop.UI.Provincias
{
    public partial class LocalidadEditForm : Form
    {
        private readonly TextBox _txtNombre = new() { Dock = DockStyle.Fill };
        private readonly ComboBox _cmbProvincia = new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Button _btnGuardar = new() { Text = "Guardar" };
        private readonly Button _btnCancelar = new() { Text = "Cancelar", DialogResult = DialogResult.Cancel };
        private readonly Localidad? _model;

        public LocalidadEditForm(Localidad? model = null)
        {
            _model = model ?? new Localidad { Activo = true };

            Text = _model.Id == 0 ? "Nueva Localidad" : "Editar Localidad";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(10), ColumnCount = 2, RowCount = 3, AutoSize = true };
            layout.Controls.Add(new Label { Text = "Nombre:", AutoSize = true, Anchor = AnchorStyles.Left, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            layout.Controls.Add(_txtNombre, 1, 0);
            layout.Controls.Add(new Label { Text = "Provincia:", AutoSize = true, Anchor = AnchorStyles.Left, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
            layout.Controls.Add(_cmbProvincia, 1, 1);

            var pnlBotones = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, AutoSize = true };
            pnlBotones.Controls.Add(_btnGuardar);
            pnlBotones.Controls.Add(_btnCancelar);
            layout.Controls.Add(pnlBotones, 0, 2);
            layout.SetColumnSpan(pnlBotones, 2);

            Controls.Add(layout);

            CargarProvincias();

            _txtNombre.Text = _model.Nombre;
            if (_model.ProvinciaId > 0)
            {
                _cmbProvincia.SelectedValue = _model.ProvinciaId;
                // Si la localidad ya tiene una provincia (sea nueva o en edición),
                // no permitimos cambiarla desde este diálogo para mantener la consistencia
                // con la selección del formulario principal.
                _cmbProvincia.Enabled = false;
            }

            _btnGuardar.Click += Guardar;
            AcceptButton = _btnGuardar;
            CancelButton = _btnCancelar;
        }

        private void CargarProvincias()
        {
            _cmbProvincia.DataSource = Repository.ListarProvincias(false);
            _cmbProvincia.DisplayMember = "Nombre";
            _cmbProvincia.ValueMember = "Id";
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

            if (_cmbProvincia.SelectedValue == null)
            {
                MessageBox.Show("Debe seleccionar una provincia.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _cmbProvincia.Focus();
                return;
            }

            if (_model == null) return; // Should not happen

            _model.Nombre = nombre;
            _model.ProvinciaId = (int)_cmbProvincia.SelectedValue;

            try
            {
                if (_model.Id > 0)
                {
                    Repository.ActualizarLocalidad(_model);
                }
                else
                {
                    _model.Id = Repository.CrearLocalidad(_model);
                }
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar la localidad: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}