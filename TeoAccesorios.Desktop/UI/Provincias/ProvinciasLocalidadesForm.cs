using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using TeoAccesorios.Desktop;                   
using TeoAccesorios.Desktop.Models;            
using Models = TeoAccesorios.Desktop.Models;   
using ProvsUI = TeoAccesorios.Desktop.UI.Provincias;

namespace TeoAccesorios.Desktop.UI.Provincias
{
    public class ProvinciasLocalidadesForm : Form
    {
        private readonly SplitContainer _splitContainer;
        private readonly DataGridView _dgvProvincias = new() { Dock = DockStyle.Fill, AutoGenerateColumns = false };
        private readonly DataGridView _dgvLocalidades = new() { Dock = DockStyle.Fill, AutoGenerateColumns = false };
        private readonly Label _lblLocalidadesTitle = new() { Text = "Localidades", Font = new Font("Segoe UI", 12, FontStyle.Bold), AutoSize = true, Padding = new Padding(5) };

        public ProvinciasLocalidadesForm()
        {
            Text = "Gestión de Provincias y Localidades";
            Width = 950;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;

            // Layout principal con SplitContainer
            _splitContainer = new SplitContainer { Dock = DockStyle.Fill, IsSplitterFixed = false };
            Controls.Add(_splitContainer);

            // Panel Izquierdo (Provincias)
            var leftPanel = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(8), RowCount = 2, ColumnCount = 1 };
            leftPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            leftPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            leftPanel.Controls.Add(new Label { Text = "Provincias", AutoSize = true, Font = new Font("Segoe UI", 12, FontStyle.Bold) }, 0, 0);
            leftPanel.Controls.Add(_dgvProvincias, 0, 1);
            _splitContainer.Panel1.Controls.Add(leftPanel);

            // Panel Derecho (Localidades)
            var rightPanel = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(8), RowCount = 3, ColumnCount = 1 };
            rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // título
            rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // botones
            rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // grid

            var barraLoc = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = false };
            var btnAgregarLoc = new Button { Text = "Agregar Localidad", AutoSize = true };
            var btnEditarLoc = new Button { Text = "Editar Localidad", AutoSize = true };
            var btnToggleActivoLoc = new Button { Text = "Activar/Inactivar", AutoSize = true };
            var btnVerInactivas = new Button { Text = "Ver inactivas", AutoSize = true };
            barraLoc.Controls.AddRange(new Control[] { btnAgregarLoc, btnEditarLoc, btnToggleActivoLoc, btnVerInactivas });

            rightPanel.Controls.Add(_lblLocalidadesTitle, 0, 0);
            rightPanel.Controls.Add(barraLoc, 0, 1);
            rightPanel.Controls.Add(_dgvLocalidades, 0, 2);
            _splitContainer.Panel2.Controls.Add(rightPanel);

            // Estética unificada
            SetupGrid(_dgvProvincias);
            SetupGrid(_dgvLocalidades);
            SetupProvinciasColumns();
            SetupLocalidadesColumns();

            // Los ajustes de columnas se aplican al terminar el binding
            _dgvProvincias.DataBindingComplete += (_, __) => TweakProvinciasColumns();
            _dgvLocalidades.DataBindingComplete += (_, __) => TweakLocalidadesColumns();

            // ----- Eventos Provincias (solo selección)
            _dgvProvincias.SelectionChanged += (s, e) => OnProvinciaSelectionChanged();

            // ----- Eventos Localidades
            btnAgregarLoc.Click += (s, e) =>
            {
                if (_dgvProvincias.CurrentRow?.DataBoundItem is not Models.ProvinciaStats provStat)
                {
                    MessageBox.Show("Primero debe seleccionar una provincia.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var confirmar = MessageBox.Show(this,
                    $"¿Crear nueva localidad en '{provStat.Nombre}'?",
                    "Confirmar creación",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmar != DialogResult.Yes) return;

                var nuevaLocalidad = new Models.Localidad { ProvinciaId = provStat.Id, Activo = true };
                using var form = new ProvsUI.LocalidadEditForm(nuevaLocalidad);
                if (form.ShowDialog(this) == DialogResult.OK) CargarDatos();
            };
            btnEditarLoc.Click += (s, e) => EditarLocalidadSeleccionada();
            btnToggleActivoLoc.Click += (s, e) => ToggleActivoLocalidad();
            btnVerInactivas.Click += (s, e) =>
            {
                if (_dgvProvincias.CurrentRow?.DataBoundItem is not Models.ProvinciaStats prov) return;
                using var f = new LocalidadesInactivasForm(prov.Id, prov.Nombre);
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    OnProvinciaSelectionChanged(); // Refrescar localidades activas
                }
            };

            _dgvProvincias.CellFormatting += Grid_CellFormatting;
            _dgvLocalidades.CellFormatting += Grid_CellFormatting;

            CargarDatos();

            HandleCreated += (_, __) => BeginInvoke((Action)(() => InitSplit()));
            Resize += (_, __) => InitSplit();
        }

        // ----------------- Carga de datos -----------------
        private void CargarDatos()
        {
            CargarProvincias();
        }

        private void CargarProvincias()
        {
            int? currentId = _dgvProvincias.CurrentRow?.DataBoundItem is Models.ProvinciaStats p ? p.Id : (int?)null;

            // El listado de provincias siempre muestra todas (activas e inactivas)
            var data = Repository.ListarProvinciasConStats(true)
                       ?? new System.Collections.Generic.List<Models.ProvinciaStats>();
            _dgvProvincias.DataSource = data;

            // si había fila seleccionada, intenta restaurarla
            if (currentId.HasValue) SeleccionarPorId(_dgvProvincias, currentId.Value);
        }

        private void OnProvinciaSelectionChanged()
        {
            var provStat = _dgvProvincias.CurrentRow?.DataBoundItem as Models.ProvinciaStats;

            if (provStat != null)
            {
                _lblLocalidadesTitle.Text = $"Localidades — {provStat.Nombre}";
                // La grilla principal de localidades solo muestra las activas
                var data = Repository.ListarLocalidadesConStats(provStat.Id, false)
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

            var confirmar = MessageBox.Show(this,
                $"¿Modificar la localidad '{locStat.Nombre}'?",
                "Confirmar modificación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmar != DialogResult.Yes) return;

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
                var confirmar = MessageBox.Show(this,
                    $"¿Activar la localidad '{loc.Nombre}'?",
                    "Confirmar activación",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmar != DialogResult.Yes) return;

                Repository.SetLocalidadActiva(loc.Id, true);
            }
            else
            {
                var confirmar = MessageBox.Show(this,
                    $"¿Inactivar la localidad '{loc.Nombre}'?",
                    "Confirmar inactivación",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (confirmar != DialogResult.Yes) return;

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
            OnProvinciaSelectionChanged(); // Recargar para reflejar el cambio
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
            _dgvProvincias.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "Id", Name = "Id", Width = 60 });
            _dgvProvincias.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Nombre", HeaderText = "Nombre", Name = "Nombre", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _dgvProvincias.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CantClientes", HeaderText = "Clientes", Name = "CantClientes", Width = 80 });
            _dgvProvincias.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CantVentas", HeaderText = "Ventas", Name = "CantVentas", Width = 80 });
        }

        private void SetupLocalidadesColumns()
        {
            _dgvLocalidades.Columns.Clear();
            _dgvLocalidades.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "Id", Name = "Id", Width = 60 });
            _dgvLocalidades.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Nombre", HeaderText = "Nombre", Name = "Nombre", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _dgvLocalidades.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CantClientes", HeaderText = "Clientes", Name = "CantClientes", Width = 90 });
            _dgvLocalidades.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CantVentas", HeaderText = "Ventas", Name = "CantVentas", Width = 90 });
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
            var colActivo = FindColumn(_dgvLocalidades, "Activo");
            if (colActivo != null) colActivo.Visible = false;

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

        private void InitSplit()
        {
            if (_splitContainer == null || !_splitContainer.IsHandleCreated) return;

            int w = _splitContainer.ClientSize.Width;
            if (w <= 0) return;

            int splitter = _splitContainer.SplitterWidth;
            int p1Min = 220;            // deseado panel izquierdo
            int p2MinDesired = 420;     // deseado panel derecho

            // Asegurar que entren los dos paneles + splitter
            int minRequired = p1Min + p2MinDesired + splitter;
            int p2Min = p2MinDesired;

            if (w < minRequired)
            {
                // Reducir el mínimo del panel derecho para que no explote
                p2Min = Math.Max(120, w - p1Min - splitter);
            }

            _splitContainer.Panel1MinSize = p1Min;
            _splitContainer.Panel2MinSize = Math.Max(0, p2Min);

            // Calcular distancia deseada con ratio y "clamp"
            int desired = (int)Math.Round(w * 0.62);
            int min = _splitContainer.Panel1MinSize;
            int max = w - _splitContainer.Panel2MinSize - splitter;
            int safe = Math.Max(min, Math.Min(desired, Math.Max(min, max)));

            if (safe >= 0 && safe < w) _splitContainer.SplitterDistance = safe;
        }
    }
}
