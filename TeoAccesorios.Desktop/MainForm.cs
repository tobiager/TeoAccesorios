using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TeoAccesorios.Desktop
{
    public class MainForm : Form
    {
        public MainForm()
        {
            Text = $"TeoAccesorios — {Sesion.Rol}";
            Width = 1200;
            Height = 720;
            StartPosition = FormStartPosition.CenterScreen;

            var menu = new MenuStrip();
            var mArchivo = new ToolStripMenuItem("Archivo");
            var mSalir = new ToolStripMenuItem("Salir", null, (_,__) => Close());
            mArchivo.DropDownItems.Add(mSalir);
            var mVer = new ToolStripMenuItem("Ver");
            var mProductos = new ToolStripMenuItem("Productos", null, (_,__) => new ProductosForm().ShowDialog(this));
            var mCategorias = new ToolStripMenuItem("Categorías", null, (_,__) => new CategoriasForm().ShowDialog(this));
            var mClientes = new ToolStripMenuItem("Clientes", null, (_,__) => new ClientesForm().ShowDialog(this));
            var mPedidos = new ToolStripMenuItem("Pedidos", null, (_,__) => new PedidosForm().ShowDialog(this));
            var mVentas = new ToolStripMenuItem("Ventas", null, (_,__) => new VentasForm().ShowDialog(this));
            var mCarrito = new ToolStripMenuItem("Carrito / Checkout", null, (_,__) => new CarritoForm().ShowDialog(this));
            var mDashboard = new ToolStripMenuItem("Dashboard", null, (_,__) => new DashboardForm().ShowDialog(this));
            mVer.DropDownItems.AddRange(new[]{ mProductos, mCategorias, mClientes, mPedidos, mVentas, mCarrito, mDashboard });
            menu.Items.AddRange(new[]{ mArchivo, mVer });
            MainMenuStrip = menu;
            Controls.Add(menu);

            var hero = new Panel{ Dock = DockStyle.Fill, Padding = new Padding(24) }; hero.BackColor = System.Drawing.Color.FromArgb(10,14,28);
            hero.BackColor = System.Drawing.Color.FromArgb(10,14,28);

            var title = new Label{ Text = $"TeoAccesorios — {Sesion.Usuario}", AutoSize = true, Font = new Font("Segoe UI", 28, FontStyle.Bold), ForeColor = Color.White };
            var subtitle = new Label{ Text = "Catálogo demo para mostrar pantallas (WinForms, .NET 8).", AutoSize = true, Font = new Font("Segoe UI", 11), ForeColor = Color.Gainsboro, Top = 60 };
            subtitle.Left = 6;

            var buttons = new FlowLayoutPanel{ FlowDirection = FlowDirection.LeftToRight, AutoSize = true, Top = 110, Left = 0, Padding = new Padding(0), Margin = new Padding(0) };
            
            void StyleButton(Button b){
                b.AutoSize = true; b.Height=36; b.Padding = new Padding(14,6,14,6);
                b.FlatStyle = FlatStyle.Flat; b.FlatAppearance.BorderSize = 0;
                b.BackColor = System.Drawing.Color.FromArgb(14,165,233); b.ForeColor = System.Drawing.Color.White;
                b.Margin = new Padding(8);
            }

            void addBtn(string text, EventHandler onClick){
                var b = new Button{ Text = text }; StyleButton(b);
                b.Click += onClick;
                buttons.Controls.Add(b);
            }
            addBtn("Productos", (_,__) => new ProductosForm().ShowDialog(this));
            addBtn("Categorías", (_,__) => new CategoriasForm().ShowDialog(this));
            addBtn("Clientes", (_,__) => new ClientesForm().ShowDialog(this));
            addBtn("Pedidos", (_,__) => new PedidosForm().ShowDialog(this)); addBtn("Ventas", (_,__) => new VentasForm().ShowDialog(this));
            addBtn("Carrito / Checkout", (_,__) => new CarritoForm().ShowDialog(this));
            var btnDash = new Button(); StyleButton(btnDash); btnDash.Text = "Dashboard"; btnDash.Click += (_,__) => new DashboardForm().ShowDialog(this); if(Sesion.Rol==RolUsuario.Admin) buttons.Controls.Add(btnDash);

            hero.Controls.Add(title);
            hero.Controls.Add(subtitle);
            hero.Controls.Add(buttons);
            buttons.Top = 120;

            Controls.Add(hero);
        }
    }
}
