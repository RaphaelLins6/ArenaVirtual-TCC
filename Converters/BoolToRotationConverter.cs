using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace ArenaVirtuall.Converters {
    public class BoolToRotationConverter : IValueConverter {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is bool booleanValue) {
                return booleanValue ? 180 : 0;
            }
            return 0;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException("ConvertBack não é suportado para BoolToRotationConverter.");
        }
    }
}