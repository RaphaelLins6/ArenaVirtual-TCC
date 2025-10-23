using ArenaVirtual.Models;
using ArenaVirtual.Services;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace ArenaVirtual.Popups;

public partial class AlterarBannerPatrocinioPopup : ContentPage, INotifyPropertyChanged {
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

    private readonly CampanhaPatrocinio _campanhaPatrocinio;
    private readonly IAlertService _alertService;
    private readonly DatabaseService _databaseService;
    private readonly SyncService _syncService;

    public event EventHandler<string>? BannerAtualizado;

    private string? _caminhoNovoBannerSelecionado;

    public AlterarBannerPatrocinioPopup(CampanhaPatrocinio campanhaPatrocinio, IAlertService alertService, DatabaseService databaseService, SyncService syncService) {
        InitializeComponent();
        _campanhaPatrocinio = campanhaPatrocinio;
        _alertService = alertService;
        _databaseService = databaseService;
        _syncService = syncService;

        BindingContext = this;
        AtualizarImagemUI(_campanhaPatrocinio.ImagemPatrocinador);
        //Debug.WriteLine($"[AlterarBannerPatrocinioPopup] Popup inicializado. ImagemPatrocinador recebido: '{_campanhaPatrocinio.ImagemPatrocinador}'");
    }

    private void AtualizarImagemUI(string? caminhoImagem) {
        if (!string.IsNullOrEmpty(caminhoImagem)) {
            //Debug.WriteLine($"[AlterarBannerPatrocinioPopup] Tentando carregar imagem do caminho: '{caminhoImagem}'");
            if (Uri.IsWellFormedUriString(caminhoImagem, UriKind.Absolute)) {
                ImagemBanner.Source = ImageSource.FromUri(new Uri(caminhoImagem));
                //Debug.WriteLine("[AlterarBannerPatrocinioPopup] Imagem carregada de URI.");
            } else if (File.Exists(caminhoImagem)) {
                ImagemBanner.Source = ImageSource.FromFile(caminhoImagem);
                //Debug.WriteLine("[AlterarBannerPatrocinioPopup] Imagem carregada de arquivo local.");
            } else {
                ImagemBanner.Source = "default_banner.png";
                //Debug.WriteLine("[AlterarBannerPatrocinioPopup] Caminho inválido, usando imagem padrão.");
            }
        } else {
            ImagemBanner.Source = "default_banner.png";
            //Debug.WriteLine("[AlterarBannerPatrocinioPopup] Caminho nulo ou vazio, usando imagem padrão.");
        }
    }

    private async void EscolherImagem_Clicked(object sender, EventArgs e) {
        //Debug.WriteLine("[AlterarBannerPatrocinioPopup] Botão 'Escolher imagem' clicado.");
        try {
            var result = await FilePicker.PickAsync(new PickOptions {
                PickerTitle = "Selecione um banner",
                FileTypes = FilePickerFileType.Images
            });
            if (result != null && !string.IsNullOrEmpty(result.FullPath)) {
                _caminhoNovoBannerSelecionado = result.FullPath;
                //Debug.WriteLine($"[AlterarBannerPatrocinioPopup] Imagem selecionada: '{_caminhoNovoBannerSelecionado}'");
                AtualizarImagemUI(_caminhoNovoBannerSelecionado);
            } else {
                //Debug.WriteLine("[AlterarBannerPatrocinioPopup] Seleção de imagem cancelada ou resultado nulo.");
            }
        } catch (Exception ex) {
            //Debug.WriteLine($"[AlterarBannerPatrocinioPopup] Erro ao escolher imagem: {ex.Message}");
            await _alertService.DisplayAlert("Erro", $"Não foi possível escolher a imagem: {ex.Message}", "OK");
        }
    }

    private async void Salvar_Clicked(object sender, EventArgs e) {
        //Debug.WriteLine("[AlterarBannerPatrocinioPopup] Botão 'Salvar' clicado.");
        if (IsBusy) return;

        if (string.IsNullOrEmpty(_caminhoNovoBannerSelecionado)) {
            //Debug.WriteLine("[AlterarBannerPatrocinioPopup] Caminho do novo banner é nulo. Ação abortada.");
            await _alertService.DisplayAlert("Aviso", "Por favor, escolha uma imagem primeiro.", "OK");
            return;
        }

        IsBusy = true;
        try {
            string diretorioImagens = FileSystem.AppDataDirectory;
            string nomeArquivo = Path.GetFileName(_caminhoNovoBannerSelecionado);
            string caminhoFinalImagem = Path.Combine(diretorioImagens, $"{_campanhaPatrocinio.ClientAppId}_{nomeArquivo}");

            //Debug.WriteLine($"[AlterarBannerPatrocinioPopup] Caminho de origem do arquivo: '{_caminhoNovoBannerSelecionado}'");
            //Debug.WriteLine($"[AlterarBannerPatrocinioPopup] Caminho de destino do arquivo: '{caminhoFinalImagem}'");

            if (File.Exists(caminhoFinalImagem)) {
                File.Delete(caminhoFinalImagem);
                //Debug.WriteLine("[AlterarBannerPatrocinioPopup] Arquivo de destino já existia e foi deletado.");
            }

            File.Copy(_caminhoNovoBannerSelecionado, caminhoFinalImagem, true);
            //Debug.WriteLine("[AlterarBannerPatrocinioPopup] Imagem copiada com sucesso.");

            _campanhaPatrocinio.ImagemPatrocinador = caminhoFinalImagem;
            _campanhaPatrocinio.IsSynced = false;
            _campanhaPatrocinio.UpdatedAt = DateTime.UtcNow;

            await _databaseService.AtualizarCampanhaPatrocinioAsync(_campanhaPatrocinio);

            BannerAtualizado?.Invoke(this, _campanhaPatrocinio.ImagemPatrocinador);
            //Debug.WriteLine($"[AlterarBannerPatrocinioPopup] Evento 'BannerAtualizado' disparado com o caminho: '{_campanhaPatrocinio.ImagemPatrocinador}'");

            await _alertService.DisplayAlert("Sucesso", "Banner de divulgação atualizado!", "OK");
            await Navigation.PopModalAsync();

        } catch (Exception ex) {
            //Debug.WriteLine($"[AlterarBannerPatrocinioPopup] Erro ao salvar a imagem: {ex.Message}");
            await _alertService.DisplayAlert("Erro", $"Erro ao salvar a imagem: {ex.Message}", "OK");
        } finally {
            IsBusy = false;
        }
    }

    private async void Cancelar_Clicked(object sender, EventArgs e) {
        await Navigation.PopModalAsync();
    }
}