using System.Globalization;
using Microsoft.Maui.Controls;

namespace ArenaVirtual.Converters {
    public class CnpjConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string rawNumber && !string.IsNullOrEmpty(rawNumber)) {
                var digits = new string(rawNumber.Where(char.IsDigit).ToArray());

                if (digits.Length == 14) 
                {
                    return $"{digits.Substring(0, 2)}.{digits.Substring(2, 3)}.{digits.Substring(5, 3)}/{digits.Substring(8, 4)}-{digits.Substring(12, 2)}";
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string formattedNumber) {
                return new string(formattedNumber.Where(char.IsDigit).ToArray());
            }
            return value;
        }
    }
}