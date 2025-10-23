using System;
using System.Collections;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace ArenaVirtual.Converters {
    public class EmptyListInvertConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {

            if (value is ICollection collection) {
                return collection.Count == 0;
            }

            
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException("ConvertBack não implementado em EmptyListInvertConverter.");
        }
    }
}