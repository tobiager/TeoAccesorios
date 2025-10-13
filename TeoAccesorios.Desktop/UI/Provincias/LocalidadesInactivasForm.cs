using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop.UI.Provincias
{
    public class LocalidadesInactivasForm : Form
    {
        private readonly int _provinciaId;
        private readonly BindingSource _bs = new();
        private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false };
        private readonly Button _btnRestaurar = new() { Text = "Restaurar" };
        private readonly Button _btnCerrar = new() { Text = "Cerrar" };

        public LocalidadesInactivasForm(int provinciaId, string provinciaNombre)
        {
            _provinciaId = provinciaId;
            Text = $"Localidades inactivas — {provinciaNombre}";
            Width = 820;
            Height = 480;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimizeBox = false;

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8), WrapContents = false };
            top.Controls.AddRange(new Control[] { _btnRestaurar, _btnCerrar });

            SetupGridColumns();
            _grid.DataSource = _bs;

            Controls.Add(_grid);
            Controls.Add(top);

            try { GridHelper.Estilizar(_grid); } catch { /* Ignored in designer */ }

            Load += (_, __) => RefreshData();
            _btnCerrar.Click += (_, __) => Close();
            _btnRestaurar.Click += (_, __) => RestaurarLocalidadSeleccionada();
        }

        private void SetupGridColumns()
        {
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "Id", Width = 60 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Nombre", HeaderText = "Nombre", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            // ProvinciaNombre no está en LocalidadStats, pero podemos asumirlo o dejarlo fuera. Lo omitiré por ahora para que compile.
            // _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProvinciaNombre", HeaderText = "Provincia", Width = 160 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CantClientes", HeaderText = "Clientes", Width = 90 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CantVentas", HeaderText = "Ventas", Width = 90 });
        }

        private void RefreshData()
        {
            var inactivas = Repository.ListarLocalidadesConStats(_provinciaId, true)
                                      .Where(l => l.Activo == false)
                                      .ToList();
            _bs.DataSource = inactivas;
            _bs.ResetBindings(false);

            _btnRestaurar.Enabled = inactivas.Any();
        }

        private void RestaurarLocalidadSeleccionada()
        {
            if (_bs.Current is not LocalidadStats loc)
            {
                MessageBox.Show("Seleccione una localidad para restaurar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show($"¿Desea restaurar la localidad '{loc.Nombre}'?", "Confirmar restauración", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    Repository.SetLocalidadActiva(loc.Id, true);
                    DialogResult = DialogResult.OK; // Señal para que el form principal refresque
                    RefreshData(); // Refresca la lista de inactivas
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ocurrió un error al restaurar la localidad:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}