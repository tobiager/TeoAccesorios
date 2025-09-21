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
            top.Controls.AddRange(new Control[] { btnNuevo, btnEditar });

            Controls.Add(grid);
            Controls.Add(top);
            grid.DataSource = bs;
            GridHelper.Estilizar(grid);
            GridHelperLock.SoloLectura(grid);
            GridHelperLock.WireDataBindingLock(grid);


            
            grid.DefaultCellStyle.WrapMode = DataGridViewTriState.False;

           
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 11F);

           
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;


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
           
            var dt = Db.Query(@"
                SELECT  id_usuario     AS Id,
                        nombreUsuario  AS NombreUsuario,
                        correo         AS Correo,
                        contrasenia    AS Contrasenia,
                        rol            AS Rol,
                        activo         AS Activo
                FROM dbo.usuario", Array.Empty<SqlParameter>());

            bs.DataSource = dt.AsEnumerable().Select(r => new Usuario
            {
                Id = r.Field<int>("Id"),
                NombreUsuario = r.Field<string>("NombreUsuario") ?? "",
                Correo = r.Field<string>("Correo") ?? "",
                Contrasenia = r.Field<string>("Contrasenia") ?? "",
                Rol = r.Field<string>("Rol") ?? "",
                Activo = r.Field<bool>("Activo")
            }).ToList();
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
