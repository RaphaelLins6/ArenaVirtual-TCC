using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace ArenaVirtual.Converters {
    public class StatusToColorConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string statusString) {
                // Converte a string para minúsculas para comparação case-insensitive
                string status = statusString.ToLowerInvariant().Trim();

                // Define as cores com base no status (você pode ajustar as cores)
                if (status == "ativa") {
                    // Verde para Status Ativo
                    return Color.FromArgb("#2ECC71"); // Verde esmeralda
                } else if (status == "finalizada" || status == "inativa") {
                    // Cinza para Status Finalizado/Inativo
                    return Color.FromArgb("#95A5A6"); // Cinza claro
                } else if (status == "pendente") {
                    // Laranja/Amarelo para Status Pendente
                    return Color.FromArgb("#F39C12"); // Laranja
                }

                // Cor padrão para qualquer outro status
                return Colors.Black;
            }

            return Colors.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            // A conversão de volta geralmente não é usada para conversores de cor, 
            // mas é exigida pela interface.
            return null;
        }
    }
}