using System;
using System.Collections;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace ArenaVirtual.Converters {
    // Objetivo: Retornar TRUE se a lista estiver VAZIA (para exibir a mensagem de lista vazia)
    public class EmptyListInvertConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {

            // 1. Verifica se o valor é uma coleção (lista)
            if (value is ICollection collection) {
                // 🎯 CORREÇÃO: Retorna TRUE se a contagem for zero (lista vazia)
                return collection.Count == 0;
            }

            // 2. Se for nulo, também consideramos como vazio (retorna TRUE)
            // Se não for uma coleção (por exemplo, null), retorna TRUE para mostrar a mensagem
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException("ConvertBack não implementado em EmptyListInvertConverter.");
        }
    }
}