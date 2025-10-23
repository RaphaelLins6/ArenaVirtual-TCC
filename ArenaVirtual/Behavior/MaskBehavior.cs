using Microsoft.Maui.Controls;
using System.Linq;
using System; 

namespace ArenaVirtual.Behaviors {
    public class MaskBehavior : Behavior<Entry> {
        public static readonly BindableProperty MaskProperty =
            BindableProperty.Create(nameof(Mask), typeof(string), typeof(MaskBehavior), string.Empty); 

        public string Mask {
            get => (string)GetValue(MaskProperty);
            set => SetValue(MaskProperty, value);
        }

        protected override void OnAttachedTo(Entry entry) {
            entry.TextChanged += OnEntryTextChanged;
            entry.Focused += OnEntryFocused; 
            entry.Unfocused += OnEntryUnfocused; 

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

        private void OnEntryFocused(object sender, FocusEventArgs e) {
            var entry = (Entry)sender;
        }

        private void OnEntryUnfocused(object sender, FocusEventArgs e) {
            var entry = (Entry)sender;
        }

        private void OnEntryTextChanged(object sender, TextChangedEventArgs args) {
            var entry = (Entry)sender;
            ApplyMask(entry, args.OldTextValue, args.NewTextValue);
        }

        private void ApplyMask(Entry entry, string oldText, string newTextValue) {
            if (string.IsNullOrEmpty(Mask) || newTextValue == null) return;

            var text = new string(newTextValue.Where(char.IsDigit).ToArray());
            var newText = string.Empty;

            if (Mask.Equals("CNPJ")) {
                if (text.Length > 14) text = text.Substring(0, 14);
                if (text.Length > 0) newText += text.Substring(0, Math.Min(2, text.Length));
                if (text.Length > 2) newText += "." + text.Substring(2, Math.Min(3, text.Length - 2));
                if (text.Length > 5) newText += "." + text.Substring(5, Math.Min(3, text.Length - 5));
                if (text.Length > 8) newText += "/" + text.Substring(8, Math.Min(4, text.Length - 8));
                if (text.Length > 12) newText += "-" + text.Substring(12, Math.Min(2, text.Length - 12));
            } else if (Mask.Equals("PHONE")) {
                if (text.Length > 11) text = text.Substring(0, 11);

                newText = string.Empty;

                if (text.Length > 0) {
                    newText += "(" + text.Substring(0, Math.Min(2, text.Length));
                }

                if (text.Length >= 2) {
                    newText += ")";
                }

                if (text.Length > 2) {
                    newText += " " + text.Substring(2, Math.Min(1, text.Length - 2));

                    if (text.Length > 3) {
                        newText += " " + text.Substring(3, Math.Min(4, text.Length - 3));
                    }
                }

                if (text.Length > 7) {
                    newText += "-" + text.Substring(7, Math.Min(4, text.Length - 7));
                }
            }

            if (entry.Text != newText) {
                entry.Text = newText;
                entry.CursorPosition = entry.Text.Length;
            }
        }
    }
}