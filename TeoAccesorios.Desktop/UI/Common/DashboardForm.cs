using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using WinFormsTimer = System.Windows.Forms.Timer;
using Microsoft.Data.SqlClient;

// Alias seguro al namespace de tus forms de provincias
using ProvsUI = TeoAccesorios.Desktop.UI.Provincias;
using UserUI = TeoAccesorios.Desktop.UI.Usuarios;
using StatsUI = TeoAccesorios.Desktop.UI.Estadisticas;
using AdminUI = TeoAccesorios.Desktop.UI.Common; 

namespace TeoAccesorios.Desktop
{
    public class DashboardForm : Form
    {
        private Panel side;
        private Panel header;
        private Panel content;

        // Estado visual navegación
        private Panel _indicador;
        private Button? _btnActivo;
        private readonly WinFormsTimer _anim = new WinFormsTimer { Interval = 10 };
        private int _targetTop, _targetHeight;

        public DashboardForm()
        {
            Text = "TeoAccesorios — " + Sesion.Rol.ToString();
            Width = 1100; Height = 720; StartPosition = FormStartPosition.CenterScreen;
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            side = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(18, 22, 35), Padding = new Padding(12) };
            Label brand = new Label { Text = "TeoAccesorios", ForeColor = Color.White, Font = new Font("Segoe UI", 16, FontStyle.Bold), Dock = DockStyle.Top, Height = 36 };
            side.Controls.Add(brand);

            _indicador = new Panel
            {
                Width = 5,
                Height = 38,
                BackColor = Color.White,
                Left = 12,
                Top = brand.Bottom + 8,
                Visible = true
            };
            side.Controls.Add(_indicador);
            _anim.Tick += (_, __) => AnimarIndicador();

            Button Btn(string txt, bool esRestringido = false)
            {
                var b = new Button
                {
                    Text = txt,
                    Dock = DockStyle.Top,
                    Height = 38,
                    Margin = new Padding(0, 8, 0, 0),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(16, 0, 0, 0),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = esRestringido ? Color.FromArgb(128, 128, 128) : Color.FromArgb(14, 165, 233),
                    ForeColor = esRestringido ? Color.FromArgb(180, 180, 180) : Color.White,
                    Enabled = !esRestringido
                };
                
                if (!esRestringido)
                {
                    b.FlatAppearance.BorderSize = 0;
                    b.FlatAppearance.MouseOverBackColor = Color.FromArgb(56, 189, 248);
                    b.FlatAppearance.MouseDownBackColor = Color.FromArgb(2, 132, 199);
                }
                else
                {
                    b.FlatAppearance.BorderSize = 0;
                    b.Cursor = Cursors.No;
                }
                
                return b;
            }

            // Determinar permisos basados en roles
            bool puedeAccederEmpleados = Sesion.Rol == RolUsuario.Gerente || Sesion.Rol == RolUsuario.Admin;
            bool puedeAccederEstadisticas = Sesion.Rol == RolUsuario.Gerente; // Solo Gerente puede acceder a estadísticas
            bool puedeHacerBackup = Sesion.Rol == RolUsuario.Gerente || Sesion.Rol == RolUsuario.Admin; // Admin y Gerente pueden hacer backup

            var btnCambiarContrasenia = Btn("Cambiar contraseña");
            var btnCerrarSesion = Btn("Cerrar sesión");
            var btnInicio = Btn("Inicio (Dashboard)");
            var btnReportes = Btn("Reportes");
            var btnEstadisticas = Btn("Estadísticas", !puedeAccederEstadisticas);
            var btnEmpleados = Btn("Empleados", !puedeAccederEmpleados);
            var btnClientes = Btn("Clientes");
            var btnCategorias = Btn("Categorías");

            // NUEVO: botón Provincias/Localidades
            var btnProvinciasLocalidades = Btn("Provincias/Localidades");

            var btnProductos = Btn("Productos");
            var btnVerVentas = Btn("Ver Ventas");
            var btnNuevaVenta = Btn("Nueva Venta");
            Button? btnBackup = null;
            if (puedeHacerBackup) btnBackup = Btn("Backup BD");

            void Nav(Button btn, Action action)
            {
                if (!btn.Enabled)
                {
                    MessageBox.Show("No tiene permisos para acceder a este módulo.", "Acceso denegado",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                ActivarBoton(btn);
                action();
            }

            void NavRestringido(Button btn, string mensaje)
            {
                MessageBox.Show(mensaje, "Acceso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            btnCerrarSesion.Click += (_, __) =>
            {
                Hide(); using var l = new LoginForm(); l.ShowDialog(this); Close();
            };

            btnInicio.Click += (_, __) => Nav(btnInicio, ShowKpis);
            btnReportes.Click += (_, __) => Nav(btnReportes, () => ShowInContent(new ReportesForm()));
            
            // Estadísticas: Solo Gerente puede acceder
            btnEstadisticas.Click += (_, __) =>
            {
                if (puedeAccederEstadisticas)
                    Nav(btnEstadisticas, () => ShowInContent(new StatsUI.EstadisticasForm()));
                else
                    NavRestringido(btnEstadisticas, "Solo el Gerente puede acceder al módulo de Estadísticas.");
            };
            
            // Empleados: Solo Gerente y Admin pueden acceder
            btnEmpleados.Click += (_, __) =>
            {
                if (puedeAccederEmpleados)
                    Nav(btnEmpleados, () => ShowInContent(new UsuariosForm()));
                else
                    NavRestringido(btnEmpleados, "Solo el Gerente y Administradores pueden acceder al módulo de Empleados.");
            };
            
            btnClientes.Click += (_, __) => Nav(btnClientes, () => ShowInContent(new ClientesForm()));
            btnCategorias.Click += (_, __) => Nav(btnCategorias, () => ShowInContent(new CategoriasForm()));

            // NUEVO: abre el administrador de provincias/localidades
            btnProvinciasLocalidades.Click += (_, __) =>
                Nav(btnProvinciasLocalidades, () => ShowInContent(new ProvsUI.ProvinciasLocalidadesForm()));

            btnProductos.Click += (_, __) => Nav(btnProductos, () => ShowInContent(new ProductosForm()));
            btnVerVentas.Click += (_, __) => Nav(btnVerVentas, () => ShowInContent(new VentasForm()));
            btnNuevaVenta.Click += (_, __) => Nav(btnNuevaVenta, () => ShowInContent(new NuevaVentaForm()));

            // Backup: Admin y Gerente pueden acceder al formulario profesional
            if (btnBackup != null) 
            {
                btnBackup.Click += (_, __) =>
                {
                    // Abrir el nuevo formulario de backup como diálogo modal
                    using var backupForm = new AdminUI.BackupForm();
                    backupForm.ShowDialog(this);
                };
            }

            // IMPORTANTE: el orden de Add con Dock=Top es inverso en pantalla (el último va más arriba).
            side.Controls.Add(btnNuevaVenta);
            side.Controls.Add(btnVerVentas);
            side.Controls.Add(btnProductos);
            side.Controls.Add(btnProvinciasLocalidades); // <— queda entre Categorías y Productos
            side.Controls.Add(btnCategorias);
            side.Controls.Add(btnClientes);
            side.Controls.Add(btnEmpleados); // Siempre agregarlo, se controla visualmente
            side.Controls.Add(btnEstadisticas); // Siempre agregarlo, se controla visualmente
            side.Controls.Add(btnReportes);
            side.Controls.Add(btnInicio);
            // REMOVIDO: side.Controls.Add(btnCambiarContrasenia);
            if (btnBackup != null) side.Controls.Add(btnBackup);
            side.Controls.Add(btnCerrarSesion);

            // HEADER CON FLOWLAYOUTPANEL PARA INCLUIR EL BOTÓN DE CAMBIAR CONTRASEÑA
            header = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(10, 14, 28), Padding = new Padding(12) };
            
            var flowPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = false,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom
            };

            var lblUser = new Label
            {
                Text = $"Usuario: {Sesion.Usuario}  —  Rol: {Sesion.Rol}",
                ForeColor = Color.White,
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 8, 20, 0)
            };

            var btnCambiarContraseniaHeader = new Button
            {
                Text = "🔑 Cambiar contraseña",
                AutoSize = true,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                Padding = new Padding(12, 4, 12, 4),
                Margin = new Padding(0, 4, 0, 0)
            };

            btnCambiarContraseniaHeader.FlatAppearance.BorderSize = 0;
            btnCambiarContraseniaHeader.FlatAppearance.MouseOverBackColor = Color.FromArgb(37, 99, 235);
            btnCambiarContraseniaHeader.FlatAppearance.MouseDownBackColor = Color.FromArgb(29, 78, 216);

            btnCambiarContraseniaHeader.Click += (_, __) =>
            {
                using var f = new UserUI.CambiarContraseniaForm();
                f.ShowDialog(this);
            };

            flowPanel.Controls.Add(lblUser);
            flowPanel.Controls.Add(btnCambiarContraseniaHeader);
            header.Controls.Add(flowPanel);

            content = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(24, 28, 44) };

            root.Controls.Add(header, 0, 0); root.SetColumnSpan(header, 2);
            root.Controls.Add(side, 0, 1);
            root.Controls.Add(content, 1, 1);
            Controls.Add(root);

            ShowKpis();
            ActivarBoton(btnInicio, animar: false);
        }

        private void ActivarBoton(Button btn, bool animar = true)
        {
            if (_btnActivo != null)
            {
                _btnActivo.BackColor = Color.FromArgb(14, 165, 233);
                _btnActivo.ForeColor = Color.White;
                _btnActivo.FlatAppearance.BorderSize = 0;
            }

            _btnActivo = btn;
            _btnActivo.BackColor = Color.White;
            _btnActivo.ForeColor = Color.Black;

            _targetTop = btn.Top;
            _targetHeight = btn.Height;

            if (!animar)
            {
                _indicador.Top = _targetTop;
                _indicador.Height = _targetHeight;
            }
            else
            {
                _anim.Start();
            }
        }

        private void AnimarIndicador()
        {
            int dy = _targetTop - _indicador.Top;
            int dh = _targetHeight - _indicador.Height;

            _indicador.Top += (int)Math.Ceiling(dy * 0.25);
            _indicador.Height += (int)Math.Ceiling(dh * 0.25);

            if (Math.Abs(dy) < 2 && Math.Abs(dh) < 2)
            {
                _indicador.Top = _targetTop;
                _indicador.Height = _targetHeight;
                _anim.Stop();
            }
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

        // BACKUP (método legacy mantenido como fallback) - Admin y Gerente
        private void DoBackup()
        {
            if (Sesion.Rol != RolUsuario.Gerente && Sesion.Rol != RolUsuario.Admin)
            {
                MessageBox.Show("Solo el Gerente y Administradores pueden realizar backups.", "Acceso denegado",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using var conn = new SqlConnection(Db.ConnectionString);
                conn.Open();

                var csb = new SqlConnectionStringBuilder(conn.ConnectionString);
                var dbName = string.IsNullOrWhiteSpace(csb.InitialCatalog) ? "TeoAccesorios" : csb.InitialCatalog;

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

                MessageBox.Show($" Backup creado en:\n{filePath}\n\n" +
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
