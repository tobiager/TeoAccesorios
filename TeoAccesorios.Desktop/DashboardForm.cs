using System;
using System.Drawing;
using System.Windows.Forms;

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

            side.Controls.Add(Btn("Nueva Venta", (_, __) => ShowInContent(new NuevaVentaForm())));
            side.Controls.Add(Btn("Ver Ventas", (_, __) => ShowInContent(new VentasForm())));
            side.Controls.Add(Btn("Productos", (_, __) => ShowInContent(new ProductosForm())));

            // <<< BOTÓN NUEVO AQUÍ (debajo de Productos)
            side.Controls.Add(Btn("Categorías", (_, __) => ShowInContent(new CategoriasForm())));
            // >>>

            side.Controls.Add(Btn("Clientes", (_, __) => ShowInContent(new ClientesForm())));

            if (Sesion.Rol == RolUsuario.Admin)
                side.Controls.Add(Btn("Empleados", (_, __) => ShowInContent(new UsuariosForm())));

            side.Controls.Add(Btn("Reportes", (_, __) => ShowInContent(new ReportesForm())));
            side.Controls.Add(Btn("Inicio (Dashboard)", (_, __) => ShowKpis()));
            side.Controls.Add(Btn("Cerrar sesión", (_, __) => { Hide(); using var l = new LoginForm(); l.ShowDialog(this); Close(); }));

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
    }
}
