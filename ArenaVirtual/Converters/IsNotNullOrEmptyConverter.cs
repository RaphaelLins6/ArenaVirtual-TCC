using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace ArenaVirtual.Converters {
    // O conversor implementa a interface IValueConverter
    public class IsNotNullOrEmptyConverter : IValueConverter {
        // Este método é chamado para converter o valor da origem (ViewModel) para a View (XAML)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            // Verifica se o valor é uma string
            if (value is string stringValue) {
                // Retorna true se a string não for nula ou vazia, caso contrário, false
                return !string.IsNullOrWhiteSpace(stringValue);
            }

            // Para outros tipos de valor, a Label permanecerá oculta
            return false;
        }

        // Este método é chamado para converter o valor da View para a origem.
        // Não é necessário para este caso, então apenas lançamos uma exceção.
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
