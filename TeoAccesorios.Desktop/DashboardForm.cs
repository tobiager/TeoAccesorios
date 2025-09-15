using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Data.SqlClient; 

namespace TeoAccesorios.Desktop
{
    public class DashboardForm : Form
    {
        private Panel side;
        private Panel header;
        private Panel content;

        public DashboardForm()
        {
            Text = "TeoAccesorios — " + Sesion.Rol.ToString();
            Width = 1100; Height = 720; StartPosition = FormStartPosition.CenterScreen;

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            side = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(18, 22, 35), Padding = new Padding(12) };
            Label brand = new Label { Text = "TeoAccesorios", ForeColor = Color.White, Font = new Font("Segoe UI", 16, FontStyle.Bold), Dock = DockStyle.Top, Height = 36 };
            side.Controls.Add(brand);

            Button Btn(string txt, EventHandler onClick)
            {
                var b = new Button { Text = txt, Dock = DockStyle.Top, Height = 38, Margin = new Padding(0, 8, 0, 0) };
                b.FlatStyle = FlatStyle.Flat; b.FlatAppearance.BorderSize = 0;
                b.BackColor = Color.FromArgb(14, 165, 233); b.ForeColor = Color.White;
                b.Click += onClick;
                return b;
            }

            
            var btnCerrarSesion = Btn("Cerrar sesión", (_, __) => { Hide(); using var l = new LoginForm(); l.ShowDialog(this); Close(); });
            var btnInicio = Btn("Inicio (Dashboard)", (_, __) => ShowKpis());
            var btnReportes = Btn("Reportes", (_, __) => ShowInContent(new ReportesForm()));
            var btnEmpleados = Btn("Empleados", (_, __) => ShowInContent(new UsuariosForm()));
            var btnClientes = Btn("Clientes", (_, __) => ShowInContent(new ClientesForm()));
            var btnCategorias = Btn("Categorías", (_, __) => ShowInContent(new CategoriasForm()));
            var btnProductos = Btn("Productos", (_, __) => ShowInContent(new ProductosForm()));
            var btnVerVentas = Btn("Ver Ventas", (_, __) => ShowInContent(new VentasForm()));
            var btnNuevaVenta = Btn("Nueva Venta", (_, __) => ShowInContent(new NuevaVentaForm()));

           
            Button? btnBackup = null;
            if (Sesion.Rol == RolUsuario.Admin)
                btnBackup = Btn("Backup BD", (_, __) => DoBackup());

            
            side.Controls.Add(btnNuevaVenta);
            side.Controls.Add(btnVerVentas);
            side.Controls.Add(btnProductos);
            side.Controls.Add(btnCategorias);
            side.Controls.Add(btnClientes);
            if (Sesion.Rol == RolUsuario.Admin) side.Controls.Add(btnEmpleados);
            side.Controls.Add(btnReportes);
            side.Controls.Add(btnInicio);
            if (btnBackup != null) side.Controls.Add(btnBackup);
            side.Controls.Add(btnCerrarSesion);                  

            header = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(10, 14, 28), Padding = new Padding(12) };
            var lblUser = new Label
            {
                Text = $"Usuario: {Sesion.Usuario}  —  Rol: {Sesion.Rol}",
                ForeColor = Color.White,
                Dock = DockStyle.Left,
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            header.Controls.Add(lblUser);

            content = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(24, 28, 44) };

            root.Controls.Add(header, 0, 0); root.SetColumnSpan(header, 2);
            root.Controls.Add(side, 0, 1);
            root.Controls.Add(content, 1, 1);
            Controls.Add(root);

            ShowKpis();
        }

        private void ShowKpis()
        {
            content.Controls.Clear();
            var kpi = new KPIsView { Dock = DockStyle.Fill };
            content.Controls.Add(kpi);
        }

        private void ShowInContent(Form f)
        {
            content.Controls.Clear();
            f.TopLevel = false;
            f.FormBorderStyle = FormBorderStyle.None;
            f.Dock = DockStyle.Fill;
            content.Controls.Add(f);
            f.Show();
        }

        // ---------- BACKUP (automático a C:\Backups) ----------
        private void DoBackup()
        {
            // Seguridad: sólo Admin ejecuta
            if (Sesion.Rol != RolUsuario.Admin)
            {
                MessageBox.Show("Sólo un administrador puede realizar backups.", "Acceso denegado",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using var conn = new SqlConnection(Db.ConnectionString);
                conn.Open();

                
                var csb = new SqlConnectionStringBuilder(conn.ConnectionString);
                var dbName = string.IsNullOrWhiteSpace(csb.InitialCatalog) ? "TeoAccesorios" : csb.InitialCatalog;

                // Carpeta fija (del lado del servidor SQL)
                var folder = @"C:\Backups";
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder); 

                var filePath = Path.Combine(folder, $"{dbName}_{DateTime.Now:yyyyMMdd_HHmmss}.bak");
                var target = filePath.Replace("'", "''");

                var sql = $@"
                    BACKUP DATABASE [{dbName}]
                    TO DISK = '{target}'
                    WITH FORMAT, INIT, NAME = 'Backup {dbName}',
                         SKIP, NOREWIND, NOUNLOAD, STATS = 10, COMPRESSION;";

                using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 0 };
                cmd.ExecuteNonQuery();

                MessageBox.Show($"✅ Backup creado en:\n{filePath}\n\n" +
                                "Nota: la ruta es accesible para el SERVICIO de SQL Server.",
                                "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (UnauthorizedAccessException uaex)
            {
                MessageBox.Show("Permisos insuficientes para escribir en C:\\Backups.\n" +
                                "Dale permisos de escritura a la cuenta del servicio de SQL Server (p. ej. NT SERVICE\\MSSQLSERVER).\n\n" +
                                uaex.Message, "Permisos insuficientes",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (SqlException sqlex)
            {
                MessageBox.Show("SQL Server rechazó la operación de backup.\n" +
                                "• Verificá permisos BACKUP DATABASE.\n" +
                                "• Asegurá que C:\\Backups existe en el SERVIDOR SQL y el servicio tiene escritura.\n\n" +
                                sqlex.Message, "Error SQL",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al hacer backup: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
