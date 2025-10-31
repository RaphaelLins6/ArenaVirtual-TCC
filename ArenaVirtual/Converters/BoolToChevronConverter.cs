using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace ArenaVirtual.Converters {
    public class BoolToChevronConverter : IValueConverter {
        // Seta para BAIXO (recolhida) ou para CIMA (expandida)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is bool isExpanded) {
                // Unicode para seta para cima (expandida)
                if (isExpanded) return "▲";

                // Unicode para seta para baixo (recolhida)
                return "▼";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}