using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace ArenaVirtual.Converters {
    public class SequenciaToColorConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            char resultado;

            if (value is string s && s.Length == 1) {
                resultado = s[0];
            }
            else if (value is char c) {
                resultado = c;
            }
            else {
                return Color.FromArgb("#9E9E9E"); // Cinza
            }

            char cUpper = char.ToUpper(resultado);

            return cUpper switch {
                'V' => Color.FromArgb("#4CAF50"), // Verde (Vitória)
                'D' => Color.FromArgb("#F44336"), // Vermelho (Derrota)
                'E' => Color.FromArgb("#FFC107"), // Amarelo (Empate)
                'N' => Color.FromArgb("#9E9E9E"), // Cinza (Não Disputado/Nulo)
                '-' => Color.FromArgb("#9E9E9E"), // Cinza (Não Disputado)
                _ => Color.FromArgb("#9E9E9E"), // Cinza
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}