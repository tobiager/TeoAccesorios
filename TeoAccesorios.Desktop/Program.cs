using System;
using System.Windows.Forms;

namespace TeoAccesorios.Desktop
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // 1) Mostrar Login como diálogo (no como formulario principal)
            using (var login = new LoginForm { StartPosition = FormStartPosition.CenterScreen })
            {
                var result = login.ShowDialog();

                // 2) Si el login fue OK, abrimos la app real (MainForm) ya maximizada
                if (result == DialogResult.OK)
                {
                    Application.Run(new MainForm());   // MainForm ya se maximiza en su constructor
                }
                // Si canceló o falló el login, simplemente salimos.
            }
        }
    }
}
