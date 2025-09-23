using ArenaVirtual.Services;
using ArenaVirtual.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Threading.Tasks;

namespace ArenaVirtual.ViewModels {
    // Adicione IServiceProvider ao construtor primário para injetá-lo automaticamente
    public partial class LoginViewModel(IAlertService alertService, UsuarioService usuarioService, SyncService syncService, ConnectivityService connectivityService, IServiceProvider serviceProvider) : ObservableObject {

        // Propriedades Observáveis
        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string senha = string.Empty;

        [ObservableProperty]
        private bool isBusy = false;

        [ObservableProperty]
        private bool isOffline = false;

        public void UpdateConnectivityStatus() {
            IsOffline = !connectivityService.IsConnected;
            Debug.WriteLine($"[LoginViewModel] Status de conectividade atualizado. Está offline: {IsOffline}");
        }

        [RelayCommand]
        private async Task Login() {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha)) {
                await alertService.DisplayAlert("Erro", "Preencha o e-mail e a senha.", "OK");
                return;
            }

            IsBusy = true;

            try {
                if (IsOffline) {
                    var usuarioOffline = await usuarioService.AutenticarOffline(Email, Senha);
                    if (usuarioOffline != null) {
                        SessaoService.Instancia.Login(usuarioOffline);
                        App.CurrentUser = usuarioOffline;
                        // Passe o serviceProvider para o construtor do AppShell
                        Application.Current.MainPage = new AppShell(App.CurrentUser, serviceProvider);
                        Debug.WriteLine("[LoginViewModel] Login offline bem-sucedido.");
                        return;
                    } else {
                        await alertService.DisplayAlert("Erro", "Você está offline e não foi possível encontrar suas credenciais no dispositivo. Tente se conectar à internet.", "OK");
                        return;
                    }
                }

                var usuario = await usuarioService.Autenticar(Email, Senha);
                if (usuario == null) {
                    await alertService.DisplayAlert("Erro", "E-mail ou senha inválidos.", "OK");
                    return;
                }

                SessaoService.Instancia.Login(usuario);
                Debug.WriteLine($"[LoginViewModel] SessaoService.Instancia.Login() chamado para ID: {usuario.Id}, Email: {usuario.Email}");

                await syncService.SyncAsync(null);
                App.CurrentUser = await usuarioService.GetUsuarioByEmailAsync(Email);

                if (App.CurrentUser == null || App.CurrentUser.Id == 0) {
                    throw new Exception("Falha na sincronização do usuário.");
                }

                SessaoService.Instancia.Login(App.CurrentUser);
                Debug.WriteLine("[LoginViewModel] Login bem-sucedido. Sincronização e navegação concluídas.");

                // Passe o serviceProvider para o construtor do AppShell
                Application.Current.MainPage = new AppShell(App.CurrentUser, serviceProvider);
            } catch (Exception ex) {
                Debug.WriteLine($"[LoginViewModel] Erro no processo de login/sincronização: {ex.Message}");
                await alertService.DisplayAlert("Erro", "Ocorreu um erro. Tente novamente ou verifique sua conexão.", "OK");
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
                    await alertService.DisplayAlert("Erro", "Página de registro não pôde ser carregada. Contate o suporte.", "OK");
                }
            } else {
                await alertService.DisplayAlert("Erro", "Serviços do aplicativo não disponíveis. Contate o suporte.", "OK");
            }
        }
    }
}