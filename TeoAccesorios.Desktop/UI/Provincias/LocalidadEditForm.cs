using System;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text.RegularExpressions;
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

        // ErrorProvider para marcar con la "X" los controles inválidos y controlar estado de guardar
        private readonly ErrorProvider _ep = new();

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

            // Configurar ErrorProvider
            _ep.ContainerControl = this;
            _ep.BlinkStyle = ErrorBlinkStyle.NeverBlink;

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

            // Actualizar estado de guardar al cambiar provincia (en caso de no estar deshabilitada)
            _cmbProvincia.SelectedValueChanged += (_, __) => UpdateGuardarState();

            _btnGuardar.Click += Guardar;
            AcceptButton = _btnGuardar;
            CancelButton = _btnCancelar;

            // Estado inicial del botón según validación
            UpdateGuardarState();
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

            // Validación previa (redundante con la que mantiene el estado del botón, pero segura)
            if (!ValidarTodo())
            {
                MessageBox.Show("Por favor corregí los errores antes de guardar.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

            // La comprobación de duplicados ya la hace ValidarTodo y bloquea el botón,
            // pero se mantiene esta verificación final por seguridad.
            try
            {
                if (EsNombreDuplicado(nombre, provinciaId))
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
                // ignorar y continuar; si falla la comprobación en tiempo real, evitamos bloquear el guardado por error de repositorio
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

        // Evita que se puedan escribir caracteres numéricos o símbolos
        private void TxtNombre_KeyPress(object? sender, KeyPressEventArgs e)
        {
            // Permitimos control (backspace), letras (incluye acentos y alfabetos unicode) y espacios
            if (!char.IsControl(e.KeyChar) && !char.IsLetter(e.KeyChar) && !char.IsWhiteSpace(e.KeyChar))
            {
                e.Handled = true;
                SystemSounds.Beep.Play();
                _toolTip.Show("Solo se permiten letras y espacios. No se permiten números ni símbolos.", _txtNombre, 1500);
            }
        }

        // Sanea texto que fue pegado: elimina dígitos y símbolos, y notifica al usuario de forma no intrusiva
        private void TxtNombre_TextChanged(object? sender, EventArgs e)
        {
            if (_suppressTextChanged) return;

            var tb = _txtNombre;
            var text = tb.Text;
            if (string.IsNullOrEmpty(text))
            {
                UpdateGuardarState();
                return;
            }

            // Mantener solo letras y espacios
            if (text.Any(ch => !char.IsLetter(ch) && !char.IsWhiteSpace(ch)))
            {
                _suppressTextChanged = true;
                var selStart = tb.SelectionStart;
                var newTextChars = text.Where(ch => char.IsLetter(ch) || char.IsWhiteSpace(ch)).ToArray();
                var newText = new string(newTextChars);
                tb.Text = newText;

                // Ajustar caret position de forma inteligente
                tb.SelectionStart = Math.Min(newText.Length, Math.Max(0, selStart - (text.Length - newText.Length)));
                _suppressTextChanged = false;

                _toolTip.Show("Se eliminaron números y símbolos del nombre porque no están permitidos.", tb, 1500);
            }

            // Actualizar estado del botón Guardar y marcar errores si corresponde
            UpdateGuardarState();
        }

        // Valida que el nombre contenga solo letras/espacios y al menos 3 caracteres de letra
        private static bool IsNombreValido(string nombre, out string motivo)
        {
            motivo = string.Empty;
            if (string.IsNullOrWhiteSpace(nombre))
            {
                motivo = "El nombre no puede estar vacío.";
                return false;
            }

            // Contar solo letras (excluye espacios)
            int letras = nombre.Count(char.IsLetter);
            if (letras < 3)
            {
                motivo = "El nombre debe tener al menos 3 letras.";
                return false;
            }

            // Solo letras y espacios permitidos (uso de \p{L} para soportar alfabetos unicode)
            if (!Regex.IsMatch(nombre, @"^[\p{L} ]+$"))
            {
                motivo = "El nombre contiene caracteres no permitidos. Solo se aceptan letras y espacios, sin símbolos ni números.";
                return false;
            }

            return true;
        }

        // Comprueba en el repositorio si el nombre ya existe en la provincia indicada (excluye el registro actual)
        private bool EsNombreDuplicado(string nombre, int provinciaId)
        {
            try
            {
                var localidades = Repository.ListarLocalidades(provinciaId, true) ?? new System.Collections.Generic.List<Localidad>();
                return localidades.Any(l => l.Id != _model?.Id &&
                                             string.Equals((l.Nombre ?? "").Trim(), nombre.Trim(), StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                // Si falla el acceso a datos, consideramos que no podemos confirmar duplicado (no bloquear)
                return false;
            }
        }

        // Valida todo el formulario y establece errores en el ErrorProvider
        private bool ValidarTodo()
        {
            bool ok = true;
            _ep.SetError(_txtNombre, string.Empty);
            _ep.SetError(_cmbProvincia, string.Empty);

            var nombre = _txtNombre.Text.Trim();
            if (string.IsNullOrWhiteSpace(nombre))
            {
                _ep.SetError(_txtNombre, "Nombre requerido (mínimo 3 letras).");
                ok = false;
            }
            else if (!IsNombreValido(nombre, out var motivo))
            {
                _ep.SetError(_txtNombre, motivo);
                ok = false;
            }
            else
            {
                // Si hay provincia seleccionada, comprobar duplicado y marcar error
                if (int.TryParse(_cmbProvincia.SelectedValue?.ToString(), out int provId))
                {
                    if (EsNombreDuplicado(nombre, provId))
                    {
                        _ep.SetError(_txtNombre, $"La localidad \"{nombre}\" ya existe en {(_cmbProvincia.Text ?? "la provincia seleccionada")}.");
                        ok = false;
                    }
                }
            }

            if (_cmbProvincia.SelectedValue == null)
            {
                _ep.SetError(_cmbProvincia, "Debe seleccionar una provincia.");
                ok = false;
            }

            return ok;
        }

        // Habilita/deshabilita el botón Guardar y el AcceptButton según la validación
        private void UpdateGuardarState()
        {
            bool valido = ValidarTodo();
            _btnGuardar.Enabled = valido;
            AcceptButton = valido ? _btnGuardar : null;
        }
    }
}