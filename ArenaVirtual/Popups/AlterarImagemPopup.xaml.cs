using ArenaVirtual.Models;
using ArenaVirtual.Services;
using System.Diagnostics;
using System.IO;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace ArenaVirtual.Popups;

public partial class AlterarImagemPopup : ContentPage {
    private readonly Usuario _usuario;
    private readonly IAlertService _alertService;
    private readonly DatabaseService _databaseService;
    private readonly SyncService _syncService;

    public event EventHandler<string>? ImagemAtualizada;

    private string? _caminhoNovaImagemSelecionada;
    // Adicione uma variável de controle para o estado de salvamento
    private bool _isSaving = false;

    public AlterarImagemPopup(Usuario usuario, IAlertService alertService, DatabaseService databaseService, SyncService syncService) {
        InitializeComponent();
        _usuario = usuario;
        _alertService = alertService;
        _databaseService = databaseService;
        _syncService = syncService;

        AtualizarImagemUI(_usuario.ImagemPath);
    }

    private void AtualizarImagemUI(string? caminhoImagem) {
        if (!string.IsNullOrEmpty(caminhoImagem) && File.Exists(caminhoImagem)) {
            try {
                ImagemPerfil.Source = ImageSource.FromFile(caminhoImagem);
            } catch {
                ImagemPerfil.Source = "default_profile.png";
            }
        } else {
            ImagemPerfil.Source = "default_profile.png";
        }
    }

    private async void EscolherImagem_Clicked(object sender, EventArgs e) {
        try {
            var result = await FilePicker.PickAsync(new PickOptions {
                PickerTitle = "Selecione uma imagem",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null && !string.IsNullOrEmpty(result.FullPath)) {
                _caminhoNovaImagemSelecionada = result.FullPath;
                AtualizarImagemUI(_caminhoNovaImagemSelecionada);
            }
        } catch (Exception ex) {
            await _alertService.DisplayAlert("Erro", $"Não foi possível escolher a imagem: {ex.Message}", "OK");
        }
    }

    private async void Salvar_Clicked(object sender, EventArgs e) {
        // Verifica a variável de controle para evitar múltiplos cliques
        if (_isSaving) return;

        if (string.IsNullOrEmpty(_caminhoNovaImagemSelecionada)) {
            await _alertService.DisplayAlert("Aviso", "Por favor, escolha uma imagem primeiro.", "OK");
            return;
        }

        _isSaving = true; // Inicia o estado de salvamento

        try {
            string diretorioImagens = FileSystem.AppDataDirectory;
            string nomeArquivo = Path.GetFileName(_caminhoNovaImagemSelecionada);
            string caminhoFinalImagem = Path.Combine(diretorioImagens, nomeArquivo);

            // Otimização: evite a cópia se a imagem já estiver no diretório correto.
            if (!File.Exists(caminhoFinalImagem)) {
                File.Copy(_caminhoNovaImagemSelecionada, caminhoFinalImagem, true);
            }

            _usuario.ImagemPath = caminhoFinalImagem;
            _usuario.IsSynced = false;
            _usuario.UpdatedAt = DateTime.UtcNow;

            await _databaseService.AtualizarUsuarioAsync(_usuario);

            Debug.WriteLine("[AlterarImagemPopup] Imagem de usuário atualizada localmente. Disparando sincronização...");
            await _syncService.SyncAsync(new Progress<string>());

            ImagemAtualizada?.Invoke(this, _usuario.ImagemPath);

            await _alertService.DisplayAlert("Sucesso", "Imagem de perfil atualizada!", "OK");
            await Navigation.PopModalAsync();
        } catch (Exception ex) {
            await _alertService.DisplayAlert("Erro", $"Erro ao salvar imagem: {ex.Message}", "OK");
        } finally {
            _isSaving = false; // Finaliza o estado de salvamento
        }
    }

    private async void Cancelar_Clicked(object sender, EventArgs e) {
        await Navigation.PopModalAsync();
    }
}