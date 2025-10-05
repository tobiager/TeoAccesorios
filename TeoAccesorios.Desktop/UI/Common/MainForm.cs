using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Data.SqlClient; 

namespace TeoAccesorios.Desktop
{
    public class MainForm : Form
    {
        public MainForm()
        {
            Text = "TeoAccesorios — Principal";
            StartPosition = FormStartPosition.CenterScreen;

            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1200, 700);
            this.Load += (_, __) => this.WindowState = FormWindowState.Maximized;

            var top = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 52,
                Padding = new Padding(8),
                BackColor = Color.FromArgb(245, 247, 250)
            };

            var btnClientes = new Button { Text = "Clientes", AutoSize = true };
            var btnProductos = new Button { Text = "Productos", AutoSize = true };
            var btnVentas = new Button { Text = "Ventas", AutoSize = true };
            var btnUsuarios = new Button { Text = "Usuarios", AutoSize = true };
            var btnCategorias = new Button { Text = "Categorías", AutoSize = true };
            var btnReportes = new Button { Text = "Reportes", AutoSize = true };
            var btnBackup = new Button { Text = "Backup", AutoSize = true }; 
            top.Controls.AddRange(new Control[]
            {
                btnClientes, btnProductos, btnVentas,
                btnUsuarios, btnCategorias, btnReportes, btnBackup
            });

            var lblSesion = new Label
            {
                Dock = DockStyle.Top,
                Height = 24,
                TextAlign = ContentAlignment.MiddleRight,
                Text = $"Sesión: {Sesion.Usuario} — Rol: {Sesion.Rol}"
            };

            Controls.Add(lblSesion);
            Controls.Add(top);

            // Navegación
            btnClientes.Click += (_, __) => new ClientesForm().ShowDialog(this);
            btnProductos.Click += (_, __) => new ProductosForm().ShowDialog(this);
            btnVentas.Click += (_, __) => new VentasForm().ShowDialog(this);
            btnUsuarios.Click += (_, __) => new UsuariosForm().ShowDialog(this);
            btnCategorias.Click += (_, __) => new CategoriasForm().ShowDialog(this);
            btnReportes.Click += (_, __) => new ReportesForm().ShowDialog(this);
         
        }

    }
}
