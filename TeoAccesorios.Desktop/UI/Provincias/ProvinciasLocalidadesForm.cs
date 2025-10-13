using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using TeoAccesorios.Desktop;                   // Db, Repository, GridHelper, GridHelperLock
using TeoAccesorios.Desktop.Models;            // Provincia, Localidad, DTOs
using Models = TeoAccesorios.Desktop.Models;   // alias para Stats
using ProvsUI = TeoAccesorios.Desktop.UI.Provincias;

namespace TeoAccesorios.Desktop.UI.Provincias
{
    public class ProvinciasLocalidadesForm : Form
    {
        private readonly SplitContainer _splitContainer = new() { Dock = DockStyle.Fill, SplitterDistance = 350 };
        private readonly DataGridView _dgvProvincias = new() { Dock = DockStyle.Fill, AutoGenerateColumns = true };
        private readonly DataGridView _dgvLocalidades = new() { Dock = DockStyle.Fill, AutoGenerateColumns = true };
        private readonly Label _lblLocalidadesTitle = new() { Text = "Localidades", Font = new Font("Segoe UI", 12, FontStyle.Bold), AutoSize = true, Padding = new Padding(5) };
        private readonly CheckBox _chkVerInactivas = new() { Text = "Ver inactivas", AutoSize = true, Padding = new Padding(8, 6, 0, 0) };

        public ProvinciasLocalidadesForm()
        {
            Text = "Gestión de Provincias y Localidades";
            Width = 950;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;

            // ----- Panel Izquierdo: Provincias
            var pnlProvincias = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            pnlProvincias.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pnlProvincias.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pnlProvincias.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            pnlProvincias.Controls.Add(
                new Label { Text = "Provincias", Font = new Font("Segoe UI", 12, FontStyle.Bold), AutoSize = true, Padding = new Padding(5) }, 0, 0);

            var pnlBotonesProv = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(5), AutoSize = true };
            // Los botones de acción para Provincias se han eliminado según el requisito.
            // El panel se mantiene por si se añaden acciones de solo lectura en el futuro (ej. "Exportar").
            
            pnlProvincias.Controls.Add(pnlBotonesProv, 0, 1);
            pnlProvincias.Controls.Add(_dgvProvincias, 0, 2);
            _splitContainer.Panel1.Controls.Add(pnlProvincias);

            // ----- Panel Derecho: Localidades
            var pnlLocalidades = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            pnlLocalidades.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pnlLocalidades.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pnlLocalidades.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            pnlLocalidades.Controls.Add(_lblLocalidadesTitle, 0, 0);

            var pnlBotonesLoc = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(5), AutoSize = true };
            var btnAgregarLoc = new Button { Text = "Agregar Localidad", AutoSize = true };
            var btnEditarLoc = new Button { Text = "Editar Localidad", AutoSize = true };
            var btnToggleActivoLoc = new Button { Text = "Activar/Inactivar", AutoSize = true };
            pnlBotonesLoc.Controls.AddRange(new Control[] { btnAgregarLoc, btnEditarLoc, btnToggleActivoLoc });

            pnlLocalidades.Controls.Add(pnlBotonesLoc, 0, 1);
            pnlLocalidades.Controls.Add(_dgvLocalidades, 0, 2);
            _splitContainer.Panel2.Controls.Add(pnlLocalidades);

            // Controles principales
            var topPanel = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true };
            topPanel.Controls.Add(_chkVerInactivas);
            Controls.Add(topPanel);
            Controls.Add(_splitContainer);
            _splitContainer.BringToFront();

            // Estética unificada
            SetupGrid(_dgvProvincias);
            SetupGrid(_dgvLocalidades);

            // Los ajustes de columnas se aplican al terminar el binding
            _dgvProvincias.DataBindingComplete += (_, __) => TweakProvinciasColumns();
            _dgvLocalidades.DataBindingComplete += (_, __) => TweakLocalidadesColumns();

            // ----- Eventos Provincias (solo selección)
            _dgvProvincias.SelectionChanged += (s, e) => CargarLocalidades();

            // ----- Eventos Localidades
            btnAgregarLoc.Click += (s, e) =>
            {
                if (_dgvProvincias.CurrentRow?.DataBoundItem is not Models.ProvinciaStats provStat)
                {
                    MessageBox.Show("Primero debe seleccionar una provincia.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                var nuevaLocalidad = new Models.Localidad { ProvinciaId = provStat.Id, Activo = true };
                using var form = new ProvsUI.LocalidadEditForm(nuevaLocalidad);
                if (form.ShowDialog(this) == DialogResult.OK) CargarDatos();
            };
            btnEditarLoc.Click += (s, e) => EditarLocalidadSeleccionada();
            btnToggleActivoLoc.Click += (s, e) => ToggleActivoLocalidad();

            // Recarga general
            _chkVerInactivas.CheckedChanged += (s, e) => CargarDatos();

            _dgvProvincias.CellFormatting += Grid_CellFormatting;
            _dgvLocalidades.CellFormatting += Grid_CellFormatting;

            CargarDatos();
        }

        // ----------------- Carga de datos -----------------
        private void CargarDatos()
        {
            CargarProvincias();
            CargarLocalidades();
        }

        private void CargarProvincias()
        {
            int? currentId = _dgvProvincias.CurrentRow?.DataBoundItem is Models.ProvinciaStats p ? p.Id : (int?)null;

            var data = Repository.ListarProvinciasConStats(_chkVerInactivas.Checked)
                       ?? new System.Collections.Generic.List<Models.ProvinciaStats>();
            
            // Definir columnas explícitamente para controlar el orden y la visibilidad
            _dgvProvincias.AutoGenerateColumns = false;
            if (_dgvProvincias.Columns.Count == 0)
            {
                SetupProvinciasColumns();
            }
            _dgvProvincias.DataSource = data;

            // si había fila seleccionada, intenta restaurarla
            if (currentId.HasValue) SeleccionarPorId(_dgvProvincias, currentId.Value);
        }

        private void CargarLocalidades()
        {
            if (_dgvProvincias.CurrentRow?.DataBoundItem is Models.ProvinciaStats provStat)
            {
                _lblLocalidadesTitle.Text = $"Localidades — {provStat.Nombre}";
                var data = Repository.ListarLocalidadesConStats(provStat.Id, _chkVerInactivas.Checked)
                           ?? new System.Collections.Generic.List<Models.LocalidadStats>();
                _dgvLocalidades.DataSource = data;
            }
            else
            {
                _lblLocalidadesTitle.Text = "Localidades";
                _dgvLocalidades.DataSource = null;
            }
        }

        // ----------------- Acciones Localidades -----------------
        private void EditarLocalidadSeleccionada()
        {
            if (_dgvLocalidades.CurrentRow?.DataBoundItem is not Models.LocalidadStats locStat) return;

            var locModel = Repository.ObtenerLocalidad(locStat.Id);
            if (locModel == null) return;

            using var form = new ProvsUI.LocalidadEditForm(locModel);
            if (form.ShowDialog(this) == DialogResult.OK) CargarDatos();
        }

        private void ToggleActivoLocalidad()
        {
            if (_dgvLocalidades.CurrentRow?.DataBoundItem is not Models.LocalidadStats loc) return;

            if (loc.Activo == false)
            {
                Repository.SetLocalidadActiva(loc.Id, true);
            }
            else
            {
                int count = Repository.ContarClientesPorLocalidad(loc.Id);
                if (count > 0)
                {
                    MessageBox.Show(
                        $"No se puede inactivar '{loc.Nombre}' porque está asignada a {count} clientes.",
                        "Acción bloqueada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                Repository.SetLocalidadActiva(loc.Id, false);
            }
            CargarLocalidades();
        }

        // ----------------- Estilo & columnas -----------------
        private void SetupGrid(DataGridView dgv)
        {
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;

            try { GridHelper.Estilizar(dgv); } catch { }
            try { GridHelperLock.WireDataBindingLock(dgv); } catch { }
        }

        private void SetupProvinciasColumns()
        {
            _dgvProvincias.Columns.Clear();
            _dgvProvincias.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "Id", Name = "Id", Width = 80 });
            _dgvProvincias.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Nombre", HeaderText = "Nombre", Name = "Nombre", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _dgvProvincias.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CantClientes", HeaderText = "Clientes", Name = "CantClientes", Width = 90 });
            _dgvProvincias.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CantVentas", HeaderText = "Ventas", Name = "CantVentas", Width = 90 });
        }

        // Se llama en DataBindingComplete para evitar NRE en setters de columnas
        private void TweakProvinciasColumns()
        {
            var colActivo = FindColumn(_dgvProvincias, "Activo");
            if (colActivo != null) colActivo.Visible = false;

            SetFriendlyHeader(_dgvProvincias, "CantClientes", "Clientes", "Cantidad de clientes únicos en esta provincia.");
            SetFriendlyHeader(_dgvProvincias, "CantVentas", "Ventas", "Cantidad de ventas enviadas a esta provincia.");
        }

        private void TweakLocalidadesColumns()
        {
            SetColumnWidth(_dgvLocalidades, "Id", 80);
            SetColumnWidth(_dgvLocalidades, "Activo", 100);
            SetFriendlyHeader(_dgvLocalidades, "CantClientes", "Clientes", "Cantidad de clientes únicos en esta localidad.");
            SetFriendlyHeader(_dgvLocalidades, "CantVentas", "Ventas", "Cantidad de ventas enviadas a esta localidad.");
        }

        private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (sender is not DataGridView grid || e.RowIndex < 0) return;

            var row = grid.Rows[e.RowIndex];
            bool? activo = row.DataBoundItem switch
            {
                Models.ProvinciaStats p => p.Activo,
                Models.LocalidadStats l => l.Activo,
                _ => null
            };

            if (activo == false)
            {
                e.CellStyle.BackColor = Color.LightGray;
                e.CellStyle.ForeColor = Color.DarkGray;
            }
        }

        // ----------------- helpers de columnas seguros -----------------
        private static DataGridViewColumn? FindColumn(DataGridView dgv, string name)
        {
            try
            {
                if (dgv?.Columns == null) return null;
                return dgv.Columns
                         .Cast<DataGridViewColumn>()
                         .FirstOrDefault(c =>
                             string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(c.DataPropertyName, name, StringComparison.OrdinalIgnoreCase));
            }
            catch { return null; }
        }

        private static void SetColumnWidth(DataGridView dgv, string name, int width)
        {
            if (dgv == null || dgv.IsDisposed) return;

            void Apply()
            {
                var col = FindColumn(dgv, name);
                if (col == null) return;

                try
                {
                    // puede lanzar si la columna aún no tiene owner interno
                    col.Width = width;
                }
                catch
                {
                    try { col.MinimumWidth = width; } catch { /* swallow */ }
                }
            }

            if (dgv.IsHandleCreated) Apply();
            else dgv.HandleCreated += (_, __) => Apply();
        }

        private static void SetFriendlyHeader(DataGridView dgv, string name, string headerText, string? toolTip = null)
        {
            if (dgv == null || dgv.IsDisposed) return;

            void Apply()
            {
                var col = FindColumn(dgv, name);
                if (col == null) return;

                try
                {
                    col.HeaderText = headerText;
                    if (!string.IsNullOrWhiteSpace(toolTip))
                        col.ToolTipText = toolTip!;
                }
                catch { /* swallow */ }
            }

            if (dgv.IsHandleCreated) Apply();
            else dgv.HandleCreated += (_, __) => Apply();
        }

        private void SeleccionarPorId(DataGridView dgv, int id)
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.DataBoundItem is Models.ProvinciaStats p && p.Id == id)
                {
                    row.Selected = true;
                    dgv.CurrentCell = row.Cells[0];
                    return;
                }
            }
        }
    }
}
