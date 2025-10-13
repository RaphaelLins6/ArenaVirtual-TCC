using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace ArenaVirtual.Converters {
    // CRIE ESTA CLASSE
    public class IntToPhaseNameConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is int rodada) {
                // Mapeia o número da Rodada para o nome da Fase
                return rodada switch {
                    1 => "Semi-Final", // Na lógica de 3 ou 4 times, Rodada 1 é Semi
                    2 => "Final",
                    // Adicione mais mapeamentos se houver Quartas (Rodada 3), Oitavas (Rodada 4), etc.
                    _ => $"Rodada {rodada}"
                };
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}