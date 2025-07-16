using ArenaVirtual.Services;
using ArenaVirtual.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ArenaVirtual.ViewModels {
    public partial class LoginViewModel : ObservableObject {
        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string senha = string.Empty;

        [RelayCommand]
        public async Task Login() {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha)) {
                await Shell.Current.DisplayAlert("Erro", "Preencha o e-mail e a senha.", "OK");
                return;
            }

            string senhaHash = DatabaseService.GerarHash(Senha);
            var usuario = await App.Database.ObterUsuarioPorEmailSenhaAsync(Email, senhaHash);

            if (usuario == null) {
                await Shell.Current.DisplayAlert("Erro", "E-mail ou senha inválidos.", "OK");
                return;
            }

            Application.Current.MainPage = new AppShell(usuario); // <- aqui é o correto
        }

        [RelayCommand]
        public async Task IrParaRegistro() {
            Application.Current.MainPage = new RegisterPage();
        }
    }
}
