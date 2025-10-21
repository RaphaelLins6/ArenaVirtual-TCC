using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace ArenaVirtual.Converters {
    public class SelectedStatTextConverter : IValueConverter {
        // Define as cores
        private static readonly Color SelectedTextColor = Color.FromHex("#FFFFFF"); // Branco (Contrasta com Laranja)
        private static readonly Color UnselectedTextColor = Color.FromHex("#AAAAAA"); // Cinza Claro (Contrasta com Cinza Escuro)

        // Value: É o valor da propriedade "EstatisticaSelecionada" (ex: "Pontos")
        // Parameter: É o valor do item atual no FlexLayout (ex: "Pontos", "Assistências", etc.)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string selectedStat && parameter is string currentStat) {
                // Verifica se a estatística atual é a estatística selecionada
                if (selectedStat.Equals(currentStat, StringComparison.OrdinalIgnoreCase)) {
                    return SelectedTextColor;
                }
            }
            // Retorna a cor padrão para itens não selecionados
            return UnselectedTextColor;
        }

        // Não é necessário para o seu caso (Binding Mode OneWay)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}