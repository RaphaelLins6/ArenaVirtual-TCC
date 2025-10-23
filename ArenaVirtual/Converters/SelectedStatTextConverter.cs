using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace ArenaVirtual.Converters {
    public class SelectedStatTextConverter : IValueConverter {
        private static readonly Color SelectedTextColor = Color.FromHex("#FFFFFF"); // Branco (Contrasta com Laranja)
        private static readonly Color UnselectedTextColor = Color.FromHex("#AAAAAA"); // Cinza Claro (Contrasta com Cinza Escuro)

 
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string selectedStat && parameter is string currentStat) {
                if (selectedStat.Equals(currentStat, StringComparison.OrdinalIgnoreCase)) {
                    return SelectedTextColor;
                }
            }
            return UnselectedTextColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}