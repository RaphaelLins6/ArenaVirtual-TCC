using ArenaVirtual.Services;
using ArenaVirtual.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Threading.Tasks;
using System;
using ArenaVirtual.Models; // Adicionado para acessar o modelo de usuário, se necessário

namespace ArenaVirtual.ViewModels {
    public partial class LoginViewModel(IAlertService alertService, UsuarioService usuarioService, SyncService syncService) : ObservableObject {
        private readonly IAlertService _alertService = alertService;
        private readonly UsuarioService _usuarioService = usuarioService;
        private readonly SyncService _syncService = syncService;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string senha = string.Empty;

        [ObservableProperty]
        private bool isBusy = false;

        [RelayCommand]
        private async Task Login() {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha)) {
                await _alertService.DisplayAlert("Erro", "Preencha o e-mail e a senha.", "OK");
                return;
            }

            IsBusy = true;

            try {
                var usuario = await _usuarioService.Autenticar(Email, Senha);

                if (usuario == null) {
                    await _alertService.DisplayAlert("Erro", "E-mail ou senha inválidos.", "OK");
                    return;
                }

                SessaoService.Instancia.Login(usuario);
                Debug.WriteLine($"[LoginViewModel] SessaoService.Instancia.Login() chamado para ID: {usuario.Id}, Email: {usuario.Email}");

                // CORREÇÃO CRÍTICA:
                // Define o usuário atual estático da aplicação.
                // Isso garante que outras partes do app, como a Dashboard,
                // possam acessar o usuário logado para filtrar dados corretamente.
                App.CurrentUser = usuario;

                Debug.WriteLine("[LoginViewModel] Login bem-sucedido. Disparando sincronização.");
                await _syncService.SyncAsync(null);

                Debug.WriteLine("[LoginViewModel] Sincronização concluída. Navegando para a página principal.");

                Application.Current.MainPage = new AppShell(usuario);
            } catch (Exception ex) {
                Debug.WriteLine($"[LoginViewModel] Erro no processo de login/sincronização: {ex.Message}");
                await _alertService.DisplayAlert("Erro", "Ocorreu um erro. Tente novamente ou verifique sua conexão.", "OK");
            } finally {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task IrParaRegistro() {
            var localServiceProvider = Application.Current?.Handler?.MauiContext?.Services;

            if (localServiceProvider != null) {
                var registerPage = localServiceProvider.GetService<RegisterPage>();
                if (registerPage != null && Application.Current?.Windows.Count > 0) {
                    Application.Current.Windows[0].Page = registerPage;
                } else {
                    await _alertService.DisplayAlert("Erro", "Página de registro não pôde ser carregada. Contate o suporte.", "OK");
                }
            } else {
                await _alertService.DisplayAlert("Erro", "Serviços do aplicativo não disponíveis. Contate o suporte.", "OK");
            }
        }
    }
}