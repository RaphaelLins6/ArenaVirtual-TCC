using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace ArenaVirtual.Converters {
    public class SelectedStatConverter : IValueConverter {
        // Define as cores (ajuste conforme o tema da sua aplicação)
        private static readonly Color SelectedBackgroundColor = Color.FromHex("#FF8C00"); // Laranja Escuro (Destaque)
        private static readonly Color UnselectedBackgroundColor = Color.FromHex("#333333"); // Cinza Escuro (Neutro)

        // Value: É o valor da propriedade "EstatisticaSelecionada" (ex: "Pontos")
        // Parameter: É o valor do item atual no FlexLayout (ex: "Pontos", "Assistências", etc.)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string selectedStat && parameter is string currentStat) {
                // Verifica se a estatística atual é a estatística selecionada
                if (selectedStat.Equals(currentStat, StringComparison.OrdinalIgnoreCase)) {
                    return SelectedBackgroundColor;
                }
            }
            // Retorna a cor padrão para itens não selecionados ou em caso de falha de binding
            return UnselectedBackgroundColor;
        }

        // Não é necessário para o seu caso (Binding Mode OneWay)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}