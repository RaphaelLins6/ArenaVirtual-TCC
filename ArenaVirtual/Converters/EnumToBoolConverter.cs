using System.Globalization;

namespace ArenaVirtual.Converters {
    internal class EnumToBoolConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null || parameter == null)
                return false;

            string enumValue = value.ToString();
            string parameterValue = parameter.ToString();

            return enumValue.Equals(parameterValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
