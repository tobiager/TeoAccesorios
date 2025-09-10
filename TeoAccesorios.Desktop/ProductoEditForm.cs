using System;
using System.Linq;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public class ProductoEditForm : Form
    {
        private readonly TextBox txtNombre = new();
        private readonly TextBox txtDesc = new();
        private readonly NumericUpDown numPrecio = new() { Minimum = 0, Maximum = 100000000, DecimalPlaces = 2, Increment = 100 };
        private readonly NumericUpDown numStock = new() { Minimum = 0, Maximum = 100000 };
        private readonly NumericUpDown numMin = new() { Minimum = 0, Maximum = 100000, Value = 5 };
        private readonly ComboBox cboCat = new() { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox cboSubcat = new() { DropDownStyle = ComboBoxStyle.DropDownList };   // << NUEVO
        private readonly Button btnGuardar = new() { Text = "Guardar", Dock = DockStyle.Bottom, Height = 36 };
        private readonly Button btnCancelar = new() { Text = "Cancelar", Dock = DockStyle.Bottom, Height = 32 };
        private readonly ErrorProvider ep = new();

        private readonly Producto model;

        public ProductoEditForm(Producto p)
        {
            model = p ?? new Producto();

            Text = "Producto";
            Width = 560; Height = 440;                                   // un poco más alto para el combo nuevo
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;

            // Categorías
            var cats = Repository.ListarCategorias();
            cboCat.Items.Clear();
            foreach (var c in cats) cboCat.Items.Add($"{c.Id} - {c.Nombre}");
            if (cboCat.Items.Count > 0) cboCat.SelectedIndex = 0;

            // Layout
            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(12) };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 7; i++) grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));   // +1 fila

            grid.Controls.Add(new Label { Text = "Nombre *" }, 0, 0); grid.Controls.Add(txtNombre, 1, 0);
            grid.Controls.Add(new Label { Text = "Descripción" }, 0, 1); grid.Controls.Add(txtDesc, 1, 1);
            grid.Controls.Add(new Label { Text = "Precio *" }, 0, 2); grid.Controls.Add(numPrecio, 1, 2);
            grid.Controls.Add(new Label { Text = "Stock *" }, 0, 3); grid.Controls.Add(numStock, 1, 3);
            grid.Controls.Add(new Label { Text = "Stock mínimo *" }, 0, 4); grid.Controls.Add(numMin, 1, 4);
            grid.Controls.Add(new Label { Text = "Categoría *" }, 0, 5); grid.Controls.Add(cboCat, 1, 5);
            grid.Controls.Add(new Label { Text = "Subcategoría" }, 0, 6); grid.Controls.Add(cboSubcat, 1, 6);   // << NUEVO

            Controls.Add(btnCancelar);
            Controls.Add(btnGuardar);
            Controls.Add(grid);

            AcceptButton = btnGuardar;
            CancelButton = btnCancelar;

            // Prefill
            txtNombre.Text = p?.Nombre ?? "";
            txtDesc.Text = p?.Descripcion ?? "";
            if (p != null)
            {
                if (p.Precio >= numPrecio.Minimum && p.Precio <= numPrecio.Maximum) numPrecio.Value = p.Precio;
                if (p.Stock >= numStock.Minimum && p.Stock <= numStock.Maximum) numStock.Value = p.Stock;
                if (p.StockMinimo >= numMin.Minimum && p.StockMinimo <= numMin.Maximum) numMin.Value = p.StockMinimo;
                var idx = cats.FindIndex(c => c.Id == p.CategoriaId);
                if (idx >= 0) cboCat.SelectedIndex = idx;
            }

            // Eventos
            txtNombre.TextChanged += (_, __) => btnGuardar.Enabled = ValidarTodo();
            txtDesc.TextChanged += (_, __) => btnGuardar.Enabled = ValidarTodo();
            numPrecio.ValueChanged += (_, __) => btnGuardar.Enabled = ValidarTodo();
            numStock.ValueChanged += (_, __) => btnGuardar.Enabled = ValidarTodo();
            numMin.ValueChanged += (_, __) => btnGuardar.Enabled = ValidarTodo();

            // Cuando cambia la categoría, recargo subcategorías válidas
            cboCat.SelectedIndexChanged += (_, __) =>
            {
                CargarSubcategorias();                         // << NUEVO
                btnGuardar.Enabled = ValidarTodo();
            };

            btnCancelar.Click += (_, __) => DialogResult = DialogResult.Cancel;

            btnGuardar.Click += (_, __) =>
            {
                if (!ValidarTodo()) return;

                model.Nombre = txtNombre.Text.Trim();
                model.Descripcion = string.IsNullOrWhiteSpace(txtDesc.Text) ? null : txtDesc.Text.Trim();
                model.Precio = numPrecio.Value;          // >= 0
                model.Stock = (int)numStock.Value;       // >= 0
                model.StockMinimo = (int)numMin.Value;         // >= 0

                var selCat = cboCat.SelectedItem!.ToString()!;
                model.CategoriaId = int.Parse(selCat.Split('-')[0].Trim());
                model.CategoriaNombre = selCat.Split('-', 2)[1].Trim();

                // Subcategoría (0 => NULL)
                var selSub = cboSubcat.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(selSub))
                {
                    var subId = int.Parse(selSub.Split('-')[0].Trim());
                    model.SubcategoriaId = subId > 0 ? subId : (int?)null;
                    model.SubcategoriaNombre = subId > 0 ? selSub.Split('-', 2)[1].Trim() : "";
                }

                DialogResult = DialogResult.OK;
                Close();
            };

            // Carga inicial de subcategorías (depende de categoría preseleccionada)
            CargarSubcategorias(init: true);                    // << NUEVO

            btnGuardar.Enabled = ValidarTodo();
        }

        // Carga subcategorías filtradas por la categoría seleccionada
        private void CargarSubcategorias(bool init = false)
        {
            cboSubcat.Items.Clear();

            // Id de categoría seleccionado en el combo
            var sel = cboCat.SelectedItem?.ToString();
            var catId = 0;
            if (!string.IsNullOrEmpty(sel)) catId = int.Parse(sel.Split('-')[0].Trim());

            var subcats = Repository
                .ListarSubcategorias(catId, incluirInactivas: false)
                .OrderBy(s => s.Nombre)
                .ToList();

            // Placeholder para permitir dejar NULL
            cboSubcat.Items.Add("0 - (Sin subcategoría)");

            foreach (var s in subcats)
                cboSubcat.Items.Add($"{s.Id} - {s.Nombre}");

            // Selección
            if (init && model.SubcategoriaId.HasValue && model.SubcategoriaId.Value > 0)
            {
                var idx = cboSubcat.Items
                    .Cast<string>()
                    .ToList()
                    .FindIndex(x => x.StartsWith(model.SubcategoriaId.Value.ToString() + " "));
                cboSubcat.SelectedIndex = idx >= 0 ? idx : 0;
            }
            else
            {
                cboSubcat.SelectedIndex = 0;
            }

            // UX: deshabilitar si no hay opciones reales
            cboSubcat.Enabled = cboSubcat.Items.Count > 1;
        }

        private bool ValidarTodo()
        {
            bool ok = true;
            ok &= FormValidator.Require(txtNombre, ep, "Nombre requerido (2–80)", 2, 80);
            ok &= FormValidator.RequireNumber(numPrecio, ep, "Precio ≥ 0", 0);
            ok &= FormValidator.RequireNumber(numStock, ep, "Stock ≥ 0 (0 permitido)", 0);
            ok &= FormValidator.RequireNumber(numMin, ep, "Stock mínimo ≥ 0", 0);
            ok &= FormValidator.RequireCombo(cboCat, ep, "Seleccioná una categoría");
            // Subcategoría es opcional (el trigger igual está protegido al filtrar por categoría)
            return ok;
        }
    }
}
