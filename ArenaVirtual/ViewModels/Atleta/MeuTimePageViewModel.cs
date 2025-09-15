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

        // Propriedade para controlar a visibilidade do botão "Ver Solicitações"
        [ObservableProperty]
        private bool _mostraBotaoVerSolicitacoes = false;

        // Propriedade para controlar se o usuário é o capitão do time
        [ObservableProperty]
        private bool _usuarioEhCapitao = false;

        // As propriedades abaixo não precisam ser Observable, pois são recomputadas no LoadDataAsync.
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
                OnPropertyChanged(nameof(VinculadoATime));
                OnPropertyChanged(nameof(NaoVinculadoATime));

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

                // Atribuindo o valor ao campo de suporte da propriedade ObservableProperty
                _usuarioEhCapitao = usuarioAtual.ClientAppId == Time.CapitaoClientAppId;

                // Lógica para exibir o botão "Ver Solicitações"
                if (UsuarioEhCapitao) {
                    var convitesPendentes = await _databaseService.ListarConvitesPendentesAsync(Time.ClientAppId);
                    MostraBotaoVerSolicitacoes = convitesPendentes.Count > 0;
                } else {
                    MostraBotaoVerSolicitacoes = false;
                }

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
                    SetNaoVinculadoState("Solicitação Cancelada", "Você ainda não está em um time. Crie seu próprio time ou solicite entrada em um time existente.");
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
            // Corrigido
            _usuarioEhCapitao = false;
            OnPropertyChanged(nameof(VinculadoATime));
            OnPropertyChanged(nameof(NaoVinculadoATime));
            OnPropertyChanged(nameof(UsuarioEhCapitao));
        }

        private void SetMensagemPendencia(string? timeNome) {
            StatusMessageTitle = "Solicitação Pendente";
            StatusMessageDescription = !string.IsNullOrEmpty(timeNome)
             ? $"Sua solicitação para entrar no time {timeNome} foi enviada. Aguarde a resposta do capitão."
             : "Sua solicitação para entrar em um time foi enviada. Aguarde a resposta do capitão.";
            ShowButtons = false;
            MostraBotaoCancelar = true;
            // Corrigido
            _usuarioEhCapitao = false;
            OnPropertyChanged(nameof(VinculadoATime));
            OnPropertyChanged(nameof(NaoVinculadoATime));
            OnPropertyChanged(nameof(UsuarioEhCapitao));
        }

        private void SetVinculadoState() {
            StatusMessageTitle = string.Empty;
            StatusMessageDescription = string.Empty;
            ShowButtons = false;
            MostraBotaoCancelar = false;
            OnPropertyChanged(nameof(VinculadoATime));
            OnPropertyChanged(nameof(NaoVinculadoATime));
            OnPropertyChanged(nameof(UsuarioEhCapitao));
        }
    }
}