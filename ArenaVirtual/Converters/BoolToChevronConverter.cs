using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace ArenaVirtual.Converters {
    public class BoolToChevronConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is bool isExpanded) {
                if (isExpanded) return "▲";

                return "▼";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}