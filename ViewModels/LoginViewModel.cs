using ArenaVirtual.Services;
using ArenaVirtual.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;

namespace ArenaVirtual.ViewModels {
    public partial class LoginViewModel : ObservableObject {
        private readonly IAlertService _alertService;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string senha = string.Empty;

        public LoginViewModel(IAlertService alertService) {
            _alertService = alertService;
        }

        [RelayCommand]
        public async Task Login() {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha)) {
                await _alertService.DisplayAlert("Erro", "Preencha o e-mail e a senha.", "OK");
                return;
            }

            string senhaHash = DatabaseService.GerarHash(Senha);
            var usuario = await App.Database.ObterUsuarioPorEmailSenhaAsync(Email, senhaHash);

            if (usuario == null) {
                await _alertService.DisplayAlert("Erro", "E-mail ou senha inválidos.", "OK");
                return;
            }

            Application.Current.MainPage = new AppShell(usuario);
        }

        [RelayCommand]
        public async Task IrParaRegistro() {
            var serviceProvider = Application.Current.Handler.MauiContext.Services;
            Application.Current.MainPage = serviceProvider.GetService<RegisterPage>();
        }
    }
}
