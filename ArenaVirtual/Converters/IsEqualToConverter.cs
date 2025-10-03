using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace ArenaVirtual.Converters {
    public class IsEqualToConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value?.Equals(parameter) ?? false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is bool isChecked && isChecked) {
                return parameter;
            }
            return null;
        }
    }
}
