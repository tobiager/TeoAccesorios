using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using System.ComponentModel;
using System.Threading.Tasks;

namespace TeoAccesorios.Desktop.UI.Common
{
    public class BackupForm : Form
    {
        private readonly GroupBox grpOpciones = new() { Text = "Opciones de Backup", Dock = DockStyle.Top, Height = 200, Padding = new Padding(10) };
        private readonly GroupBox grpProgreso = new() { Text = "Progreso", Dock = DockStyle.Fill, Padding = new Padding(10) };
        
        // Controles de opciones
        private readonly RadioButton rbRutaPredeterminada = new() { Text = "Ruta predeterminada (C:\\Backups)", Checked = true, AutoSize = true };
        private readonly RadioButton rbRutaPersonalizada = new() { Text = "Ruta personalizada:", AutoSize = true };
        private readonly TextBox txtRutaPersonalizada = new() { Enabled = false, Width = 300 };
        private readonly Button btnExaminar = new() { Text = "Examinar...", Enabled = false, Width = 80 };
        
        private readonly CheckBox chkCompresion = new() { Text = "Usar compresión", Checked = true, AutoSize = true };
        private readonly CheckBox chkVerificarBackup = new() { Text = "Verificar integridad del backup", Checked = true, AutoSize = true };
        private readonly CheckBox chkSobreescribir = new() { Text = "Sobreescribir archivo existente", Checked = true, AutoSize = true };
        
        private readonly Label lblNombreArchivo = new() { Text = "Nombre del archivo:", AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        private readonly TextBox txtNombreArchivo = new() { Width = 300 };
        
        // Controles de progreso
        private readonly ProgressBar progressBar = new() { Dock = DockStyle.Top, Height = 25, Style = ProgressBarStyle.Marquee, Visible = false };
        private readonly RichTextBox txtLog = new() { Dock = DockStyle.Fill, ReadOnly = true, Font = new Font("Consolas", 9) };
        
        // Controles de acción
        private readonly Button btnIniciarBackup = new() { Text = "🗄️ Iniciar Backup", Width = 120, Height = 35, BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        private readonly Button btnCancelar = new() { Text = "Cancelar", Width = 80, Height = 35, BackColor = Color.FromArgb(108, 117, 125), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        private readonly Button btnLimpiarLog = new() { Text = "Limpiar Log", Width = 100, Height = 35, BackColor = Color.FromArgb(255, 193, 7), ForeColor = Color.Black, FlatStyle = FlatStyle.Flat };

        private BackgroundWorker? backgroundWorker;
        private bool operacionEnProgreso = false;

        public BackupForm()
        {
            InitializeComponent();
            ConfigurarEventos();
            ConfigurarPermisos();
            GenerarNombreArchivoPredeterminado();
            AgregarLogInicial();
        }

        private void InitializeComponent()
        {
            Text = "🗄️ Backup de Base de Datos";
            Size = new Size(700, 550);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = true;
            
            // Configurar botones
            btnIniciarBackup.FlatAppearance.BorderSize = 0;
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnLimpiarLog.FlatAppearance.BorderSize = 0;

            // Layout principal
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                Padding = new Padding(10)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 220));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

            // Configurar grupo de opciones
            ConfigurarGrupoOpciones();
            
            // Configurar grupo de progreso
            ConfigurarGrupoProgreso();

            // Panel de botones
            var panelBotones = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 10, 0, 0)
            };
            panelBotones.Controls.AddRange(new Control[] { btnCancelar, btnIniciarBackup, btnLimpiarLog });

            mainLayout.Controls.Add(grpOpciones, 0, 0);
            mainLayout.Controls.Add(grpProgreso, 0, 1);
            mainLayout.Controls.Add(panelBotones, 0, 2);

            Controls.Add(mainLayout);
        }

        private void ConfigurarGrupoOpciones()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 6,
                Padding = new Padding(5)
            };
            
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            // Fila 0: Ruta predeterminada
            layout.Controls.Add(rbRutaPredeterminada, 0, 0);
            layout.SetColumnSpan(rbRutaPredeterminada, 3);

            // Fila 1: Ruta personalizada
            layout.Controls.Add(rbRutaPersonalizada, 0, 1);
            layout.Controls.Add(txtRutaPersonalizada, 1, 1);
            layout.Controls.Add(btnExaminar, 2, 1);

            // Fila 2: Nombre de archivo
            layout.Controls.Add(lblNombreArchivo, 0, 2);
            layout.Controls.Add(txtNombreArchivo, 1, 2);
            layout.SetColumnSpan(txtNombreArchivo, 2);

            // Fila 3: Opciones adicionales
            layout.Controls.Add(chkCompresion, 0, 3);
            layout.SetColumnSpan(chkCompresion, 3);

            layout.Controls.Add(chkVerificarBackup, 0, 4);
            layout.SetColumnSpan(chkVerificarBackup, 3);

            layout.Controls.Add(chkSobreescribir, 0, 5);
            layout.SetColumnSpan(chkSobreescribir, 3);

            grpOpciones.Controls.Add(layout);
        }

        private void ConfigurarGrupoProgreso()
        {
            var layoutProgreso = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2
            };
            layoutProgreso.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            layoutProgreso.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            layoutProgreso.Controls.Add(progressBar, 0, 0);
            layoutProgreso.Controls.Add(txtLog, 0, 1);

            grpProgreso.Controls.Add(layoutProgreso);
        }

        private void ConfigurarEventos()
        {
            rbRutaPredeterminada.CheckedChanged += (s, e) =>
            {
                txtRutaPersonalizada.Enabled = !rbRutaPredeterminada.Checked;
                btnExaminar.Enabled = !rbRutaPredeterminada.Checked;
            };

            rbRutaPersonalizada.CheckedChanged += (s, e) =>
            {
                txtRutaPersonalizada.Enabled = rbRutaPersonalizada.Checked;
                btnExaminar.Enabled = rbRutaPersonalizada.Checked;
                if (rbRutaPersonalizada.Checked && string.IsNullOrWhiteSpace(txtRutaPersonalizada.Text))
                {
                    txtRutaPersonalizada.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                }
            };

            btnExaminar.Click += (s, e) =>
            {
                using var folderDialog = new FolderBrowserDialog
                {
                    Description = "Seleccione la carpeta donde guardar el backup",
                    ShowNewFolderButton = true,
                    SelectedPath = txtRutaPersonalizada.Text
                };

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtRutaPersonalizada.Text = folderDialog.SelectedPath;
                }
            };

            btnIniciarBackup.Click += async (s, e) => await IniciarBackup();
            btnCancelar.Click += (s, e) => Close();
            btnLimpiarLog.Click += (s, e) =>
            {
                txtLog.Clear();
                AgregarLogInicial();
            };

            FormClosing += (s, e) =>
            {
                if (operacionEnProgreso)
                {
                    var result = MessageBox.Show(
                        "Hay un backup en progreso. ¿Está seguro de que desea cancelar?",
                        "Operación en progreso",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.No)
                    {
                        e.Cancel = true;
                        return;
                    }

                    backgroundWorker?.CancelAsync();
                }
            };
        }

        private void ConfigurarPermisos()
        {
            // Verificar permisos de acceso
            if (Sesion.Rol != RolUsuario.Admin && Sesion.Rol != RolUsuario.Gerente)
            {
                MessageBox.Show(
                    "No tiene permisos para acceder a la funcionalidad de backup.\nSolo Administradores y Gerentes pueden realizar backups.",
                    "Acceso denegado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                Close();
                return;
            }

            // Mostrar información de permisos en el log
            var rolTexto = Sesion.Rol == RolUsuario.Gerente ? "Gerente" : "Administrador";
            AgregarLog($"✅ Acceso autorizado como {rolTexto}: {Sesion.Usuario}", Color.Green);
        }

        private void GenerarNombreArchivoPredeterminado()
        {
            try
            {
                using var conn = new SqlConnection(Db.ConnectionString);
                var csb = new SqlConnectionStringBuilder(conn.ConnectionString);
                var dbName = string.IsNullOrWhiteSpace(csb.InitialCatalog) ? "TeoAccesorios" : csb.InitialCatalog;
                
                txtNombreArchivo.Text = $"{dbName}_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
            }
            catch
            {
                txtNombreArchivo.Text = $"TeoAccesorios_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
            }
        }

        private void AgregarLogInicial()
        {
            AgregarLog("🗄️ Sistema de Backup de Base de Datos - TeoAccesorios", Color.Blue);
            AgregarLog($"📅 Sesión iniciada: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", Color.Gray);
            AgregarLog($"👤 Usuario: {Sesion.Usuario} ({Sesion.Rol})", Color.Gray);
            AgregarLog("", Color.Black);
            AgregarLog("ℹ️ Configurar las opciones de backup y presionar 'Iniciar Backup' para comenzar.", Color.DarkBlue);
            AgregarLog("", Color.Black);
        }

        private void AgregarLog(string mensaje, Color color)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() => AgregarLog(mensaje, color)));
                return;
            }

            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.SelectionLength = 0;
            txtLog.SelectionColor = color;
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {mensaje}\n");
            txtLog.SelectionColor = txtLog.ForeColor;
            txtLog.ScrollToCaret();
        }

        private async Task IniciarBackup()
        {
            if (operacionEnProgreso)
            {
                MessageBox.Show("Ya hay un backup en progreso.", "Backup en curso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Validaciones
            if (string.IsNullOrWhiteSpace(txtNombreArchivo.Text))
            {
                MessageBox.Show("Debe especificar un nombre para el archivo de backup.", "Nombre requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombreArchivo.Focus();
                return;
            }

            if (rbRutaPersonalizada.Checked && string.IsNullOrWhiteSpace(txtRutaPersonalizada.Text))
            {
                MessageBox.Show("Debe especificar una ruta personalizada.", "Ruta requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRutaPersonalizada.Focus();
                return;
            }

            // Determinar ruta final
            string rutaDestino;
            if (rbRutaPredeterminada.Checked)
            {
                rutaDestino = @"C:\Backups";
            }
            else
            {
                rutaDestino = txtRutaPersonalizada.Text.Trim();
            }

            // Crear directorio si no existe
            try
            {
                if (!Directory.Exists(rutaDestino))
                {
                    Directory.CreateDirectory(rutaDestino);
                    AgregarLog($"📁 Directorio creado: {rutaDestino}", Color.Green);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al crear el directorio de destino:\n{ex.Message}", "Error de directorio", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var archivoCompleto = Path.Combine(rutaDestino, txtNombreArchivo.Text);

            // Verificar si el archivo existe
            if (File.Exists(archivoCompleto) && !chkSobreescribir.Checked)
            {
                var result = MessageBox.Show(
                    $"El archivo '{txtNombreArchivo.Text}' ya existe.\n¿Desea sobreescribirlo?",
                    "Archivo existente",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No)
                    return;
            }

            // Configurar UI para operación en progreso
            operacionEnProgreso = true;
            btnIniciarBackup.Enabled = false;
            progressBar.Visible = true;
            progressBar.Style = ProgressBarStyle.Marquee;

            AgregarLog("🚀 Iniciando proceso de backup...", Color.Blue);
            AgregarLog($"📂 Destino: {archivoCompleto}", Color.DarkGreen);

            // Configurar BackgroundWorker
            backgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            backgroundWorker.DoWork += (s, e) =>
            {
                var parametros = (dynamic)e.Argument!;
                RealizarBackup(parametros.archivo, parametros.compresion, parametros.verificar);
            };

            backgroundWorker.ProgressChanged += (s, e) =>
            {
                if (e.UserState is string mensaje)
                    AgregarLog(mensaje, Color.DarkBlue);
            };

            backgroundWorker.RunWorkerCompleted += (s, e) =>
            {
                operacionEnProgreso = false;
                btnIniciarBackup.Enabled = true;
                progressBar.Visible = false;
                progressBar.Style = ProgressBarStyle.Blocks;

                if (e.Error != null)
                {
                    AgregarLog($"❌ Error durante el backup: {e.Error.Message}", Color.Red);
                    if (e.Error.InnerException != null)
                        AgregarLog($"   Detalle: {e.Error.InnerException.Message}", Color.Red);
                }
                else if (e.Cancelled)
                {
                    AgregarLog("⚠️ Backup cancelado por el usuario.", Color.Orange);
                }
                else
                {
                    AgregarLog("✅ Backup completado exitosamente.", Color.Green);
                    AgregarLog($"📁 Archivo guardado en: {archivoCompleto}", Color.Green);
                    
                    // Mostrar información del archivo
                    try
                    {
                        var fileInfo = new FileInfo(archivoCompleto);
                        var tamañoMB = fileInfo.Length / (1024.0 * 1024.0);
                        AgregarLog($"📊 Tamaño del archivo: {tamañoMB:F2} MB", Color.Green);
                    }
                    catch { }
                }

                backgroundWorker?.Dispose();
                backgroundWorker = null;
            };

            // Iniciar backup
            backgroundWorker.RunWorkerAsync(new
            {
                archivo = archivoCompleto,
                compresion = chkCompresion.Checked,
                verificar = chkVerificarBackup.Checked
            });
        }

        private void RealizarBackup(string archivoCompleto, bool usarCompresion, bool verificarBackup)
        {
            try
            {
                backgroundWorker?.ReportProgress(0, "🔗 Conectando a la base de datos...");

                using var conn = new SqlConnection(Db.ConnectionString);
                conn.Open();

                var csb = new SqlConnectionStringBuilder(conn.ConnectionString);
                var dbName = string.IsNullOrWhiteSpace(csb.InitialCatalog) ? "TeoAccesorios" : csb.InitialCatalog;

                backgroundWorker?.ReportProgress(0, $"📋 Base de datos: {dbName}");
                backgroundWorker?.ReportProgress(0, $"🖥️ Servidor: {csb.DataSource ?? "localhost"}");

                // Verificar espacio en disco
                try
                {
                    var drive = new DriveInfo(Path.GetPathRoot(archivoCompleto)!);
                    var espacioLibreGB = drive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
                    backgroundWorker?.ReportProgress(0, $"💾 Espacio libre en disco: {espacioLibreGB:F2} GB");
                }
                catch { }

                // Construir comando SQL
                var target = archivoCompleto.Replace("'", "''");
                var opciones = new List<string> { "FORMAT", "INIT", $"NAME = 'Backup completo de {dbName}'", "SKIP", "NOREWIND", "NOUNLOAD", "STATS = 10" };

                if (usarCompresion)
                {
                    opciones.Add("COMPRESSION");
                    backgroundWorker?.ReportProgress(0, "🗜️ Compresión habilitada");
                }

                if (verificarBackup)
                {
                    opciones.Add("CHECKSUM");
                    backgroundWorker?.ReportProgress(0, "✅ Verificación de integridad habilitada");
                }

                var sql = $@"
                    BACKUP DATABASE [{dbName}]
                    TO DISK = '{target}'
                    WITH {string.Join(", ", opciones)};";

                backgroundWorker?.ReportProgress(0, "⏳ Ejecutando backup... (esto puede tomar varios minutos)");

                using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 0 };
                
                // Configurar evento de progreso si está disponible
                conn.InfoMessage += (s, e) =>
                {
                    if (e.Message.Contains("percent"))
                        backgroundWorker?.ReportProgress(0, $"📊 {e.Message}");
                };

                cmd.ExecuteNonQuery();

                if (verificarBackup)
                {
                    backgroundWorker?.ReportProgress(0, "🔍 Verificando integridad del backup...");
                    
                    var verifySql = $"RESTORE VERIFYONLY FROM DISK = '{target}' WITH CHECKSUM;";
                    using var verifyCmd = new SqlCommand(verifySql, conn) { CommandTimeout = 0 };
                    verifyCmd.ExecuteNonQuery();
                    
                    backgroundWorker?.ReportProgress(0, "✅ Verificación de integridad completada exitosamente");
                }
            }
            catch (SqlException sqlEx)
            {
                var mensaje = sqlEx.Message;
                if (sqlEx.Message.Contains("permission"))
                    mensaje += "\n\nVerifique que el servicio de SQL Server tenga permisos de escritura en la carpeta de destino.";
                
                throw new Exception($"Error de SQL Server: {mensaje}", sqlEx);
            }
            catch (UnauthorizedAccessException uaEx)
            {
                throw new Exception($"Permisos insuficientes: {uaEx.Message}\n\nVerifique que tenga permisos de escritura en la carpeta de destino.", uaEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error durante el backup: {ex.Message}", ex);
            }
        }
    }
}