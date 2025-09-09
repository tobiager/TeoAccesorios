using System;
using System.Linq;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class ClientesForm : Form
    {
        private readonly CheckBox chkInactivos = new CheckBox { Text = "Ver inactivos" };
        private readonly BindingSource bs = new BindingSource();
        private readonly DataGridView grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AutoGenerateColumns = true,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect
        };

        public ClientesForm()
        {
            Text = "Clientes";
            Width = 800;
            Height = 500;
            StartPosition = FormStartPosition.CenterParent;

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(8) };
            var btnNuevo = new Button { Text = "Nuevo" };
            var btnEditar = new Button { Text = "Editar" };
            var btnEliminar = new Button { Text = "Eliminar" };
            var btnRestaurar = new Button { Text = "Restaurar" };

            // === Handlers ===
            btnNuevo.Click += (s, e) =>
            {
                var cli = new Cliente { Activo = true };
                using (var f = new ClienteEditForm(cli))
                {
                    if (f.ShowDialog(this) == DialogResult.OK)
                    {
                        try
                        {
                            var id = Repository.InsertarCliente(cli);
                            cli.Id = id;
                            LoadData();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(this, "No se pudo crear el cliente.\n" + ex.Message,
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            };

            btnEditar.Click += (s, e) =>
            {
                var sel = GetSeleccionado();
                if (sel == null) return;

                var tmp = new Cliente
                {
                    Id = sel.Id,
                    Nombre = sel.Nombre,
                    Email = sel.Email,
                    Telefono = sel.Telefono,
                    Direccion = sel.Direccion,
                    Localidad = sel.Localidad,
                    Provincia = sel.Provincia,
                    Activo = sel.Activo
                };

                using (var f = new ClienteEditForm(tmp))
                {
                    if (f.ShowDialog(this) == DialogResult.OK)
                    {
                        try
                        {
                            Repository.ActualizarCliente(tmp);
                            LoadData();
                            SeleccionarPorId(tmp.Id);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(this, "No se pudo actualizar el cliente.\n" + ex.Message,
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            };

            btnEliminar.Click += (s, e) =>
            {
                var sel = GetSeleccionado();
                if (sel == null) return;

                if (MessageBox.Show(this, "�Inactivar al cliente \"" + sel.Nombre + "\"?",
                        "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

                try
                {
                    Repository.EliminarCliente(sel.Id); // Activo = 0
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "No se pudo inactivar el cliente.\n" + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnRestaurar.Click += (s, e) =>
            {
                var sel = GetSeleccionado();
                if (sel == null) return;

                try
                {
                    sel.Activo = true;
                    Repository.ActualizarCliente(sel); // set Activo=1
                    LoadData();
                    SeleccionarPorId(sel.Id);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "No se pudo restaurar el cliente.\n" + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            chkInactivos.CheckedChanged += (s, e) => LoadData();

            // UI
            top.Controls.Add(btnNuevo);
            top.Controls.Add(btnEditar);
            top.Controls.Add(btnEliminar);
            top.Controls.Add(btnRestaurar);
            top.Controls.Add(chkInactivos);

            Controls.Add(grid);
            Controls.Add(top);
            GridHelper.Estilizar(grid);
            GridHelperLock.SoloLectura(grid);
            GridHelperLock.WireDataBindingLock(grid);
            LoadData();
        }

        private Cliente GetSeleccionado()
        {
            return grid.CurrentRow != null ? grid.CurrentRow.DataBoundItem as Cliente : null;
        }

        private void SeleccionarPorId(int id)
        {
            foreach (DataGridViewRow row in grid.Rows)
            {
                var c = row.DataBoundItem as Cliente;
                if (c != null && c.Id == id)
                {
                    row.Selected = true;
                    grid.CurrentCell = row.Cells[0];
                    break;
                }
            }
        }

        private void LoadData()
        {
            var list = Repository.ListarClientes(chkInactivos.Checked);
            bs.DataSource = list;
            grid.DataSource = bs;
        }
    }
}
