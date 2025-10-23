using ArenaVirtual.Models;
using ArenaVirtual.Services;
using System.Diagnostics;
using CommunityToolkit.Maui.Views;
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.ComponentModel;

namespace ArenaVirtual.Popups;

public partial class AlterarSenhaPopup : ContentPage, INotifyPropertyChanged {
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private bool _isBusy;
    public bool IsBusy {
        get => _isBusy;
        set {
            if (_isBusy != value) {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }
    }

    private readonly Usuario _usuario;
    private readonly IAlertService _alertService;
    private readonly DatabaseService _databaseService;
    private readonly SyncService _syncService;

    public AlterarSenhaPopup(Usuario usuario, IAlertService alertService, DatabaseService databaseService, SyncService syncService) {
        InitializeComponent();
        _usuario = usuario;
        _alertService = alertService;
        _databaseService = databaseService;
        _syncService = syncService;

        BindingContext = this;
    }

    private async void Cancelar_Clicked(object sender, EventArgs e) {
        await Navigation.PopModalAsync();
    }

    private async void Salvar_Clicked(object sender, EventArgs e) {
        if (IsBusy) return;

        string senhaAtual = SenhaAtualEntry.Text?.Trim() ?? string.Empty;
        string novaSenha = NovaSenhaEntry.Text?.Trim() ?? string.Empty;
        string confirmarNovaSenha = ConfirmarNovaSenhaEntry.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(senhaAtual) ||
          string.IsNullOrWhiteSpace(novaSenha) ||
          string.IsNullOrWhiteSpace(confirmarNovaSenha)) {
            await _alertService.DisplayAlert("Erro", "Preencha todos os campos.", "OK");
            return;
        }

        if (!BCrypt.Net.BCrypt.Verify(senhaAtual, _usuario.SenhaHash)) {
            await _alertService.DisplayAlert("Erro", "Senha atual incorreta.", "OK");
            return;
        }

        if (novaSenha != confirmarNovaSenha) {
            await _alertService.DisplayAlert("Erro", "As novas senhas não coincidem.", "OK");
            return;
        }

        IsBusy = true; 

        try {
            _usuario.SenhaHash = UsuarioService.GerarHash(novaSenha);

            _usuario.IsSynced = false;
            _usuario.UpdatedAt = DateTime.UtcNow;

            int rowsAffected = await _databaseService.AtualizarUsuarioAsync(_usuario);

            if (rowsAffected > 0) {
                if (App.CurrentUser != null && App.CurrentUser.Id == _usuario.Id) {
                    App.CurrentUser.SenhaHash = _usuario.SenhaHash;
                }

                //Debug.WriteLine("[AlterarSenhaPopup] Senha do usuário atualizada localmente. Disparando sincronização...");

                await _syncService.SyncAsync(new Progress<string>());

                await _alertService.DisplayAlert("Sucesso", "Senha atualizada com sucesso!", "OK");
                await Navigation.PopModalAsync();
            } else {
                await _alertService.DisplayAlert("Erro", "Falha ao atualizar a senha no banco de dados. Nenhuma linha afetada.", "OK");
            }
        } catch (Exception ex) {
            await _alertService.DisplayAlert("Erro", $"Ocorreu um erro ao salvar a senha: {ex.Message}", "OK");
        } finally {
            IsBusy = false; 
        }
    }
}
