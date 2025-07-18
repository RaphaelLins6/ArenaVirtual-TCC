using ArenaVirtual.Models;
using ArenaVirtual.Services;

namespace ArenaVirtual.Popups;

public partial class AlterarImagemPopup : ContentPage {
    private readonly Usuario _usuario;
    private readonly IAlertService _alertService;
    private readonly DatabaseService _databaseService;

    public event EventHandler<string>? ImagemAtualizada;

    private string? _caminhoNovaImagemSelecionada;

    public AlterarImagemPopup(Usuario usuario, IAlertService alertService) {
        InitializeComponent();
        _usuario = usuario;
        _alertService = alertService;

        var serviceProvider = App.Current?.Handler?.MauiContext?.Services;
        if (serviceProvider != null) {
            _databaseService = serviceProvider.GetRequiredService<DatabaseService>();
        } else {
            throw new InvalidOperationException("DatabaseService not registered or app context is null.");
        }

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
            await _databaseService.AtualizarUsuarioAsync(_usuario);

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