using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using System.Linq;

namespace ArenaVirtual.Converters {
    public class FormatConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null || parameter == null) return value;

            var text = value.ToString();
            // Limpa o valor (remove não dígitos)
            text = new string(text.Where(char.IsDigit).ToArray());
            var maskType = parameter.ToString().ToUpper();
            var newText = string.Empty;

            if (maskType.Equals("PHONE")) {
                if (text.Length > 11) text = text.Substring(0, 11);

                // Formato: (XX) 9 XXXX-XXXX (11 dígitos)
                if (text.Length > 0) newText += "(" + text.Substring(0, Math.Min(2, text.Length));
                if (text.Length >= 2) newText += ")";

                if (text.Length > 2) {
                    // Nono Dígito e Bloco de 4
                    newText += " " + text.Substring(2, Math.Min(1, text.Length - 2)); // 9
                    if (text.Length > 3) {
                        newText += " " + text.Substring(3, Math.Min(4, text.Length - 3)); // XXXX
                    }
                }

                // Traço e Último Bloco
                if (text.Length > 7) {
                    newText += "-" + text.Substring(7, Math.Min(4, text.Length - 7)); // -XXXX
                }
            } else if (maskType.Equals("CNPJ")) {
                if (text.Length > 14) text = text.Substring(0, 14);
                // Formato: XX.XXX.XXX/XXXX-XX
                if (text.Length > 0) newText += text.Substring(0, Math.Min(2, text.Length));
                if (text.Length > 2) newText += "." + text.Substring(2, Math.Min(3, text.Length - 2));
                if (text.Length > 5) newText += "." + text.Substring(5, Math.Min(3, text.Length - 5));
                if (text.Length > 8) newText += "/" + text.Substring(8, Math.Min(4, text.Length - 8));
                if (text.Length > 12) newText += "-" + text.Substring(12, Math.Min(2, text.Length - 12));
            }

            return newText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            // Não usado para exibição
            return value;
        }
    }
}