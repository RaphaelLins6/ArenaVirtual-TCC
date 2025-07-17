using ArenaVirtual.Models;
using ArenaVirtual.Services;

namespace ArenaVirtual.Popups;

public partial class AlterarSenhaPopup : ContentPage {
    private Usuario _usuario;
    private IAlertService _alertService;
    private DatabaseService _databaseService; // Pode ser null se GetService retornar null

    public AlterarSenhaPopup(Usuario usuario, IAlertService alertService) {
        InitializeComponent();
        _usuario = usuario;
        _alertService = alertService;

        _databaseService = App.Current?.Handler?.MauiContext?.Services?.GetRequiredService<DatabaseService>()
                           ?? throw new InvalidOperationException("DatabaseService not registered or app context is null.");
    }

    private async void Cancelar_Clicked(object sender, EventArgs e) {
        await Navigation.PopModalAsync();
    }

    private async void Salvar_Clicked(object sender, EventArgs e) {
        string senhaAtual = SenhaAtualEntry.Text?.Trim() ?? string.Empty;
        string novaSenha = NovaSenhaEntry.Text?.Trim() ?? string.Empty;
        string confirmarNovaSenha = ConfirmarNovaSenhaEntry.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(senhaAtual) ||
            string.IsNullOrWhiteSpace(novaSenha) ||
            string.IsNullOrWhiteSpace(confirmarNovaSenha)) {
            await _alertService.DisplayAlert("Erro", "Preencha todos os campos.", "OK");
            return;
        }

        if (_usuario.Senha != UsuarioService.GerarHash(senhaAtual)) {
            await _alertService.DisplayAlert("Erro", "Senha atual incorreta.", "OK");
            return;
        }

        if (novaSenha != confirmarNovaSenha) {
            await _alertService.DisplayAlert("Erro", "As novas senhas não coincidem.", "OK");
            return;
        }

        _usuario.Senha = UsuarioService.GerarHash(novaSenha);
        await _databaseService.AtualizarUsuarioAsync(_usuario);

        await _alertService.DisplayAlert("Sucesso", "Senha atualizada com sucesso!", "OK");
        await Navigation.PopModalAsync();
    }
}