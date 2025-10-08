﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TeoAccesorios.Desktop
{
    public class InactivosBaseForm<T> : Form where T : class
    {
        protected readonly BindingSource bs = new();

        protected readonly DataGridView grid = new()
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,                         
            AutoGenerateColumns = false,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,

            // Bloqueos de edición y tamaño
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeColumns = false,        
            AllowUserToResizeRows = false,           
            AllowUserToOrderColumns = false,        

            // Bloquear cambios en cabeceras
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            RowHeadersVisible = false,
            RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing
        };

        private readonly Button btnRestaurar = new() { Text = "Restaurar" };
        private readonly Button btnCerrar = new() { Text = "Cerrar" };

        private readonly Func<IEnumerable<T>> _loader;
        private readonly Action<T> _restaurarAction;

        public InactivosBaseForm(
            string titulo,
            Func<IEnumerable<T>> loader,
            Action<T> restaurarAction,
            Action<DataGridView> configurarGrid)
        {
            Text = titulo;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(800, 500);
            Size = new System.Drawing.Size(900, 560);

            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
            _restaurarAction = restaurarAction ?? throw new ArgumentNullException(nameof(restaurarAction));

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(8) };
            top.Controls.Add(btnRestaurar);
            top.Controls.Add(btnCerrar);

            Controls.Add(grid);
            Controls.Add(top);

            GridHelper.Estilizar(grid);
            GridHelperLock.Apply(grid);

            configurarGrid(grid);
            grid.DataSource = bs;

            Load += (_, __) => Refrescar();
            Shown += (_, __) => AjustarTamanoRespectoAlOwner(); //  al abrir, ocupa ~70% del padre
            btnRestaurar.Click += (_, __) => OnRestaurar();
            btnCerrar.Click += (_, __) => Close();
        }

        protected void Refrescar() => bs.DataSource = _loader().ToList();

        private void OnRestaurar()
        {
            if (bs.Current is T item)
            {
                _restaurarAction(item);
                Refrescar();
            }
        }
        private void AjustarTamanoRespectoAlOwner()
        {
            // El tamaño ahora es fijo y se establece en el constructor.
            // Si el Owner es más pequeño que el formulario, centrarlo sigue siendo el comportamiento correcto.
            CenterToParent();
        }
    }
}
