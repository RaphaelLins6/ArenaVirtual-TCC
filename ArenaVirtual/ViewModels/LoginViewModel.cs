using ArenaVirtual.Services;
using ArenaVirtual.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace ArenaVirtual.ViewModels {
    public partial class LoginViewModel(IAlertService alertService, UsuarioService usuarioService, SyncService syncService) : ObservableObject {
        private readonly IAlertService _alertService = alertService;
        private readonly UsuarioService _usuarioService = usuarioService;
        private readonly SyncService _syncService = syncService;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string senha = string.Empty;

        // Adiciona a nova propriedade para controlar a sobreposição de carregamento
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private bool isBusy;

        [RelayCommand(CanExecute = nameof(CanLogin))]
        public async Task Login() {
            // Define IsBusy como true para iniciar o carregamento e desabilitar o botão
            IsBusy = true;

            try {
                // 1. Validações iniciais
                if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha)) {
                    await _alertService.DisplayAlert("Erro", "Preencha o e-mail e a senha.", "OK");
                    return;
                }

                // 2. Autentica o usuário
                var usuario = await _usuarioService.Autenticar(Email, Senha);

                if (usuario == null) {
                    await _alertService.DisplayAlert("Erro", "E-mail ou senha inválidos.", "OK");
                    return;
                }

                // 3. Define a sessão do usuário
                SessaoService.Instancia.Login(usuario);
                System.Diagnostics.Debug.WriteLine($"[LoginViewModel] SessaoService.Instancia.Login() chamado para ID: {usuario.Id}, Email: {usuario.Email}");

                // 4. Inicia a sincronização de dados e aguarda a conclusão
                // A UI fica "congelada" com o overlay de loading visível.
                Debug.WriteLine("[LoginViewModel] Login bem-sucedido. Disparando sincronização.");
                await _syncService.SyncAsync(null);

                Debug.WriteLine("[LoginViewModel] Sincronização concluída. Navegando para a página principal.");

                // 5. Navega para a página principal (AppShell)
                Application.Current.MainPage = new AppShell(usuario);
            } catch (Exception ex) {
                Debug.WriteLine($"[LoginViewModel] Erro no processo de login/sincronização: {ex.Message}");
                await _alertService.DisplayAlert("Erro", "Ocorreu um erro. Tente novamente ou verifique sua conexão.", "OK");
            } finally {
                // Garante que o estado de ocupado seja resetado, independentemente do resultado
                IsBusy = false;
            }
        }

        // Método para desabilitar o botão de login enquanto a tarefa estiver rodando
        private bool CanLogin() => !IsBusy;

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