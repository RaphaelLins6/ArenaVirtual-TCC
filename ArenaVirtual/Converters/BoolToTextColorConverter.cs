using System.Globalization;

namespace ArenaVirtual.Converters {
    public class BoolToTextColorConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is bool isSelected) {
                return isSelected ? Color.FromArgb("#1A1A1A") : Colors.White;
            }
            return Colors.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}