using Microsoft.Maui.Controls;
using System.Linq;
using System; // Adicione o using System

namespace ArenaVirtual.Behaviors {
    public class MaskBehavior : Behavior<Entry> {
        public static readonly BindableProperty MaskProperty =
            BindableProperty.Create(nameof(Mask), typeof(string), typeof(MaskBehavior), string.Empty); // Removemos o propertyChanged

        public string Mask {
            get => (string)GetValue(MaskProperty);
            set => SetValue(MaskProperty, value);
        }

        protected override void OnAttachedTo(Entry entry) {
            // O objeto 'entry' é o nosso AssociatedObject
            entry.TextChanged += OnEntryTextChanged;
            entry.Focused += OnEntryFocused; // Adicionar para aplicar máscara em foco, se necessário.
            entry.Unfocused += OnEntryUnfocused; // Adicionar para garantir formatação inicial.

            // ⭐️ Chamada inicial para garantir que o valor já preenchido seja mascarado ⭐️
            if (!string.IsNullOrEmpty(entry.Text)) {
                ApplyMask(entry, entry.Text, entry.Text);
            }

            base.OnAttachedTo(entry);
        }

        protected override void OnDetachingFrom(Entry entry) {
            entry.TextChanged -= OnEntryTextChanged;
            entry.Focused -= OnEntryFocused;
            entry.Unfocused -= OnEntryUnfocused;
            base.OnDetachingFrom(entry);
        }

        // Métodos de foco são úteis para garantir que o comportamento seja aplicado
        private void OnEntryFocused(object sender, FocusEventArgs e) {
            var entry = (Entry)sender;
            // Opcional: Remover máscara ao focar para facilitar a digitação
        }

        private void OnEntryUnfocused(object sender, FocusEventArgs e) {
            var entry = (Entry)sender;
            // Opcional: Reaplicar máscara ao perder o foco (já é feito pelo TextChanged)
        }

        private void OnEntryTextChanged(object sender, TextChangedEventArgs args) {
            var entry = (Entry)sender;
            ApplyMask(entry, args.OldTextValue, args.NewTextValue);
        }

        private void ApplyMask(Entry entry, string oldText, string newTextValue) {
            if (string.IsNullOrEmpty(Mask) || newTextValue == null) return;

            // 1. Limpa o valor (remove não dígitos)
            var text = new string(newTextValue.Where(char.IsDigit).ToArray());
            var newText = string.Empty;

            // 2. Aplica a máscara com base no tipo
            if (Mask.Equals("CNPJ")) {
                if (text.Length > 14) text = text.Substring(0, 14);
                if (text.Length > 0) newText += text.Substring(0, Math.Min(2, text.Length));
                if (text.Length > 2) newText += "." + text.Substring(2, Math.Min(3, text.Length - 2));
                if (text.Length > 5) newText += "." + text.Substring(5, Math.Min(3, text.Length - 5));
                if (text.Length > 8) newText += "/" + text.Substring(8, Math.Min(4, text.Length - 8));
                if (text.Length > 12) newText += "-" + text.Substring(12, Math.Min(2, text.Length - 12));
            } else if (Mask.Equals("PHONE")) {
                // Limita a entrada a 11 dígitos
                if (text.Length > 11) text = text.Substring(0, 11);

                newText = string.Empty;

                // 1. DDD: (XX
                if (text.Length > 0) {
                    newText += "(" + text.Substring(0, Math.Min(2, text.Length));
                }

                // 2. Fechar DDD: (XX)
                if (text.Length >= 2) {
                    newText += ")";
                }

                // 3. Nono Dígito e Primeiro Grupo (para 11 dígitos: 9 XXXX)
                if (text.Length > 2) {
                    // Pega o nono dígito (índice 2)
                    newText += " " + text.Substring(2, Math.Min(1, text.Length - 2));

                    if (text.Length > 3) {
                        // Pega os 4 dígitos seguintes (índice 3)
                        newText += " " + text.Substring(3, Math.Min(4, text.Length - 3));
                    }
                }

                // 4. Segundo Grupo e Traço (para 11 dígitos: -XXXX)
                if (text.Length > 7) {
                    // Adiciona o traço e o último grupo de 4 (índice 7)
                    newText += "-" + text.Substring(7, Math.Min(4, text.Length - 7));
                }
            }

            // 3. Atualiza o campo se houve mudança
            if (entry.Text != newText) {
                // Verifica se a mudança é o resultado da nossa própria aplicação de máscara para evitar loop infinito
                entry.Text = newText;
                // Posiciona o cursor no final do texto mascarado
                entry.CursorPosition = entry.Text.Length;
            }
        }
    }
}