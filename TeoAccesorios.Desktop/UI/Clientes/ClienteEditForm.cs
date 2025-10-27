using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;
using TeoAccesorios.Desktop.UI.Common;

namespace TeoAccesorios.Desktop
{
    public class ClienteEditForm : Form
    {
        private readonly TextBox txtNombre = new();
        private readonly TextBox txtEmail = new();
        private readonly TextBox txtTel = new();
        private readonly TextBox txtDir = new();
        private readonly ComboBox cmbProv = new() { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox cmbLoc = new() { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly CheckBox chkActivo = new() { Text = "Activo" };
        private readonly Button btnGuardar = new() { Text = "Guardar", Width = 100, Height = 30 };
        private readonly Button btnCancel = new() { Text = "Cancelar", Width = 100, Height = 30 };
        private readonly ErrorProvider ep = new();

        private readonly Cliente model;

        public ClienteEditForm(Cliente c)
        {
            model = c ?? new Cliente { Activo = true };

            Text = "Cliente";
            Width = 520; Height = 380;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;

            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(12) };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 7; i++) grid.RowStyles.Add(new RowStyle(SizeType.Absolute, i == 6 ? 46 : 36));

            grid.Controls.Add(new Label { Text = "Nombre *", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 0); grid.Controls.Add(txtNombre, 1, 0); txtNombre.Dock = DockStyle.Fill;
            grid.Controls.Add(new Label { Text = "Email *", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 1); grid.Controls.Add(txtEmail, 1, 1); txtEmail.Dock = DockStyle.Fill;
            grid.Controls.Add(new Label { Text = "Teléfono", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 2); grid.Controls.Add(txtTel, 1, 2); txtTel.Dock = DockStyle.Fill;
            grid.Controls.Add(new Label { Text = "Dirección *", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 3); grid.Controls.Add(txtDir, 1, 3); txtDir.Dock = DockStyle.Fill;
            grid.Controls.Add(new Label { Text = "Provincia *", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 4); grid.Controls.Add(cmbProv, 1, 4); cmbProv.Dock = DockStyle.Fill;
            grid.Controls.Add(new Label { Text = "Localidad *", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 5); grid.Controls.Add(cmbLoc, 1, 5); cmbLoc.Dock = DockStyle.Fill;

            var panelButtons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            panelButtons.Controls.Add(btnGuardar);
            panelButtons.Controls.Add(btnCancel);
            panelButtons.Controls.Add(chkActivo);

            grid.Controls.Add(panelButtons, 0, 6);
            grid.SetColumnSpan(panelButtons, 2);

            Controls.Add(grid);

            AcceptButton = btnGuardar;
            CancelButton = btnCancel;

            // Carga de datos
            LoadProvinciasAndLocalidades();

            // Eventos
            cmbProv.SelectedValueChanged += CmbProv_SelectedValueChanged;

            // Asignar validación a todos los controles relevantes
            AssignValidationHandlers();
            btnGuardar.Click += (_, __) =>
            {
                if (!Validar()) return;

                model.Nombre = txtNombre.Text.Trim();
                model.Email = txtEmail.Text.Trim(); // Ya no puede ser null porque es obligatorio
                model.Telefono = string.IsNullOrWhiteSpace(txtTel.Text) ? null : txtTel.Text.Trim();
                model.Direccion = txtDir.Text.Trim();
                model.LocalidadId = (int?)cmbLoc.SelectedValue;
                model.Activo = chkActivo.Checked;

                DialogResult = DialogResult.OK;
                Close();
            };

            btnCancel.Click += (_, __) => { DialogResult = DialogResult.Cancel; Close(); };

            btnGuardar.Enabled = Validar();
        }

        private void LoadProvinciasAndLocalidades()
        {
            txtNombre.Text = model.Nombre;
            txtEmail.Text = model.Email ?? "";
            txtTel.Text = model.Telefono ?? "";
            txtDir.Text = model.Direccion;
            chkActivo.Checked = model.Activo;
            
            int? provIdToSelect = null;
            int? locIdToSelect = model.LocalidadId;
            
            var provincias = Repository.ListarProvincias(false);
            
            if (model.LocalidadId.HasValue)
            {
                var loc = Repository.ObtenerLocalidad(model.LocalidadId.Value);
                if (loc != null)
                {
                    provIdToSelect = loc.ProvinciaId;
                    var prov = Repository.ObtenerProvincia(loc.ProvinciaId);
                    // Si la provincia del cliente está inactiva, la añadimos temporalmente para que se muestre
                    if (prov != null && prov.Activo == false && !provincias.Any(p => p.Id == prov.Id))
                    {
                        provincias.Add(prov);
                        provincias = provincias.OrderBy(p => p.Nombre).ToList();
                    }
                }
            }
            
            FormUtils.BindCombo(cmbProv, provincias, selectedValue: provIdToSelect);
            
            // Cargar localidades basadas en la provincia seleccionada (o la del modelo)
            var localidades = Repository.ListarLocalidades(provIdToSelect, false);
            if (locIdToSelect.HasValue)
            {
                var selectedLoc = Repository.ObtenerLocalidad(locIdToSelect.Value);
                // Si la localidad del cliente está inactiva, la añadimos temporalmente
                if (selectedLoc != null && selectedLoc.Activo == false && !localidades.Any(l => l.Id == selectedLoc.Id))
                {
                    localidades.Add(selectedLoc);
                    localidades = localidades.OrderBy(l => l.Nombre).ToList();
                }
            }
            
            FormUtils.BindCombo(cmbLoc, localidades, selectedValue: locIdToSelect);
        }

        private void CmbProv_SelectedValueChanged(object? sender, EventArgs e)
        {
            if (cmbProv.SelectedValue is int provId)
            {
                var localidades = Repository.ListarLocalidades(provId, false);
                FormUtils.BindCombo(cmbLoc, localidades);
            }
            else
            {
                FormUtils.BindCombo<Localidad>(cmbLoc, null);
            }
            Validar();
        }

        private bool Validar()
        {
            bool ok = true;
            ok &= FormValidator.Require(txtNombre, ep, "Nombre requerido", 2, 120);
            ok &= FormValidator.Require(txtDir, ep, "Dirección requerida", 3, 120);
            ok &= RequireEmail(txtEmail, ep, "Email requerido y válido");
            ok &= FormValidator.OptionalPhone(txtTel, ep, "Teléfono inválido");
            if (cmbProv.SelectedValue == null) { ep.SetError(cmbProv, "Provincia requerida"); ok = false; } else { ep.SetError(cmbProv, ""); }
            if (cmbLoc.SelectedValue == null) { ep.SetError(cmbLoc, "Localidad requerida"); ok = false; } else { ep.SetError(cmbLoc, ""); }
            return ok;
        }

        private bool RequireEmail(TextBox tb, ErrorProvider ep, string msg)
        {
            string v = tb.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(v))
            {
                ep.SetError(tb, msg);
                return false;
            }

            // Validar formato de email
            var emailValid = System.Text.RegularExpressions.Regex.IsMatch(v, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!emailValid)
            {
                ep.SetError(tb, "Email inválido");
                return false;
            }

            ep.SetError(tb, "");
            return true;
        }

        private void AssignValidationHandlers()
        {
            txtNombre.TextChanged += (_, __) => btnGuardar.Enabled = Validar();
            txtEmail.TextChanged += (_, __) => btnGuardar.Enabled = Validar();
            txtTel.TextChanged += (_, __) => btnGuardar.Enabled = Validar();
            txtDir.TextChanged += (_, __) => btnGuardar.Enabled = Validar();
            // La validación de los combos se dispara cuando cambia la selección de provincia
            // y al final de la carga inicial.
            cmbLoc.SelectedValueChanged += (_, __) => btnGuardar.Enabled = Validar();
        }
    }
}
