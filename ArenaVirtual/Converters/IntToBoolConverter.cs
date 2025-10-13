using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace ArenaVirtual.Converters {
    public class IntToBoolConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is int rodada && parameter is string paramString) {
                var parts = paramString.Split('_');
                if (parts.Length == 2 && int.TryParse(parts[0], out int expectedRodada) && int.TryParse(parts[1], out int gameIndex)) {
                    if (rodada == expectedRodada) {
                        
                        return true;
                    }
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}