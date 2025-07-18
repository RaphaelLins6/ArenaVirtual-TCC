using ArenaVirtual.Models;
using ArenaVirtual.Services;
using System.Diagnostics;
using CommunityToolkit.Maui.Views; 

namespace ArenaVirtual.Popups;

public partial class AlterarSenhaPopup : ContentPage {
    private readonly Usuario _usuario;
    private readonly IAlertService _alertService;
    private readonly DatabaseService _databaseService;

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

// Troque a verificação da senha atual para usar BCrypt.Verify
        if (!BCrypt.Net.BCrypt.Verify(senhaAtual, _usuario.SenhaHash)) {
            await _alertService.DisplayAlert("Erro", "Senha atual incorreta.", "OK");
            return;
        }

        if (novaSenha != confirmarNovaSenha) {
            await _alertService.DisplayAlert("Erro", "As novas senhas não coincidem.", "OK");
            return;
        }

        _usuario.SenhaHash = UsuarioService.GerarHash(novaSenha);

        try {
            int rowsAffected = await _databaseService.AtualizarUsuarioAsync(_usuario);

            if (rowsAffected > 0) {
                if (App.CurrentUser != null && App.CurrentUser.Id == _usuario.Id) {
                    App.CurrentUser.SenhaHash = _usuario.SenhaHash;
                }

                await _alertService.DisplayAlert("Sucesso", "Senha atualizada com sucesso!", "OK");
                await Navigation.PopModalAsync();
            } else {
                await _alertService.DisplayAlert("Erro", "Falha ao atualizar a senha no banco de dados. Nenhuma linha afetada.", "OK");
            }
        } catch (Exception ex) {
            await _alertService.DisplayAlert("Erro", $"Ocorreu um erro ao salvar a senha: {ex.Message}", "OK");
        }
    }
}