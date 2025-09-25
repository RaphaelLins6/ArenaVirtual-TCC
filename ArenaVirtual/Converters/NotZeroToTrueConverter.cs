using System.Globalization;

namespace ArenaVirtual.Converters {
    public class NotZeroToTrueConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is int intValue)
                return intValue > 0; // Altera para "maior que zero"
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}