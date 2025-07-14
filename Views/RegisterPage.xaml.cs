using Microsoft.Maui.Controls;
using ArenaVirtual.Models;
using ArenaVirtual.ViewModels;
using ArenaVirtual.Services;


namespace ArenaVirtual.Views {
    public partial class RegisterPage : ContentPage {
        public RegisterPage() {
            InitializeComponent();
        }

        private void OnRegistrarClicked(object sender, EventArgs e) {
            // Obtenha os valores dos campos de entrada  
            string username = UsernameEntry?.Text?.Trim() ?? string.Empty;
            string email = EmailEntry?.Text?.Trim() ?? string.Empty; // Adicionado operador de null-coalescing para evitar CS8600
            string password = PasswordEntry?.Text ?? string.Empty; // Adicionado operador de null-coalescing para evitar CS8600
            string confirmPassword = ConfirmPasswordEntry?.Text ?? string.Empty; // Adicionado operador de null-coalescing para evitar CS8600
            var perfilTipo = ProfileTypePicker?.SelectedItem;

            // Validação simples  
            if (string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(confirmPassword) ||
                perfilTipo == null) {
                DisplayAlert("Erro", "Preencha todos os campos.", "OK");
                return;
            }

            if (password != confirmPassword) {
                DisplayAlert("Erro", "As senhas não coincidem.", "OK");
                return;
            }

            // Converta o perfilTipo para TipoPerfil  
            if (!Enum.TryParse(perfilTipo.ToString(), out TipoPerfil selectedProfileType)) {
                DisplayAlert("Erro", "Tipo de perfil inválido.", "OK");
                return;
            }

            // Crie o usuário (ajuste conforme seu modelo)  
            Usuario novoUsuario = new Usuario {
                Nome = username,
                Email = email,
                Senha = password,
                Perfil = selectedProfileType
            };

            DisplayAlert("Sucesso", "Usuário registrado com sucesso!", "OK");
        }
    }
}
