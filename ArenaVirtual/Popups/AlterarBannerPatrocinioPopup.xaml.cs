using ArenaVirtual.Models;
using ArenaVirtual.Services;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace ArenaVirtual.Popups;

// A classe foi renomeada
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

    // O objeto de foco agora é a PropostaPatrocinio
    private readonly PropostaPatrocinio _propostaPatrocinio;
    private readonly IAlertService _alertService;
    private readonly DatabaseService _databaseService;
    private readonly SyncService _syncService;

    public event EventHandler<string>? BannerAtualizado;

    private string? _caminhoNovoBannerSelecionado;

    // O construtor agora recebe PropostaPatrocinio
    public AlterarBannerPatrocinioPopup(PropostaPatrocinio propostaPatrocinio, IAlertService alertService, DatabaseService databaseService, SyncService syncService) {
        InitializeComponent();
        _propostaPatrocinio = propostaPatrocinio;
        _alertService = alertService;
        _databaseService = databaseService;
        _syncService = syncService;

        BindingContext = this;
        // Usa ImagemPatrocinador em vez de BannerUrl
        AtualizarImagemUI(_propostaPatrocinio.ImagemPatrocinador);
        Debug.WriteLine($"[AlterarBannerPatrocinioPopup] Popup inicializado. ImagemPatrocinador recebido: '{_propostaPatrocinio.ImagemPatrocinador}'");
    }

    private void AtualizarImagemUI(string? caminhoImagem) {
        if (!string.IsNullOrEmpty(caminhoImagem)) {
            Debug.WriteLine($"[AlterarBannerPatrocinioPopup] Tentando carregar imagem do caminho: '{caminhoImagem}'");
            if (Uri.IsWellFormedUriString(caminhoImagem, UriKind.Absolute)) {
                ImagemBanner.Source = ImageSource.FromUri(new Uri(caminhoImagem));
                Debug.WriteLine("[AlterarBannerPatrocinioPopup] Imagem carregada de URI.");
            } else if (File.Exists(caminhoImagem)) {
                ImagemBanner.Source = ImageSource.FromFile(caminhoImagem);
                Debug.WriteLine("[AlterarBannerPatrocinioPopup] Imagem carregada de arquivo local.");
            } else {
                ImagemBanner.Source = "default_banner.png";
                Debug.WriteLine("[AlterarBannerPatrocinioPopup] Caminho inválido, usando imagem padrão.");
            }
        } else {
            ImagemBanner.Source = "default_banner.png";
            Debug.WriteLine("[AlterarBannerPatrocinioPopup] Caminho nulo ou vazio, usando imagem padrão.");
        }
    }

    private async void EscolherImagem_Clicked(object sender, EventArgs e) {
        Debug.WriteLine("[AlterarBannerPatrocinioPopup] Botão 'Escolher imagem' clicado.");
        try {
            var result = await FilePicker.PickAsync(new PickOptions {
                PickerTitle = "Selecione um banner",
                FileTypes = FilePickerFileType.Images
            });
            if (result != null && !string.IsNullOrEmpty(result.FullPath)) {
                _caminhoNovoBannerSelecionado = result.FullPath;
                Debug.WriteLine($"[AlterarBannerPatrocinioPopup] Imagem selecionada: '{_caminhoNovoBannerSelecionado}'");
                AtualizarImagemUI(_caminhoNovoBannerSelecionado);
            } else {
                Debug.WriteLine("[AlterarBannerPatrocinioPopup] Seleção de imagem cancelada ou resultado nulo.");
            }
        } catch (Exception ex) {
            Debug.WriteLine($"[AlterarBannerPatrocinioPopup] Erro ao escolher imagem: {ex.Message}");
            await _alertService.DisplayAlert("Erro", $"Não foi possível escolher a imagem: {ex.Message}", "OK");
        }
    }

    private async void Salvar_Clicked(object sender, EventArgs e) {
        Debug.WriteLine("[AlterarBannerPatrocinioPopup] Botão 'Salvar' clicado.");
        if (IsBusy) return;

        if (string.IsNullOrEmpty(_caminhoNovoBannerSelecionado)) {
            Debug.WriteLine("[AlterarBannerPatrocinioPopup] Caminho do novo banner é nulo. Ação abortada.");
            await _alertService.DisplayAlert("Aviso", "Por favor, escolha uma imagem primeiro.", "OK");
            return;
        }

        IsBusy = true;
        try {
            // Lógica de copiar o arquivo local
            string diretorioImagens = FileSystem.AppDataDirectory;
            string nomeArquivo = Path.GetFileName(_caminhoNovoBannerSelecionado);
            // Usamos o ClientAppId da Proposta para criar um nome de arquivo exclusivo
            string caminhoFinalImagem = Path.Combine(diretorioImagens, $"{_propostaPatrocinio.ClientAppId}_{nomeArquivo}");

            Debug.WriteLine($"[AlterarBannerPatrocinioPopup] Caminho de origem do arquivo: '{_caminhoNovoBannerSelecionado}'");
            Debug.WriteLine($"[AlterarBannerPatrocinioPopup] Caminho de destino do arquivo: '{caminhoFinalImagem}'");

            if (File.Exists(caminhoFinalImagem)) {
                File.Delete(caminhoFinalImagem);
                Debug.WriteLine("[AlterarBannerPatrocinioPopup] Arquivo de destino já existia e foi deletado.");
            }

            File.Copy(_caminhoNovoBannerSelecionado, caminhoFinalImagem, true);
            Debug.WriteLine("[AlterarBannerPatrocinioPopup] Imagem copiada com sucesso.");

            // *** MUDANÇA CRÍTICA: Atualiza o modelo PropostaPatrocinio ***
            _propostaPatrocinio.ImagemPatrocinador = caminhoFinalImagem;
            _propostaPatrocinio.IsSynced = false;
            _propostaPatrocinio.UpdatedAt = DateTime.UtcNow;

            // Chamar o serviço para atualizar a PropostaPatrocinio no banco de dados
            await _databaseService.AtualizarPropostaPatrocinioAsync(_propostaPatrocinio);

            // Disparar o evento para notificar a ViewModel principal
            BannerAtualizado?.Invoke(this, _propostaPatrocinio.ImagemPatrocinador);
            Debug.WriteLine($"[AlterarBannerPatrocinioPopup] Evento 'BannerAtualizado' disparado com o caminho: '{_propostaPatrocinio.ImagemPatrocinador}'");

            await _alertService.DisplayAlert("Sucesso", "Banner de divulgação atualizado!", "OK");
            await Navigation.PopModalAsync();

        } catch (Exception ex) {
            Debug.WriteLine($"[AlterarBannerPatrocinioPopup] Erro ao salvar a imagem: {ex.Message}");
            await _alertService.DisplayAlert("Erro", $"Erro ao salvar a imagem: {ex.Message}", "OK");
        } finally {
            IsBusy = false;
        }
    }

    private async void Cancelar_Clicked(object sender, EventArgs e) {
        await Navigation.PopModalAsync();
    }
}