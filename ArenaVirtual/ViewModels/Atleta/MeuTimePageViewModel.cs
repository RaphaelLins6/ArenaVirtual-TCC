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

        [ObservableProperty]
        private bool _mostraBotaoCancelar = false;

        [ObservableProperty]
        private bool _mostraBotaoVerSolicitacoes = false;

        [ObservableProperty]
        private bool _usuarioEhCapitao = false;

        public bool VinculadoATime => SessaoService.Instancia.GetUsuarioAtual()?.TimeClientAppId != null;

        public bool NaoVinculadoATime => !VinculadoATime;


        private readonly TimeService _timeService;
        private readonly UsuarioService _usuarioService;
        private readonly DatabaseService _databaseService;

        public MeuTimePageViewModel() { }

        public MeuTimePageViewModel(TimeService timeService, UsuarioService usuarioService, DatabaseService databaseService) {
            _timeService = timeService;
            _usuarioService = usuarioService;
            _databaseService = databaseService;
        }

        [RelayCommand]
        public async Task LoadDataAsync() {
            try {
                var usuarioAtual = SessaoService.Instancia.GetUsuarioAtual();
                // Remove as chamadas OnPropertyChanged daqui. Elas serão chamadas nos métodos de estado.

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

                // Agora o objeto Time está carregado, pode-se usar suas propriedades
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

                // A ordem das chamadas é crucial.
                // Define se o usuário é o capitão APENAS DEPOIS que a propriedade Time foi populada.
                UsuarioEhCapitao = usuarioAtual.ClientAppId == Time.CapitaoClientAppId;

                // A lógica para exibir o botão de solicitações agora depende do valor atualizado de UsuarioEhCapitao.
                if (UsuarioEhCapitao) {
                    var convitesPendentes = await _databaseService.ListarConvitesPendentesAsync(Time.ClientAppId);
                    MostraBotaoVerSolicitacoes = convitesPendentes.Count > 0;
                } else {
                    MostraBotaoVerSolicitacoes = false;
                }

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

        [RelayCommand]
        private async Task CancelarSolicitacao() {
            bool confirmacao = await Application.Current.MainPage.DisplayAlert(
            "Cancelar Solicitação",
            "Tem certeza de que deseja cancelar sua solicitação de entrada no time?",
            "Sim",
            "Não");

            if (!confirmacao) {
                return;
            }

            try {
                var usuarioAtual = SessaoService.Instancia.GetUsuarioAtual();
                if (usuarioAtual != null) {
                    await _databaseService.DeletarConvitePendenteDoUsuarioAsync(usuarioAtual.ClientAppId);
                    await Application.Current.MainPage.DisplayAlert("Sucesso", "Sua solicitação foi cancelada.", "OK");
                    await LoadDataAsync();
                }
            } catch (Exception ex) {
                Debug.WriteLine($"[ERRO DE CANCELAMENTO] Falha ao cancelar a solicitação: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Erro", "Não foi possível cancelar a solicitação. Tente novamente mais tarde.", "OK");
            }
        }

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
            MostraBotaoCancelar = false;
            // Garante que as propriedades de estado sejam atualizadas.
            UsuarioEhCapitao = false;
            OnPropertyChanged(nameof(VinculadoATime));
            OnPropertyChanged(nameof(NaoVinculadoATime));
        }

        private void SetMensagemPendencia(string? timeNome) {
            StatusMessageTitle = "Solicitação Pendente";
            StatusMessageDescription = !string.IsNullOrEmpty(timeNome)
            ? $"Sua solicitação para entrar no time {timeNome} foi enviada. Aguarde a resposta do capitão."
            : "Sua solicitação para entrar em um time foi enviada. Aguarde a resposta do capitão.";
            ShowButtons = false;
            MostraBotaoCancelar = true;
            // Garante que as propriedades de estado sejam atualizadas.
            UsuarioEhCapitao = false;
            OnPropertyChanged(nameof(VinculadoATime));
            OnPropertyChanged(nameof(NaoVinculadoATime));
        }

        private void SetVinculadoState() {
            StatusMessageTitle = string.Empty;
            StatusMessageDescription = string.Empty;
            ShowButtons = false;
            MostraBotaoCancelar = false;
            OnPropertyChanged(nameof(VinculadoATime));
            OnPropertyChanged(nameof(NaoVinculadoATime));
        }
    }
}