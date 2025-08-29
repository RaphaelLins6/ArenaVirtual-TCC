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

            SessaoService.Instancia.Login(usuario);
            System.Diagnostics.Debug.WriteLine($"[LoginViewModel] SessaoService.Instancia.Login() chamado para ID: {usuario.Id}, Email: {usuario.Email}");

            // Inicia a sincronização em segundo plano imediatamente após o login
            Debug.WriteLine("[LoginViewModel] Login bem-sucedido. Disparando sincronização em segundo plano.");

            // A tarefa de sincronização não precisa ser "await" aqui, pois não queremos que ela bloqueie a UI.
            // O uso de `_ =` evita um aviso do compilador sobre o await não utilizado.
            _ = Task.Run(async () => {
                // A sincronização em segundo plano não reporta progresso para a tela de carregamento,
                // pois esta será fechada. O foco agora é não bloquear a UI.
                try {
                    await _syncService.SyncAsync(null); // O parâmetro IProgress<string> pode ser null ou um Progress<string> vazio se o método exigir.
                    Debug.WriteLine("[LoginViewModel] Sincronização em segundo plano concluída.");
                } catch (Exception ex) {
                    Debug.WriteLine($"[LoginViewModel] Erro na sincronização em segundo plano: {ex.Message}");
                    // Registre o erro, mas não bloqueie a UI com um pop-up.
                }
            });

            // Navegue para a página principal imediatamente
            Application.Current.MainPage = new AppShell(usuario);
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