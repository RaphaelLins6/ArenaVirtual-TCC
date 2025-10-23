using ArenaVirtual.Models;
using ArenaVirtual.Services;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace ArenaVirtual.Popups;

public partial class AlterarBannerPopup : ContentPage, INotifyPropertyChanged {
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

    private readonly Campeonato _campeonato;
    private readonly IAlertService _alertService;
    private readonly DatabaseService _databaseService; 
    private readonly SyncService _syncService; 

    public event EventHandler<string>? BannerAtualizado;

    private string? _caminhoNovoBannerSelecionado;

    public AlterarBannerPopup(Campeonato campeonato, IAlertService alertService, DatabaseService databaseService, SyncService syncService) {
        InitializeComponent();
        _campeonato = campeonato;
        _alertService = alertService;
        _databaseService = databaseService;
        _syncService = syncService;

        BindingContext = this;
        AtualizarImagemUI(_campeonato.BannerUrl);
        //Debug.WriteLine($"[AlterarBannerPopup] Popup inicializado. BannerUrl recebido: '{_campeonato.BannerUrl}'");
    }

    private void AtualizarImagemUI(string? caminhoImagem) {
        if (!string.IsNullOrEmpty(caminhoImagem)) {
            //Debug.WriteLine($"[AlterarBannerPopup] Tentando carregar imagem do caminho: '{caminhoImagem}'");
            if (Uri.IsWellFormedUriString(caminhoImagem, UriKind.Absolute)) {
                ImagemBanner.Source = ImageSource.FromUri(new Uri(caminhoImagem));
                //Debug.WriteLine("[AlterarBannerPopup] Imagem carregada de URI.");
            }
            else if (File.Exists(caminhoImagem)) {
                ImagemBanner.Source = ImageSource.FromFile(caminhoImagem);
                //Debug.WriteLine("[AlterarBannerPopup] Imagem carregada de arquivo local.");
            } else {
                ImagemBanner.Source = "default_banner.png";
                //Debug.WriteLine("[AlterarBannerPopup] Caminho inválido, usando imagem padrão.");
            }
        } else {
            ImagemBanner.Source = "default_banner.png";
            //Debug.WriteLine("[AlterarBannerPopup] Caminho nulo ou vazio, usando imagem padrão.");
        }
    }

    private async void EscolherImagem_Clicked(object sender, EventArgs e) {
        //Debug.WriteLine("[AlterarBannerPopup] Botão 'Escolher imagem' clicado.");
        try {
            var result = await FilePicker.PickAsync(new PickOptions {
                PickerTitle = "Selecione um banner",
                FileTypes = FilePickerFileType.Images
            });
            if (result != null && !string.IsNullOrEmpty(result.FullPath)) {
                _caminhoNovoBannerSelecionado = result.FullPath;
                //Debug.WriteLine($"[AlterarBannerPopup] Imagem selecionada: '{_caminhoNovoBannerSelecionado}'");
                AtualizarImagemUI(_caminhoNovoBannerSelecionado);
            } else {
                //Debug.WriteLine("[AlterarBannerPopup] Seleção de imagem cancelada ou resultado nulo.");
            }
        } catch (Exception ex) {
            //Debug.WriteLine($"[AlterarBannerPopup] Erro ao escolher imagem: {ex.Message}");
            await _alertService.DisplayAlert("Erro", $"Não foi possível escolher a imagem: {ex.Message}", "OK");
        }
    }

    private async void Salvar_Clicked(object sender, EventArgs e) {
        //Debug.WriteLine("[AlterarBannerPopup] Botão 'Salvar' clicado.");
        if (IsBusy) return;

        if (string.IsNullOrEmpty(_caminhoNovoBannerSelecionado)) {
            //Debug.WriteLine("[AlterarBannerPopup] Caminho do novo banner é nulo. Ação abortada.");
            await _alertService.DisplayAlert("Aviso", "Por favor, escolha uma imagem primeiro.", "OK");
            return;
        }

        IsBusy = true; 
        try {
            string diretorioImagens = FileSystem.AppDataDirectory;
            string nomeArquivo = Path.GetFileName(_caminhoNovoBannerSelecionado);
            string caminhoFinalImagem = Path.Combine(diretorioImagens, nomeArquivo);

            //Debug.WriteLine($"[AlterarBannerPopup] Caminho de origem do arquivo: '{_caminhoNovoBannerSelecionado}'");
            //Debug.WriteLine($"[AlterarBannerPopup] Caminho de destino do arquivo: '{caminhoFinalImagem}'");

            if (File.Exists(caminhoFinalImagem)) {
                File.Delete(caminhoFinalImagem);
                //Debug.WriteLine("[AlterarBannerPopup] Arquivo de destino já existia e foi deletado.");
            }

            File.Copy(_caminhoNovoBannerSelecionado, caminhoFinalImagem, true);
            Debug.WriteLine("[AlterarBannerPopup] Imagem copiada com sucesso.");

            _campeonato.BannerUrl = caminhoFinalImagem;

            // Lógica de sincronização:
            // _campeonato.IsSynced = false;
            // _campeonato.UpdatedAt = DateTime.UtcNow;

            await _databaseService.AtualizarCampeonatoAsync(_campeonato);

            BannerAtualizado?.Invoke(this, _campeonato.BannerUrl);
            //Debug.WriteLine($"[AlterarBannerPopup] Evento 'BannerAtualizado' disparado com o caminho: '{_campeonato.BannerUrl}'");

            await _alertService.DisplayAlert("Sucesso", "Banner atualizado!", "OK");
            await Navigation.PopModalAsync();

        } catch (Exception ex) {
            //Debug.WriteLine($"[AlterarBannerPopup] Erro ao salvar a imagem: {ex.Message}");
            await _alertService.DisplayAlert("Erro", $"Erro ao salvar a imagem: {ex.Message}", "OK");
        } finally {
            IsBusy = false; 
        }
    }

    private async void Cancelar_Clicked(object sender, EventArgs e) {
        await Navigation.PopModalAsync();
    }
}