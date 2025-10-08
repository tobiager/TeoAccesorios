using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class UsuariosForm : Form
    {
        private readonly BindingSource bs = new BindingSource();
        private readonly DataGridView grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AutoGenerateColumns = true,

           
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,        
            AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells,    

            RowHeadersVisible = false,          
            AllowUserToAddRows = false,        
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
            EnableHeadersVisualStyles = false
        };

        public UsuariosForm()
        {
            Text = "Usuarios";
            Width = 900;
            Height = 480;
            StartPosition = FormStartPosition.CenterParent;

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(8) };
            var btnNuevo = new Button { Text = "Nuevo" };
            var btnEditar = new Button { Text = "Editar" };
            var btnVerInactivos = new Button { Text = "Ver inactivos" };

            top.Controls.AddRange(new Control[] { btnNuevo, btnEditar, btnVerInactivos });

            Controls.Add(grid);
            Controls.Add(top);
            grid.DataSource = bs;
            GridHelper.Estilizar(grid); // Aplicar estilo
            GridHelperLock.Apply(grid);

            // Ocultar columna Activo después de cargar datos
            grid.DataBindingComplete += (s, e) =>
            {
                var colActivo = grid.Columns["Activo"];
                if (colActivo != null)
                {
                    colActivo.Visible = false;
                }
            };

            // === NUEVO - Agregar a tabla real dbo.Usuarios
            btnNuevo.Click += (s, e) =>
            {
                var u = new Usuario();
                using (var f = new UsuarioEditForm(u))
                {
                    if (f.ShowDialog(this) == DialogResult.OK)
                    {
                        var res = f.Result;

                        using (var cn = new SqlConnection(Db.ConnectionString))
                        using (var cmd = new SqlCommand(@"
                            INSERT INTO dbo.Usuarios (NombreUsuario, correo, contrasenia, rol, Activo)
                            VALUES (@n,@c,@p,@r,@a);", cn))
                        {
                            cmd.Parameters.AddWithValue("@n", res.NombreUsuario ?? "");
                            cmd.Parameters.AddWithValue("@c", res.Correo ?? "");
                            cmd.Parameters.AddWithValue("@p", res.Contrasenia ?? "");
                            cmd.Parameters.AddWithValue("@r", string.IsNullOrWhiteSpace(res.Rol) ? "Vendedor" : res.Rol);
                            cmd.Parameters.AddWithValue("@a", res.Activo);
                            cn.Open();
                            cmd.ExecuteNonQuery();
                        }
                        LoadData();
                    }
                }
            };

            btnVerInactivos.Click += (_, __) =>
            {
                using (var f = new UsuariosInactivosForm())
                {
                    f.ShowDialog(this);
                    LoadData();
                }
            };

            // === EDITAR - Actualizar a tabla real dbo.Usuarios, PK = Id
            btnEditar.Click += (s, e) =>
            {
                if (grid.CurrentRow == null || !(grid.CurrentRow.DataBoundItem is Usuario sel)) return;

                var tmp = new Usuario
                {
                    Id = sel.Id,
                    NombreUsuario = sel.NombreUsuario,
                    Correo = sel.Correo,
                    Contrasenia = sel.Contrasenia,
                    Rol = sel.Rol,
                    Activo = sel.Activo
                };

                using (var f = new UsuarioEditForm(tmp))
                {
                    if (f.ShowDialog(this) == DialogResult.OK)
                    {
                        var u = f.Result;

                        Db.Exec(@"
                            UPDATE dbo.Usuarios
                               SET NombreUsuario=@n,
                                   correo=@c,
                                   contrasenia=@p,
                                   rol=@r,
                                   Activo=@a
                             WHERE Id=@id;",
                            new SqlParameter("@id", u.Id),
                            new SqlParameter("@n", u.NombreUsuario ?? ""),
                            new SqlParameter("@c", u.Correo ?? ""),
                            new SqlParameter("@p", u.Contrasenia ?? ""),
                            new SqlParameter("@r", string.IsNullOrWhiteSpace(u.Rol) ? "Vendedor" : u.Rol),
                            new SqlParameter("@a", u.Activo)
                        );

                        LoadData();
                        
                        TrySelectRowById(u.Id);
                    }
                }
            };

            LoadData();
        }

        private void LoadData()
        {
            bs.DataSource = Repository.ListarUsuarios().Where(u => u.Activo).ToList();
        }

        private void TrySelectRowById(int id)
        {
            foreach (DataGridViewRow row in grid.Rows)
            {
                var u = row.DataBoundItem as Usuario;
                if (u != null && u.Id == id)
                {
                    row.Selected = true;
                    grid.CurrentCell = row.Cells[0];
                    break;
                }
            }
        }
    }
}
