using System.Globalization;
using ArenaVirtual.Models;

namespace ArenaVirtual.Converters {
    public class MultiPerfilToBoolConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is TipoPerfil perfilSelecionado && parameter is string perfilsString) {
                var perfis = perfilsString.Split(',');
                foreach (var perfil in perfis) {
                    if (Enum.TryParse(perfil.Trim(), out TipoPerfil targetPerfil) && perfilSelecionado == targetPerfil) {
                        return true;
                    }
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}