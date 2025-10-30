using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace ArenaVirtual.Converters {
    public class IsNotNullConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            // Esta é a verificação mais simples e segura: se o objeto não é nulo.
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException("IsNotNullConverter não suporta conversão reversa.");
        }
    }
}