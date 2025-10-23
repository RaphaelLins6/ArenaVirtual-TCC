using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace ArenaVirtual.Converters {
    public class StatusToColorConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string statusString) {
                string status = statusString.ToLowerInvariant().Trim();

                if (status == "ativa") {
                    return Color.FromArgb("#2ECC71"); 
                } else if (status == "finalizada" || status == "inativa") {
                    return Color.FromArgb("#95A5A6"); 
                } else if (status == "pendente") {
                    return Color.FromArgb("#F39C12"); 
                }

                return Colors.Black;
            }

            return Colors.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}