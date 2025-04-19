using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaVirtuall.Converters {
    internal class BoolToRotationConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is bool booleanValue) {
                // Retorna 180 se o valor for verdadeiro, caso contrário, 0
                return booleanValue ? 180 : 0;
            }
            return 0; // Valor padrão
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException("ConvertBack não é suportado para BoolToRotationConverter.");
        }
    }
}
