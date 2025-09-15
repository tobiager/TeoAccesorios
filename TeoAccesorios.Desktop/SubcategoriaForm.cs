using System;
using System.Linq;
using System.Windows.Forms;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
	public class SubcategoriasForm : Form
	{
		// UI
		private readonly DataGridView grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
		private readonly BindingSource bs = new BindingSource();
		private readonly ComboBox cboFiltroCat = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
		private readonly CheckBox chkInactivas = new CheckBox { Text = "Ver inactivas" };

		// filtro inicial opcional
		private readonly int? _categoriaInicial;

		// ctor por defecto
		public SubcategoriasForm() : this(null) { }

		// ctor con filtro por categoría
		public SubcategoriasForm(int? categoriaId)
		{
			_categoriaInicial = categoriaId;

			Text = "Subcategorías";
			Width = 900;
			Height = 600;
			StartPosition = FormStartPosition.CenterParent;

			// Barra superior
			var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(6) };
			top.Controls.Add(new Label { Text = "Categoría:", AutoSize = true, Padding = new Padding(0, 10, 6, 0) });
			top.Controls.Add(cboFiltroCat);
			top.Controls.Add(chkInactivas);
			var btnNuevo = new Button { Text = "Nuevo" };
			var btnEditar = new Button { Text = "Editar" };
			var btnEliminar = new Button { Text = "Eliminar" };
			var btnRestaurar = new Button { Text = "Restaurar" };
			top.Controls.AddRange(new Control[] { btnNuevo, btnEditar, btnEliminar, btnRestaurar });

			Controls.Add(grid);
			Controls.Add(top);

			// Estilo (tus helpers)
			GridHelper.Estilizar(grid);
			GridHelperLock.SoloLectura(grid);

			grid.DataSource = bs;

			// Cargar combos (elegir categoría inicial si vino por ctor)
			CargarCategoriasEnCombo();
			if (_categoriaInicial.HasValue)
				SeleccionarCategoriaEnCombo(_categoriaInicial.Value);

			// Eventos
			chkInactivas.CheckedChanged += (s, e) => LoadSubcategorias();
			cboFiltroCat.SelectedIndexChanged += (s, e) => LoadSubcategorias();

			// ABM
			btnNuevo.Click += (s, e) =>
			{
				int? catId = (cboFiltroCat.SelectedItem as ComboItem)?.Value;
				// si no hay categorías, evitamos excepción
				var primeraCat = Repository.ListarCategorias(true).OrderBy(x => x.Nombre).FirstOrDefault();
				if (!catId.HasValue && primeraCat is null)
				{
					MessageBox.Show("No hay categorías disponibles. Creá una categoría primero.", "Atención",
						MessageBoxButtons.OK, MessageBoxIcon.Information);
					return;
				}

				int catParaNueva = catId ?? primeraCat!.Id;

				var sub = new Subcategoria { Activo = true, CategoriaId = catParaNueva };
				using var f = new SubcategoriaEditForm(sub);
				if (f.ShowDialog(this) == DialogResult.OK)
				{
					sub.Id = Repository.InsertarSubcategoria(sub);
					LoadSubcategorias();
				}
			};

			btnEditar.Click += (s, e) =>
			{
				if (grid.CurrentRow?.DataBoundItem is Subcategoria sel)
				{
					var tmp = new Subcategoria
					{
						Id = sel.Id,
						Nombre = sel.Nombre,
						Descripcion = sel.Descripcion,
						CategoriaId = sel.CategoriaId,
						Activo = sel.Activo
					};
					using var f = new SubcategoriaEditForm(tmp);
					if (f.ShowDialog(this) == DialogResult.OK)
					{
						Repository.ActualizarSubcategoria(tmp);
						LoadSubcategorias();
					}
				}
			};

			btnEliminar.Click += (s, e) =>
			{
				if (grid.CurrentRow?.DataBoundItem is Subcategoria sel && sel.Activo)
				{
					if (!Repository.TryDesactivarSubcategoria(sel.Id, out int cant))
					{
						MessageBox.Show(
							$"No se puede desactivar la subcategoría \"{sel.Nombre}\" porque tiene {cant} producto(s) asignado(s).\n\n" +
							"Primero reasigná o quitá esos productos.",
							"Acción no permitida",
							MessageBoxButtons.OK,
							MessageBoxIcon.Warning);
						return;
					}
					LoadSubcategorias();
				}
			};

			btnRestaurar.Click += (s, e) =>
			{
				if (grid.CurrentRow?.DataBoundItem is Subcategoria sel && !sel.Activo)
				{
					Repository.SetSubcategoriaActiva(sel.Id, true);
					LoadSubcategorias();
				}
			};

			// Doble click para editar (quality of life)
			grid.CellDoubleClick += (s, e) => btnEditar.PerformClick();

			// Inicial
			LoadSubcategorias();
		}

		private void CargarCategoriasEnCombo()
		{
			cboFiltroCat.Items.Clear();
			cboFiltroCat.Items.Add(new ComboItem { Text = "Todas", Value = null });
			foreach (var c in Repository.ListarCategorias(true).OrderBy(x => x.Nombre))
				cboFiltroCat.Items.Add(new ComboItem { Text = $"{c.Id} - {c.Nombre}", Value = c.Id });
			cboFiltroCat.SelectedIndex = 0;
		}

		private void SeleccionarCategoriaEnCombo(int categoriaId)
		{
			for (int i = 0; i < cboFiltroCat.Items.Count; i++)
			{
				if ((cboFiltroCat.Items[i] as ComboItem)?.Value == categoriaId)
				{
					cboFiltroCat.SelectedIndex = i;
					break;
				}
			}
		}

		private void LoadSubcategorias()
		{
			int? catId = (cboFiltroCat.SelectedItem as ComboItem)?.Value;
			var data = Repository.ListarSubcategorias(catId, chkInactivas.Checked)
								 .OrderBy(s => s.CategoriaNombre)
								 .ThenBy(s => s.Nombre)
								 .ToList();
			bs.DataSource = data;
			grid.DataSource = bs;
		}

		private class ComboItem
		{
			public string Text { get; set; } = "";
			public int? Value { get; set; }
			public override string ToString() => Text;
		}
	}
}

