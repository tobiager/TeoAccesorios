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
            var btnEliminar = new Button { Text = "Eliminar" }; // Agregar botón eliminar
            var btnRestablecerPass = new Button { Text = "Restablecer contraseña" };
            var btnVerInactivos = new Button { Text = "Ver inactivos" };

            top.Controls.AddRange(new Control[] { btnNuevo, btnEditar, btnEliminar, btnRestablecerPass, btnVerInactivos });

            btnRestablecerPass.Visible = Sesion.Rol == RolUsuario.Gerente;

            Controls.Add(grid);
            Controls.Add(top);
            grid.DataSource = bs;
            GridHelper.Estilizar(grid);
            GridHelperLock.Apply(grid);

            // Configurar columnas después de cargar datos
            grid.DataBindingComplete += (s, e) =>
            {
                // Ocultar la columna Contrasenia original
                var colContrasenia = grid.Columns["Contrasenia"];
                if (colContrasenia != null)
                {
                    colContrasenia.Visible = false;
                }

                // Ocultar columna Activo
                var colActivo = grid.Columns["Activo"];
                if (colActivo != null)
                {
                    colActivo.Visible = false;
                }

                // Agregar columna personalizada para el estado de contraseña
                if (grid.Columns["ContraseniaEstado"] == null)
                {
                    var colEstado = new DataGridViewTextBoxColumn
                    {
                        Name = "ContraseniaEstado",
                        HeaderText = "Estado Contraseña",
                        ReadOnly = true,
                        AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                    };
                    grid.Columns.Add(colEstado);

                    // Llenar los valores de la nueva columna
                    foreach (DataGridViewRow row in grid.Rows)
                    {
                        if (row.DataBoundItem is Usuario usuario)
                        {
                            row.Cells["ContraseniaEstado"].Value = usuario.ContraseniaEstado;
                            
                            // Opcional: cambiar el color de fondo para usuarios con contraseña por defecto
                            if (usuario.Contrasenia == "default123")
                            {
                                row.Cells["ContraseniaEstado"].Style.BackColor = Color.LightYellow;
                                row.Cells["ContraseniaEstado"].Style.ForeColor = Color.DarkOrange;
                            }
                        }
                    }
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

            // NUEVO: Implementar eliminación con restricciones
            btnEliminar.Click += (s, e) =>
            {
                if (grid.CurrentRow == null || !(grid.CurrentRow.DataBoundItem is Usuario sel)) return;

                // Verificar si es gerente
                bool esGerente = sel.Rol?.Equals("Gerente", StringComparison.OrdinalIgnoreCase) ?? false;
                if (esGerente)
                {
                    MessageBox.Show("No se puede eliminar al usuario con rol Gerente.", "Restricción de gerente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Verificar permisos para eliminar
                if (Sesion.Rol != RolUsuario.Gerente && Sesion.Rol != RolUsuario.Admin)
                {
                    MessageBox.Show("No tiene permisos para eliminar usuarios.", "Permiso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Confirmación
                var confirm = MessageBox.Show(
                    $"¿Está seguro de que desea eliminar al usuario '{sel.NombreUsuario}'?",
                    "Confirmar eliminación",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm == DialogResult.Yes)
                {
                    // Realizar soft delete (desactivar usuario)
                    Repository.SetUsuarioActivo(sel.Id, false);
                    MessageBox.Show("Usuario eliminado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                }
            };

            btnRestablecerPass.Click += (s, e) =>
            {
                if (grid.CurrentRow == null || !(grid.CurrentRow.DataBoundItem is Usuario sel)) return;

                if (sel.Id == Sesion.UsuarioId)
                {
                    MessageBox.Show("No puede restablecer su propia contraseña desde esta pantalla. Use la opción 'Cambiar contraseña' en el menú principal.", "Acción no permitida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var confirm = MessageBox.Show(
                    $"¿Está seguro de que desea restablecer la contraseña del usuario '{sel.NombreUsuario}'?\n\nLa nueva contraseña será: default123",
                    "Confirmar restablecimiento",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm == DialogResult.Yes)
                {
                    Db.Exec(@"
                        UPDATE dbo.Usuarios SET contrasenia = @p WHERE Id = @id;",
                        new SqlParameter("@p", "default123"),
                        new SqlParameter("@id", sel.Id)
                    );

                    MessageBox.Show("La contraseña ha sido restablecida.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                }
            };

            // === EDITAR - Actualizar a tabla real dbo.Usuarios, PK = Id
            btnEditar.Click += (s, e) =>
            {
                if (grid.CurrentRow == null || !(grid.CurrentRow.DataBoundItem is Usuario sel)) return;

                // --- Validaciones de permisos por rol ---
                bool esGerente = sel.Rol?.Equals("Gerente", StringComparison.OrdinalIgnoreCase) ?? false;
                if (esGerente && Sesion.Rol != RolUsuario.Gerente)
                {
                    MessageBox.Show("Solo un Gerente puede editar a otro usuario con rol Gerente.", "Permiso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

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
