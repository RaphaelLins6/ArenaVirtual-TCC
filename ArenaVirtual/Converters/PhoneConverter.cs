using System.Globalization;
using Microsoft.Maui.Controls;

namespace ArenaVirtual.Converters {
    public class PhoneConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string rawNumber && !string.IsNullOrEmpty(rawNumber)) {
                var digits = new string(rawNumber.Where(char.IsDigit).ToArray());

                if (digits.Length == 11) 
                {
                    return $"({digits.Substring(0, 2)}) {digits.Substring(2, 5)}-{digits.Substring(7, 4)}";
                } else if (digits.Length == 10) 
                  {
                    return $"({digits.Substring(0, 2)}) {digits.Substring(2, 4)}-{digits.Substring(6, 4)}";
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