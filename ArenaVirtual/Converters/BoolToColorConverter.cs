using System.Globalization;

namespace ArenaVirtual.Converters {
    public class BoolToColorConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is bool isSelected) {
                return isSelected ? Color.FromArgb("#FF8C00") : Color.FromArgb("#333333");
            }
            return Color.FromArgb("#333333");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}