using System;
using System.Drawing;
using System.Linq;
using System.Media;
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

        // Nuevos miembros para evitar recursión al sanear texto y para mostrar tips
        private readonly ToolTip _toolTip = new();
        private bool _suppressTextChanged = false;

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

            // Manejadores para prevenir números y sanear pegados
            _txtNombre.KeyPress += TxtNombre_KeyPress;
            _txtNombre.TextChanged += TxtNombre_TextChanged;

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

            if (_model == null) return; 

            var provinciaId = (int)_cmbProvincia.SelectedValue;

            // Comprobación de duplicados (incluye inactivos)
            try
            {
                var localidades = Repository.ListarLocalidades(provinciaId, true) ?? new System.Collections.Generic.List<Localidad>();
                var existe = localidades.Any(l => l.Id != _model.Id &&
                                                  string.Equals((l.Nombre ?? "").Trim(), nombre, StringComparison.OrdinalIgnoreCase));
                if (existe)
                {
                    var provNombre = _cmbProvincia.Text ?? "la provincia seleccionada";
                    MessageBox.Show($"La localidad \"{nombre}\" ya está registrada en {provNombre}.\n\nNo se permiten localidades duplicadas. Verificá la lista antes de continuar.",
                                    "Localidad duplicada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtNombre.Focus();
                    return;
                }
            }
            catch
            {
                // Si la comprobación por alguna razón falla, seguimos y dejamos el manejo en el try/catch de abajo.
            }

            _model.Nombre = nombre;
            _model.ProvinciaId = provinciaId;

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
                // Mensaje amigable al usuario con la opción de ver el detalle técnico en caso necesario
                MessageBox.Show("No se pudo guardar la localidad. Por favor intentá nuevamente.\n\nDetalles: " + ex.Message,
                                "Error al guardar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Evita que se puedan escribir caracteres numéricos
        private void TxtNombre_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
                SystemSounds.Beep.Play();
                _toolTip.Show("No se permiten números en el nombre de la localidad.", _txtNombre, 1000);
            }
        }

        // Sanea texto que fue pegado: elimina dígitos y notifica al usuario de forma no intrusiva
        private void TxtNombre_TextChanged(object? sender, EventArgs e)
        {
            if (_suppressTextChanged) return;

            var tb = _txtNombre;
            var text = tb.Text;
            if (string.IsNullOrEmpty(text)) return;

            if (text.Any(char.IsDigit))
            {
                _suppressTextChanged = true;
                var selStart = tb.SelectionStart;
                var newTextChars = text.Where(ch => !char.IsDigit(ch)).ToArray();
                var newText = new string(newTextChars);
                tb.Text = newText;

                // Ajustar caret position de forma inteligente
                tb.SelectionStart = Math.Min(newText.Length, Math.Max(0, selStart - (text.Length - newText.Length)));
                _suppressTextChanged = false;

                _toolTip.Show("Se eliminaron los números del nombre porque no están permitidos.", tb, 1500);
            }
        }
    }
}