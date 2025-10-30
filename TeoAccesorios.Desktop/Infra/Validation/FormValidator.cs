using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TeoAccesorios.Desktop
{
    public static class FormValidator
    {
        public static bool Require(TextBox tb, ErrorProvider ep, string msg, int minLen = 1, int maxLen = int.MaxValue)
        {
            string v = tb.Text?.Trim() ?? "";
            if (v.Length < minLen || v.Length > maxLen)
            {
                ep.SetError(tb, msg);
                return false;
            }
            ep.SetError(tb, "");
            return true;
        }

        public static bool RequireCombo(ComboBox cb, ErrorProvider ep, string msg)
        {
            if (cb.SelectedIndex < 0 && cb.SelectedValue == null)
            {
                ep.SetError(cb, msg);
                return false;
            }
            ep.SetError(cb, "");
            return true;
        }

        public static bool RequireNumber(NumericUpDown num, ErrorProvider ep, string msg, decimal min, decimal? max = null)
        {
            decimal v = num.Value;
            if (v < min || (max.HasValue && v > max.Value))
            {
                ep.SetError(num, msg);
                return false;
            }
            ep.SetError(num, "");
            return true;
        }

        public static bool OptionalEmail(TextBox tb, ErrorProvider ep, string msg)
        {
            string v = tb.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(v))
            {
                ep.SetError(tb, "");
                return true; 
            }

            // email simple
            var ok = Regex.IsMatch(v, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!ok) ep.SetError(tb, msg); else ep.SetError(tb, "");
            return ok;
        }

        public static bool OptionalPhone(TextBox tb, ErrorProvider ep, string msg)
        {
            string v = tb.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(v))
            {
                ep.SetError(tb, "");
                return true;
            }
            // acepta +, espacios, -, paréntesis, dígitos
            var ok = Regex.IsMatch(v, @"^[\d\+\-\s\(\)]{6,}$");
            if (!ok) ep.SetError(tb, msg); else ep.SetError(tb, "");
            return ok;
        }
    }
}
