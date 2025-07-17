using ArenaVirtual.Services;
using ArenaVirtual.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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

            var usuario = await _usuarioService.Autenticar(Email, Senha);

            if (usuario == null) {
                await _alertService.DisplayAlert("Erro", "E-mail ou senha inválidos.", "OK");
                return;
            }

            if (Application.Current?.Windows.Count > 0) {
                if (usuario != null) {
                    Application.Current.Windows[0].Page = new AppShell(usuario);
                } else {
                    await _alertService.DisplayAlert("Erro", "Falha ao carregar o perfil do usuário.", "OK");
                }
            } else {
                await _alertService.DisplayAlert("Erro", "Nenhuma janela do aplicativo disponível.", "OK");
            }
        }

        [RelayCommand]
        public async Task IrParaRegistro() {
            var localServiceProvider = Application.Current?.Handler?.MauiContext?.Services;

            if (localServiceProvider != null) {
                var registerPage = localServiceProvider.GetService<RegisterPage>();

                if (registerPage != null) {
                    if (Application.Current?.Windows.Count > 0) {
                        Application.Current.Windows[0].Page = registerPage;
                    } else {
                        await _alertService.DisplayAlert("Erro", "Nenhuma janela do aplicativo disponível.", "OK");
                    }
                } else {
                    await _alertService.DisplayAlert("Erro", "Página de registro não pôde ser carregada. Contate o suporte.", "OK");
                }
            } else {
                await _alertService.DisplayAlert("Erro", "Serviços do aplicativo não disponíveis. Contate o suporte.", "OK");
            }
            await Task.CompletedTask;
        }
    }
}