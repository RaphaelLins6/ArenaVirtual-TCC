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
using ArenaVirtual.Views.CampeonatoPage;

namespace ArenaVirtual.ViewModels.Atleta {

    public partial class MeuTimePageViewModel : ObservableObject {

        public partial class MembroModel : ObservableObject {
            public string Nome { get; set; }
            public ImageSource Foto { get; set; }
            public Guid ClientAppId { get; set; }
            public bool EhCapitaoLogado { get; set; }
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

                // Obtém o ClientAppId do usuário logado (Capitão ou Membro)
                var capitãoId = usuarioAtual.ClientAppId;

                var membrosCarregados = new ObservableCollection<MembroModel>();
                if (usuariosDoTime != null) {
                    foreach (var usuario in usuariosDoTime) {
                        membrosCarregados.Add(new MembroModel {
                            Nome = usuario.Nome,
                            Foto = GetImageSourceFromFile(usuario.ImagemPath),
                            ClientAppId = usuario.ClientAppId, // Preenche o ID para remoção segura
                                                               // Define se este card é do usuário logado (para uso no XAML se necessário)
                            EhCapitaoLogado = usuario.ClientAppId == capitãoId
                        });
                    }
                }
                MembrosDoTime = membrosCarregados;

                // Propriedade principal para comandos e botões na página
                UsuarioEhCapitao = usuarioAtual.ClientAppId == Time.CapitaoClientAppId;

                MostraBotaoVerSolicitacoes = UsuarioEhCapitao;

                SetVinculadoState();

            } catch (Exception ex) {
                SetNaoVinculadoState("Erro", "Não foi possível carregar os dados do time.");
                //Debug.WriteLine($"[ERRO GERAL] Falha ao carregar dados do time: {ex.Message}");
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
                //Debug.WriteLine($"[ERRO DE CANCELAMENTO] Falha ao cancelar a solicitação: {ex.Message}");
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
                //Debug.WriteLine($"[ERRO DE CARREGAMENTO] Falha ao carregar imagem: {ex.Message}");
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
            UsuarioEhCapitao = false;
            MostraBotaoVerSolicitacoes = false; 
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
            UsuarioEhCapitao = false;
            MostraBotaoVerSolicitacoes = false; 
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

        [RelayCommand]
        private async Task ProcurarCampeonatos() {
            await Shell.Current.GoToAsync(nameof(ProcurarCampeonatosPage));
        }

        [RelayCommand]
        private async Task RemoverMembro(MembroModel membro) {
            if (Time == null || !UsuarioEhCapitao) {
                return;
            }

            var usuarioAtual = SessaoService.Instancia.GetUsuarioAtual();
            if (usuarioAtual?.ClientAppId == membro.ClientAppId) {
                await Application.Current.MainPage.DisplayAlert("Atenção", "Você não pode se remover do time nesta tela, pois você é o Capitão. Para sair, você deve primeiro transferir a capitania ou excluir o time.", "OK");
                return;
            }

            bool confirmacao = await Application.Current.MainPage.DisplayAlert(
                "Remover Membro",
                $"Tem certeza de que deseja remover o membro {membro.Nome} do time?",
                "Sim",
                "Não");

            if (!confirmacao) {
                return;
            }

            try {

                await _usuarioService.RemoverUsuarioDoTimeAsync(membro.ClientAppId);

                MembrosDoTime.Remove(membro);
                await Application.Current.MainPage.DisplayAlert("Sucesso", $"{membro.Nome} foi removido do time.", "OK");

            } catch (Exception ex) {
                //Debug.WriteLine($"[ERRO DE REMOÇÃO] Falha ao remover membro: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Erro", "Não foi possível remover o membro. Tente novamente mais tarde.", "OK");
            }
        }
    }
}