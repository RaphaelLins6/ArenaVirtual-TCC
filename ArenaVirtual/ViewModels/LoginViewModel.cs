using ArenaVirtual.Services;
using ArenaVirtual.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Threading.Tasks;

namespace ArenaVirtual.ViewModels {
    public partial class LoginViewModel(
        IAlertService alertService,
        UsuarioService usuarioService,
        SyncService syncService,
        ConnectivityService connectivityService,
        IServiceProvider serviceProvider
    ) : ObservableObject {

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string senha = string.Empty;

        [ObservableProperty]
        private bool isBusy = false;

        [ObservableProperty]
        private bool isOffline = false;

        [ObservableProperty]
        private bool isDarkMode = true;

        public void UpdateConnectivityStatus() {
            IsOffline = !connectivityService.IsConnected;
            //Debug.WriteLine($"[LoginViewModel] Status de conectividade atualizado. Está offline: {IsOffline}");
        }

        [RelayCommand]
        private async Task Login() {

            IsBusy = true;

            try {
                bool forcarModoOfflineParaTeste = true; 

                if (IsOffline || forcarModoOfflineParaTeste) {
                    //Debug.WriteLine("[LoginViewModel] Modo offline detectado. Tentando autenticação local...");
                    var usuarioOffline = await usuarioService.AutenticarOffline(Email, Senha);
                    if (usuarioOffline != null) {
                        SessaoService.Instancia.Login(usuarioOffline);
                        App.CurrentUser = usuarioOffline;
                        //Debug.WriteLine($"[LoginViewModel] Login offline OK. Usuário: {usuarioOffline.Email}, ID: {usuarioOffline.ClientAppId}");
                        Application.Current.MainPage = new AppShell(App.CurrentUser, serviceProvider);
                        return;
                    } else {
                        //Debug.WriteLine("[LoginViewModel] Nenhuma credencial encontrada offline.");
                        await alertService.DisplayAlert("Erro", "Você está offline e não foi possível encontrar suas credenciais no dispositivo. Tente se conectar à internet.", "OK");
                        return;
                    }
                }

                // Se forcarModoOfflineParaTeste é TRUE, o código abaixo nunca será executado

                //Debug.WriteLine("[LoginViewModel] Tentando autenticação online...");
                //var usuario = await usuarioService.Autenticar(Email, Senha); // <-- Este método será ignorado ou comentado

                // ... (Restante da lógica de autenticação online e sincronização) ...

                //if (usuario == null) {
                //    //Debug.WriteLine("[LoginViewModel] Autenticação falhou. Usuário nulo.");
                //    await alertService.DisplayAlert("Erro", "E-mail ou senha inválidos.", "OK");
                //    return;
                //}

                //SessaoService.Instancia.Login(usuario);

                // 🚨 NOTA: Se você comentar o método Autenticar, lembre-se de que a sincronização não
                // poderá ser feita aqui, pois 'usuario' será nulo ou o token estará faltando.
                // O login offline puro deve levar à AppShell imediatamente.

                await syncService.SyncAsync(null);
                //Debug.WriteLine("[LoginViewModel] Sincronização concluída.");

                App.CurrentUser = await usuarioService.GetUsuarioByEmailAsync(Email);

                if (App.CurrentUser == null || App.CurrentUser.Id == 0) {
                    throw new Exception("Falha na sincronização do usuário.");
                }

                SessaoService.Instancia.Login(App.CurrentUser);

                Application.Current.MainPage = new AppShell(App.CurrentUser, serviceProvider);

            } catch (Exception ex) {
                //Debug.WriteLine($"[LoginViewModel] Erro no processo de login/sincronização: {ex}");
                await alertService.DisplayAlert("Erro", "Ocorreu um erro. Tente novamente ou verifique sua conexão.", "OK");
            } finally {
                IsBusy = false;
                //Debug.WriteLine("[LoginViewModel] Login encerrado. IsBusy = false");
            }
        }

        [RelayCommand]
        private async Task IrParaRegistro() {
            //Debug.WriteLine("[LoginViewModel] Navegação para registro acionada.");
            var localServiceProvider = Application.Current?.Handler?.MauiContext?.Services;
            if (localServiceProvider != null) {
                var registerPage = localServiceProvider.GetService<RegisterPage>();
                if (registerPage != null && Application.Current?.Windows.Count > 0) {
                    //Debug.WriteLine("[LoginViewModel] Página de registro encontrada. Navegando...");
                    Application.Current.Windows[0].Page = registerPage;
                } else {
                    //Debug.WriteLine("[LoginViewModel] ERRO: Página de registro não encontrada.");
                    await alertService.DisplayAlert("Erro", "Página de registro não pôde ser carregada. Contate o suporte.", "OK");
                }
            } else {
                //Debug.WriteLine("[LoginViewModel] ERRO: Services do app não disponíveis.");
                await alertService.DisplayAlert("Erro", "Serviços do aplicativo não disponíveis. Contate o suporte.", "OK");
            }
        }

        [RelayCommand]
        private void ToggleTheme() {
            if (Application.Current != null) {
                //Debug.WriteLine($"[THEME DEBUG] 1. IsDarkMode ANTES do clique: {IsDarkMode}");
                IsDarkMode = !IsDarkMode;
                //Debug.WriteLine($"[THEME DEBUG] 2. IsDarkMode DEPOIS de inverter: {IsDarkMode}");
                Application.Current.UserAppTheme = IsDarkMode ? AppTheme.Dark : AppTheme.Light;
                //Debug.WriteLine($"[THEME DEBUG] 3. Tema aplicado: {Application.Current.UserAppTheme}");
            } else {
                //Debug.WriteLine("[THEME DEBUG] ERRO: Application.Current é nulo.");
            }
        }
    }
}
