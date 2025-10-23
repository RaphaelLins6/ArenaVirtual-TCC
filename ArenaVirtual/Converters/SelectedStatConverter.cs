using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace ArenaVirtual.Converters {
    public class SelectedStatConverter : IValueConverter {
        private static readonly Color SelectedBackgroundColor = Color.FromHex("#FF8C00"); 
        private static readonly Color UnselectedBackgroundColor = Color.FromHex("#333333"); 

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string selectedStat && parameter is string currentStat) {
                if (selectedStat.Equals(currentStat, StringComparison.OrdinalIgnoreCase)) {
                    return SelectedBackgroundColor;
                }
            }
            return UnselectedBackgroundColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}