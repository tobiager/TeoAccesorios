using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TeoAccesorios.Desktop.UI.Common
{
    public static class FormUtils
    {
        /// <summary>
        /// Populates a ComboBox safely, avoiding exceptions and unintended event triggers.
        /// </summary>
        public static void BindCombo<T>(
            ComboBox cbo,
            IList<T> data,
            string displayMember = "Nombre",
            string valueMember = "Id",
            int? selectedValue = null)
        {
            // Store old value to try to restore it if no new selection is provided
            var oldValue = cbo.SelectedValue;

            cbo.BeginUpdate();
            cbo.DataSource = null;

            cbo.DisplayMember = displayMember;
            cbo.ValueMember = valueMember;
            cbo.DropDownStyle = ComboBoxStyle.DropDownList;
            cbo.AutoCompleteMode = AutoCompleteMode.None; // Prevents NotSupportedException
            cbo.AutoCompleteSource = AutoCompleteSource.None;

            cbo.DataSource = data?.ToList() ?? new List<T>();

            cbo.SelectedValue = selectedValue ?? oldValue ?? -1;
            cbo.EndUpdate();
        }
    }
}