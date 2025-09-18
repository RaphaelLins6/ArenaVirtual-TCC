using ArenaVirtual.Models;
using ArenaVirtual.Services;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace ArenaVirtual.Popups;

public partial class AlterarImagemPopup : ContentPage, INotifyPropertyChanged {

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

    public event EventHandler<string>? ImagemAtualizada;

    private string? _caminhoNovaImagemSelecionada;

    public AlterarImagemPopup(Usuario usuario, IAlertService alertService, DatabaseService databaseService, SyncService syncService) {
        InitializeComponent();
        _usuario = usuario;
        _alertService = alertService;
        _databaseService = databaseService;
        _syncService = syncService;

        // É crucial definir o BindingContext para que o XAML "veja" as propriedades do Code-Behind.
        BindingContext = this;

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
        if (IsBusy) return;

        if (string.IsNullOrEmpty(_caminhoNovaImagemSelecionada)) {
            await _alertService.DisplayAlert("Aviso", "Por favor, escolha uma imagem primeiro.", "OK");
            return;
        }

        IsBusy = true; // Ativa o indicador de carregamento.

        try {
            string diretorioImagens = FileSystem.AppDataDirectory;
            string nomeArquivo = Path.GetFileName(_caminhoNovaImagemSelecionada);
            string caminhoFinalImagem = Path.Combine(diretorioImagens, nomeArquivo);

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
            IsBusy = false; // Desativa o indicador de carregamento.
        }
    }

    private async void Cancelar_Clicked(object sender, EventArgs e) {
        await Navigation.PopModalAsync();
    }
}