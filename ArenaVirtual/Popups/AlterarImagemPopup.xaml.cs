using ArenaVirtual.Models;
using ArenaVirtual.Services;
using System.Diagnostics;

namespace ArenaVirtual.Popups;

public partial class AlterarImagemPopup : ContentPage {
    private readonly Usuario _usuario;
    private readonly IAlertService _alertService;
    private readonly DatabaseService _databaseService;
    private readonly SyncService _syncService;

    public event EventHandler<string>? ImagemAtualizada;

    private string? _caminhoNovaImagemSelecionada;

    // Injeção de dependências no construtor
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
                byte[] imageBytes = File.ReadAllBytes(caminhoImagem);
                ImagemPerfil.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
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
        if (string.IsNullOrEmpty(_caminhoNovaImagemSelecionada)) {
            await _alertService.DisplayAlert("Aviso", "Por favor, escolha uma imagem primeiro.", "OK");
            return;
        }

        try {
            string diretorioImagens = FileSystem.AppDataDirectory;
            string nomeArquivo = Path.GetFileName(_caminhoNovaImagemSelecionada);
            string caminhoFinalImagem = Path.Combine(diretorioImagens, nomeArquivo);

            if (_caminhoNovaImagemSelecionada != caminhoFinalImagem) {
                File.Copy(_caminhoNovaImagemSelecionada, caminhoFinalImagem, true);
            }

            _usuario.ImagemPath = caminhoFinalImagem;

            // Marcar usuário para sincronização antes de atualizar no banco de dados
            _usuario.IsSynced = false;
            _usuario.UpdatedAt = DateTime.UtcNow;

            await _databaseService.AtualizarUsuarioAsync(_usuario);

            // Disparo manual da sincronização após a atualização local
            Debug.WriteLine("[AlterarImagemPopup] Imagem de usuário atualizada localmente. Disparando sincronização...");

            // Crie e passe um objeto de progresso vazio
            await _syncService.SyncAsync(new Progress<string>());

            ImagemAtualizada?.Invoke(this, _usuario.ImagemPath);

            await _alertService.DisplayAlert("Sucesso", "Imagem de perfil atualizada!", "OK");
            await Navigation.PopModalAsync();
        } catch (Exception ex) {
            await _alertService.DisplayAlert("Erro", $"Erro ao salvar imagem: {ex.Message}", "OK");
        }
    }

    private async void Cancelar_Clicked(object sender, EventArgs e) {
        await Navigation.PopModalAsync();
    }
}