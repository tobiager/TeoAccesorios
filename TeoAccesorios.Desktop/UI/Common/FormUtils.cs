using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TeoAccesorios.Desktop.UI.Common
{
    /// <summary>
    /// Contiene métodos de utilidad para formularios de WinForms.
    /// </summary>
    public static class FormUtils
    {
        /// <summary>
        /// Vincula una lista de objetos a un ComboBox, con opción de agregar un item "Todos".
        /// </summary>
        public static void BindCombo<T>(ComboBox cbo, IEnumerable<T> source, string? allItemText = null)
        {
            var list = new List<object>();
            if (allItemText != null)
            {
                list.Add(allItemText);
            }
            if (source != null)
            {
                list.AddRange(source.Cast<object>());
            }

            cbo.DataSource = list;
            if (cbo.Items.Count > 0)
            {
                cbo.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Vincula una lista de objetos a un ComboBox, estableciendo DisplayMember y ValueMember.
        /// </summary>
        public static void BindCombo<T>(ComboBox cbo, List<T> source, string displayMember = "Nombre", string valueMember = "Id", object? selectedValue = null)
        {
            // Validar que los objetos tienen las propiedades especificadas
            if (source != null && source.Count > 0)
            {
                var firstItem = source[0];
                var type = firstItem!.GetType();
                
                // Si es un tipo primitivo (string, int, etc.), no intentar establecer DisplayMember/ValueMember
                if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
                {
                    cbo.DataSource = source;
                    if (selectedValue != null) cbo.SelectedValue = selectedValue;
                    else if (cbo.Items.Count > 0) cbo.SelectedIndex = 0;
                    return;
                }
                
                // Verificar que las propiedades existen
                var displayProperty = type.GetProperty(displayMember);
                var valueProperty = type.GetProperty(valueMember);
                
                if (displayProperty == null)
                {
                    throw new ArgumentException($"La propiedad '{displayMember}' no existe en el tipo {type.Name}");
                }
                
                if (valueProperty == null)
                {
                    throw new ArgumentException($"La propiedad '{valueMember}' no existe en el tipo {type.Name}");
                }
            }

            cbo.DataSource = source;
            cbo.DisplayMember = displayMember;
            cbo.ValueMember = valueMember;

            if (selectedValue != null) cbo.SelectedValue = selectedValue;
            else if (cbo.Items.Count > 0) cbo.SelectedIndex = 0;
        }
    }
}