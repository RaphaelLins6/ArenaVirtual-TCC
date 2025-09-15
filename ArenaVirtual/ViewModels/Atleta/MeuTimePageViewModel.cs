using ArenaVirtual.Services;
using ArenaVirtual.Views.Atleta;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using ArenaVirtual.Models;
using System.Diagnostics;
using System;
using System.Threading.Tasks;

namespace ArenaVirtual.ViewModels.Atleta {

    public partial class MeuTimePageViewModel : ObservableObject {

        public partial class MembroModel : ObservableObject {
            public string Nome { get; set; }
            public ImageSource Foto { get; set; }
        }

        [ObservableProperty]
        private Time? _time;

        [ObservableProperty]
        private ImageSource? _logoImageSource;

        [ObservableProperty]
        private ObservableCollection<MembroModel> _membrosDoTime = [];

        [ObservableProperty]
        private string _statusMessageTitle = string.Empty;

        [ObservableProperty]
        private string _statusMessageDescription = string.Empty;

        [ObservableProperty]
        private bool _showButtons = true;

        // Propriedades calculadas que dependem de outras
        public bool VinculadoATime => SessaoService.Instancia.GetUsuarioAtual()?.TimeClientAppId != null;
        public bool NaoVinculadoATime => !VinculadoATime;
        public bool UsuarioEhCapitao => SessaoService.Instancia.GetUsuarioAtual()?.ClientAppId == Time?.CapitaoClientAppId;

        private readonly TimeService _timeService;
        private readonly UsuarioService _usuarioService;
        private readonly DatabaseService _databaseService;

        public MeuTimePageViewModel() { }

        public MeuTimePageViewModel(TimeService timeService, UsuarioService usuarioService, DatabaseService databaseService) {
            _timeService = timeService;
            _usuarioService = usuarioService;
            _databaseService = databaseService;
        }

        // Método parcial que é executado automaticamente quando a propriedade _time é alterada.
        partial void OnTimeChanged(Time? value) {
            OnPropertyChanged(nameof(UsuarioEhCapitao));
        }

        [RelayCommand]
        public async Task LoadDataAsync() {
            try {
                var usuarioAtual = SessaoService.Instancia.GetUsuarioAtual();
                if (usuarioAtual == null) {
                    SetNaoVinculadoState("Erro de Sessão", "Não foi possível carregar as informações do usuário.");
                    return;
                }

                if (usuarioAtual.TimeClientAppId == null) {
                    var convitePendente = await _databaseService.ObterConvitePendenteDoUsuarioAsync(usuarioAtual.ClientAppId);
                    if (convitePendente != null) {
                        var timeConvidado = await _timeService.ObterPorClientAppIdAsync(convitePendente.TimeClientAppId);
                        SetMensagemPendencia(timeConvidado?.Nome);
                    } else {
                        SetNaoVinculadoState("Você ainda não está em um time!", "Crie seu próprio time ou solicite entrada em um time existente.");
                    }
                    return;
                }

                var timeDoUsuario = await _timeService.ObterPorClientAppIdAsync(usuarioAtual.TimeClientAppId.Value);

                if (timeDoUsuario == null) {
                    SetNaoVinculadoState("Ops! O time não foi encontrado.", "Pode ter sido excluído. Crie um novo ou entre em outro.");
                    return;
                }

                // A propriedade 'Time' é atualizada, disparando OnTimeChanged
                Time = timeDoUsuario;
                LogoImageSource = GetImageSourceFromFile(Time.LogoUrl);

                var usuariosDoTime = await _databaseService.GetMembrosByTimeClientAppIdAsync(Time.ClientAppId);

                var membrosCarregados = new ObservableCollection<MembroModel>();
                if (usuariosDoTime != null) {
                    foreach (var usuario in usuariosDoTime) {
                        membrosCarregados.Add(new MembroModel {
                            Nome = usuario.Nome,
                            Foto = GetImageSourceFromFile(usuario.ImagemPath)
                        });
                    }
                }
                MembrosDoTime = membrosCarregados;

                SetVinculadoState();
            } catch (Exception ex) {
                SetNaoVinculadoState("Erro", "Não foi possível carregar os dados do time.");
                Debug.WriteLine($"[ERRO GERAL] Falha ao carregar dados do time: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task CriarMeuTime() =>
            await Shell.Current.GoToAsync(nameof(CriarTimePage));

        [RelayCommand]
        private async Task EntrarTime() =>
            await Shell.Current.GoToAsync(nameof(EntrarTimePage));

        [RelayCommand]
        private async Task GerenciarTime() =>
            await Shell.Current.GoToAsync(nameof(EditarTimePage));

        [RelayCommand]
        private async Task VerSolicitacoes() =>
            await Shell.Current.GoToAsync(nameof(SolicitacaoTimePage));

        private ImageSource? GetImageSourceFromFile(string? filePath) {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) {
                return ImageSource.FromFile("placeholder.png");
            }
            try {
                return ImageSource.FromStream(() => File.OpenRead(filePath));
            } catch (Exception ex) {
                Debug.WriteLine($"[ERRO DE CARREGAMENTO] Falha ao carregar imagem: {ex.Message}");
                return ImageSource.FromFile("placeholder.png");
            }
        }

        private void SetNaoVinculadoState(string title, string description) {
            Time = null;
            MembrosDoTime.Clear();
            StatusMessageTitle = title;
            StatusMessageDescription = description;
            ShowButtons = true;
            OnPropertyChanged(nameof(VinculadoATime));
            OnPropertyChanged(nameof(NaoVinculadoATime));
        }

        private void SetMensagemPendencia(string? timeNome) {
            StatusMessageTitle = "Solicitação Pendente";
            StatusMessageDescription = !string.IsNullOrEmpty(timeNome)
                ? $"Sua solicitação para entrar no time {timeNome} foi enviada. Aguarde a resposta do capitão."
                : "Sua solicitação para entrar em um time foi enviada. Aguarde a resposta do capitão.";
            ShowButtons = false;
            OnPropertyChanged(nameof(VinculadoATime));
            OnPropertyChanged(nameof(NaoVinculadoATime));
        }

        private void SetVinculadoState() {
            StatusMessageTitle = string.Empty;
            StatusMessageDescription = string.Empty;
            ShowButtons = false;
            OnPropertyChanged(nameof(VinculadoATime));
            OnPropertyChanged(nameof(NaoVinculadoATime));
        }
    }
}