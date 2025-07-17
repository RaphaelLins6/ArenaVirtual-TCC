using ArenaVirtual.Models;
using ArenaVirtual.Services; // SecurityService não existe, use métodos de DatabaseService

namespace ArenaVirtual.Popups;

public partial class AlterarSenhaPopup : ContentPage {
    private Usuario _usuario;
    private IAlertService _alertService;
    private DatabaseService _databaseService;

    public AlterarSenhaPopup(Usuario usuario, IAlertService alertService) {
        InitializeComponent();
        _usuario = usuario;
        _alertService = alertService;
        _databaseService = App.Current.Handler.MauiContext.Services.GetService<DatabaseService>();
    }

    private async void Cancelar_Clicked(object sender, EventArgs e) {
        await Navigation.PopModalAsync();
    }

    private async void Salvar_Clicked(object sender, EventArgs e) {
        string senhaAtual = SenhaAtualEntry.Text?.Trim();
        string novaSenha = NovaSenhaEntry.Text?.Trim();
        string confirmarNovaSenha = ConfirmarNovaSenhaEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(senhaAtual) ||
            string.IsNullOrWhiteSpace(novaSenha) ||
            string.IsNullOrWhiteSpace(confirmarNovaSenha)) {
            await _alertService.DisplayAlert("Erro", "Preencha todos os campos.", "OK");
            return;
        }

        // Verifica se a senha atual confere
        if (_usuario.Senha != DatabaseService.GerarHash(senhaAtual)) {
            await _alertService.DisplayAlert("Erro", "Senha atual incorreta.", "OK");
            return;
        }

        if (novaSenha != confirmarNovaSenha) {
            await _alertService.DisplayAlert("Erro", "As novas senhas não coincidem.", "OK");
            return;
        }

        // Atualiza a senha com hash
        _usuario.Senha = DatabaseService.GerarHash(novaSenha);
        await _databaseService.AtualizarUsuarioAsync(_usuario);

        await _alertService.DisplayAlert("Sucesso", "Senha atualizada com sucesso!", "OK");
        await Navigation.PopModalAsync();
    }
}
