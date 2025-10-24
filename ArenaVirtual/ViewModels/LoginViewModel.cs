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

        public void UpdateConnectivityStatus() {
            IsOffline = !connectivityService.IsConnected;
            //Debug.WriteLine($"[LoginViewModel] Status de conectividade atualizado. Está offline: {IsOffline}");
        }

        [RelayCommand]
        private async Task Login() {
            //Debug.WriteLine($"[LoginViewModel] Login iniciado. Email: {Email}, Senha preenchida: {!string.IsNullOrEmpty(Senha)}");

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha)) {
                await alertService.DisplayAlert("Erro", "Preencha o e-mail e a senha.", "OK");
                //Debug.WriteLine("[LoginViewModel] Falha: email ou senha em branco.");
                return;
            }

            IsBusy = true;

            try {
                if (IsOffline) {
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

                //Debug.WriteLine("[LoginViewModel] Tentando autenticação online...");
                var usuario = await usuarioService.Autenticar(Email, Senha);

                if (usuario == null) {
                    //Debug.WriteLine("[LoginViewModel] Autenticação falhou. Usuário nulo.");
                    await alertService.DisplayAlert("Erro", "E-mail ou senha inválidos.", "OK");
                    return;
                }

                SessaoService.Instancia.Login(usuario);
                //Debug.WriteLine($"[LoginViewModel] Login inicial OK. Sessão criada para {usuario.Email} (ID {usuario.ClientAppId})");

                await syncService.SyncAsync(null);
                //Debug.WriteLine("[LoginViewModel] Sincronização concluída.");

                App.CurrentUser = await usuarioService.GetUsuarioByEmailAsync(Email);
                //Debug.WriteLine($"[LoginViewModel] Usuario retornado após sync: {(App.CurrentUser == null ? "NULL" : App.CurrentUser.Email)}");

                if (App.CurrentUser == null || App.CurrentUser.Id == 0) {
                    throw new Exception("Falha na sincronização do usuário.");
                }

                SessaoService.Instancia.Login(App.CurrentUser);
                //Debug.WriteLine($"[LoginViewModel] Sessão atualizada com usuário {App.CurrentUser.Email} (ID {App.CurrentUser.ClientAppId}).");

                Application.Current.MainPage = new AppShell(App.CurrentUser, serviceProvider);
                //Debug.WriteLine("[LoginViewModel] Navegação para AppShell concluída.");
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
    }
}
