using ArenaVirtual.Services;
using ArenaVirtual.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace ArenaVirtual.ViewModels {
    public partial class LoginViewModel : ObservableObject {
        private readonly IAlertService _alertService;
        private readonly UsuarioService _usuarioService;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string senha = string.Empty;

        public LoginViewModel(IAlertService alertService, UsuarioService usuarioService) {
            _alertService = alertService;
            _usuarioService = usuarioService;
        }

        [RelayCommand]
        public async Task Login() {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha)) {
                await _alertService.DisplayAlert("Erro", "Preencha o e-mail e a senha.", "OK");
                return;
            }

            string senhaHash = UsuarioService.GerarHash(Senha);
            var usuario = await _usuarioService.Autenticar(Email, senhaHash);

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
            await Task.CompletedTask;
        }
    }
}
