using System.Globalization;

namespace ArenaVirtual.Converters {
    public class BoolToFavoritoTextConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return (bool)value ? "Desfavoritar" : "Favoritar";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
