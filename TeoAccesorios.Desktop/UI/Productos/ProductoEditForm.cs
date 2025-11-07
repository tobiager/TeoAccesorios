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
        private readonly ComboBox cboSubcat = new() { DropDownStyle = ComboBoxStyle.DropDownList };   
        private readonly Button btnGuardar = new() { Text = "Guardar", Dock = DockStyle.Bottom, Height = 36 };
        private readonly Button btnCancelar = new() { Text = "Cancelar", Dock = DockStyle.Bottom, Height = 32 };
        private readonly ErrorProvider ep = new();

        private readonly Producto model;

        public ProductoEditForm(Producto p)
        {
            model = p ?? new Producto();

            Text = "Producto";
            Width = 560; Height = 440;                                   
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
            for (int i = 0; i < 7; i++) grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));   

            grid.Controls.Add(new Label { Text = "Nombre *" }, 0, 0); grid.Controls.Add(txtNombre, 1, 0);
            grid.Controls.Add(new Label { Text = "Descripción" }, 0, 1); grid.Controls.Add(txtDesc, 1, 1);
            grid.Controls.Add(new Label { Text = "Precio *" }, 0, 2); grid.Controls.Add(numPrecio, 1, 2);
            grid.Controls.Add(new Label { Text = "Stock *" }, 0, 3); grid.Controls.Add(numStock, 1, 3);
            grid.Controls.Add(new Label { Text = "Stock mínimo *" }, 0, 4); grid.Controls.Add(numMin, 1, 4);
            grid.Controls.Add(new Label { Text = "Categoría *" }, 0, 5); grid.Controls.Add(cboCat, 1, 5);
            grid.Controls.Add(new Label { Text = "Subcategoría" }, 0, 6); grid.Controls.Add(cboSubcat, 1, 6);   

            Controls.Add(btnCancelar);
            Controls.Add(btnGuardar);
            Controls.Add(grid);

            // No dejar AcceptButton fijo: lo activamos solo si el formulario está válido
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

            // Eventos: usar helper para centralizar enable/AcceptButton
            txtNombre.TextChanged += (_, __) => UpdateGuardarState();
            txtDesc.TextChanged += (_, __) => UpdateGuardarState();
            numPrecio.ValueChanged += (_, __) => UpdateGuardarState();
            numStock.ValueChanged += (_, __) => UpdateGuardarState();
            numMin.ValueChanged += (_, __) => UpdateGuardarState();

            // Cuando cambia la categoría, recargo subcategorías válidas
            cboCat.SelectedIndexChanged += (_, __) =>
            {
                CargarSubcategorias();                         
                UpdateGuardarState();
            };

            btnCancelar.Click += (_, __) => DialogResult = DialogResult.Cancel;

            btnGuardar.Click += (_, __) =>
            {
                // doble chequeo por seguridad
                if (!ValidarTodo()) return;

                model.Nombre = txtNombre.Text.Trim();
                model.Descripcion = string.IsNullOrWhiteSpace(txtDesc.Text) ? null : txtDesc.Text.Trim();
                model.Precio = numPrecio.Value;          
                model.Stock = (int)numStock.Value;       
                model.StockMinimo = (int)numMin.Value;         

                var selCat = cboCat.SelectedItem!.ToString()!;
                model.CategoriaId = int.Parse(selCat.Split('-')[0].Trim());
                model.CategoriaNombre = selCat.Split('-', 2)[1].Trim();

                // Subcategoría 
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
            CargarSubcategorias(init: true);                    

            // Estado inicial del botón/AcceptButton
            UpdateGuardarState();
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
            // Limpiar errores previos
            ep.Clear();

            ok &= FormValidator.Require(txtNombre, ep, "Nombre requerido (2–80)", 2, 80);
            ok &= FormValidator.RequireNumber(numPrecio, ep, "Precio ≥ 0", 0);
            ok &= FormValidator.RequireNumber(numStock, ep, "Stock ≥ 0 (0 permitido)", 0);
            ok &= FormValidator.RequireNumber(numMin, ep, "Stock mínimo ≥ 0", 0);
            ok &= FormValidator.RequireCombo(cboCat, ep, "Seleccioná una categoría");

            // Validaciones defensivas adicionales (evitan valores fuera de límites si se establecen por código)
            if (numPrecio.Value < numPrecio.Minimum || numPrecio.Value > numPrecio.Maximum)
            {
                ep.SetError(numPrecio, $"Precio debe estar entre {numPrecio.Minimum} y {numPrecio.Maximum}");
                ok = false;
            }

            if (numStock.Value < numStock.Minimum || numStock.Value > numStock.Maximum)
            {
                ep.SetError(numStock, $"Stock debe estar entre {numStock.Minimum} y {numStock.Maximum}");
                ok = false;
            }

            if (numMin.Value < numMin.Minimum || numMin.Value > numMin.Maximum)
            {
                ep.SetError(numMin, $"Stock mínimo debe estar entre {numMin.Minimum} y {numMin.Maximum}");
                ok = false;
            }

            // Evitar inconsistencias obvias: stock no negativo y stock mínimo no negativo (NumericUpDown los evita, pero por seguridad)
            if (numStock.Value < 0)
            {
                ep.SetError(numStock, "Stock no puede ser negativo");
                ok = false;
            }
            if (numMin.Value < 0)
            {
                ep.SetError(numMin, "Stock mínimo no puede ser negativo");
                ok = false;
            }

            // Validación de unicidad de nombre (compara strings, case-insensitive)
            var nombreActual = txtNombre.Text.Trim();
            if (!string.IsNullOrEmpty(nombreActual))
            {
                var iguales = Repository.ListarProductos(true)
                    .Where(x => x.Id != model.Id) // excluir el propio registro al editar
                    .Select(x => (x.Nombre ?? "").Trim())
                    .Any(n => string.Equals(n, nombreActual, StringComparison.OrdinalIgnoreCase));

                if (iguales)
                {
                    ep.SetError(txtNombre, "Ya existe otro producto con el mismo nombre");
                    ok = false;
                }
            }

            return ok;
        }

        // Centraliza la actualización del estado del botón Guardar y del AcceptButton
        private void UpdateGuardarState()
        {
            bool valido = ValidarTodo();
            btnGuardar.Enabled = valido;
            // Solo permitir que Enter invoque Guardar cuando el formulario sea válido
            AcceptButton = valido ? btnGuardar : null;
        }
    }
}
