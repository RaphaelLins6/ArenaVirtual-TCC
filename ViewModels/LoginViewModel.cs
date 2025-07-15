using ArenaVirtual.Models;
using ArenaVirtual.Views;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ArenaVirtual.ViewModels {
    public partial class LoginViewModel : ObservableObject {
        private string email = string.Empty;
        private string senha = string.Empty;

        public string Email {
            get => email;
            set => SetProperty(ref email, value);
        }

        public string Senha {
            get => senha;
            set => SetProperty(ref senha, value);
        }

        [RelayCommand]
        private async Task EntrarAsync() {
            var emailValue = this.email; 
            var senhaValue = this.senha; 
            Usuario? usuarioAutenticado = await UsuarioService.Autenticar(emailValue, senhaValue);

            if (usuarioAutenticado != null) {
                await Shell.Current.DisplayAlert("Login", "Login concluído", "OK");
                if (Application.Current?.Windows.Count > 0) {
                    Application.Current.Windows[0].Page = new AppShell(usuarioAutenticado);
                }
            } else {
                await Shell.Current.DisplayAlert("Erro de Login", "Email ou senha inválidos.", "OK");
            }
        }

        [RelayCommand]
        private async Task RegistrarAsync() {
            await Shell.Current.GoToAsync(nameof(RegisterPage));
        }
    }
}