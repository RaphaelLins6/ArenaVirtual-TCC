using ArenaVirtual.Models;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ArenaVirtual.ViewModels.Atleta {

    public partial class CampeonatoItemViewModel : ObservableObject {
        public Campeonato Campeonato { get; set; }
        public string Nome => Campeonato?.Nome;
        public string Descricao => Campeonato?.Descricao;
        public string LogoUrl => Campeonato?.LogoUrl;

        [ObservableProperty]
        private string buttonText;

        [ObservableProperty]
        private bool isButtonEnabled;

        [ObservableProperty]
        private Color buttonColor;

        public CampeonatoItemViewModel(Campeonato campeonato, string buttonText, bool isEnabled, Color buttonColor) {
            Campeonato = campeonato;
            ButtonText = buttonText;
            IsButtonEnabled = isEnabled;
            ButtonColor = buttonColor;
        }
    }

    public partial class ProcurarCampeonatosViewModel : ObservableObject {

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SolicitarInscricaoCommand))]
        private bool isBusy;

        private bool IsNotBusy => !IsBusy;

        private readonly CampeonatoService _campeonatoService;
        private readonly DatabaseService _databaseService;
        private readonly SessaoService _sessaoService;
        private readonly TimeService _timeService;
        private readonly IAlertService _alertService;

        [ObservableProperty]
        private ObservableCollection<CampeonatoItemViewModel> campeonatosDisponiveis;

        public ProcurarCampeonatosViewModel(
            CampeonatoService campeonatoService,
            DatabaseService databaseService,
            SessaoService sessaoService,
            TimeService timeService,
            IAlertService alertService) {
            _campeonatoService = campeonatoService;
            _databaseService = databaseService;
            _sessaoService = sessaoService;
            _timeService = timeService;
            _alertService = alertService;
            CampeonatosDisponiveis = new ObservableCollection<CampeonatoItemViewModel>();
        }

        public async Task OnAppearingAsync() {
            Debug.WriteLine("[ProcurarCampeonatosViewModel] OnAppearingAsync chamado.");
            await PesquisarCampeonatosAsync(string.Empty);
        }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        public async Task PesquisarCampeonatosAsync(string query) {
            IsBusy = true;
            try {
                var todosCampeonatos = await _campeonatoService.ObterTodosAsync();
                var timeAtual = await _sessaoService.GetTimeAtualAsync();

                if (timeAtual == null) {
                    Debug.WriteLine("Usuário não pertence a nenhum time. Não é possível solicitar inscrição em campeonatos.");
                    MainThread.BeginInvokeOnMainThread(() => {
                        CampeonatosDisponiveis.Clear();
                    });
                    return;
                }

                var campeonatosFiltrados = string.IsNullOrWhiteSpace(query)
                ? todosCampeonatos
                : todosCampeonatos.Where(c => c.Nome.ToLower().Contains(query.ToLower())).ToList();

                MainThread.BeginInvokeOnMainThread(async () => {
                    CampeonatosDisponiveis.Clear();
                    foreach (var campeonato in campeonatosFiltrados) {

                        // ******************************************************
                        // 1. LÓGICA DE VERIFICAÇÃO DE VAGAS
                        // ******************************************************
                        // Usando o 'Id' do campeonato para obter os times aceitos
                        var timesAceitos = await _databaseService.ObterTimesAceitosAsync(campeonato.Id);

                        // Usamos NumeroMaximoEquipes do seu modelo (assumimos que Campeonato tem essa prop)
                        bool vagasEsgotadas = timesAceitos.Count >= campeonato.NumeroMaximoEquipes;

                        // 2. VERIFICAÇÃO DE STATUS DE SOLICITAÇÃO EXISTENTE
                        var solicitacaoExistente = await _databaseService.ObterSolicitacaoPorTimeECampeonatoAsync(timeAtual.ClientAppId.ToString(), campeonato.ClientAppId.ToString());

                        string buttonText = "Solicitar Inscrição";
                        bool isEnabled = true;
                        Color buttonColor = Color.FromArgb("#FF9800"); // Laranja (Padrão)

                        // 3. APLICAÇÃO DA LÓGICA (Prioridade: Inscrito > Vagas Esgotadas > Pendente)

                        // Prioridade 1: Já Inscrito/Aceito?
                        if (solicitacaoExistente?.Status == StatusConvite.Aceito) {
                            buttonText = "Inscrito";
                            isEnabled = false;
                            buttonColor = Color.FromArgb("#4CAF50"); // Verde
                        }
                        // Prioridade 2: Vagas Esgotadas? (Bloqueia novas solicitações)
                        else if (vagasEsgotadas) {
                            buttonText = "Vagas Esgotadas";
                            isEnabled = false;
                            buttonColor = Color.FromArgb("#FF0000"); // Vermelho
                        }
                        // Prioridade 3: Solicitação Pendente? (Bloqueia novas solicitações)
                        else if (solicitacaoExistente?.Status == StatusConvite.Pendente) {
                            buttonText = "Pendente";
                            isEnabled = false;
                            buttonColor = Color.FromArgb("#9E9E9E"); // Cinza
                        }
                        // Prioridade 4: Recusado ou Nulo (Default) -> Permanece Solicitar Inscrição

                        CampeonatosDisponiveis.Add(new CampeonatoItemViewModel(campeonato, buttonText, isEnabled, buttonColor));
                    }
                });
            } finally {
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        private async Task SolicitarInscricaoAsync(CampeonatoItemViewModel campeonatoItemVM) {
            if (campeonatoItemVM == null) return;
            IsBusy = true;
            try {
                var timeAtual = await _sessaoService.GetTimeAtualAsync();
                if (timeAtual == null) {
                    Debug.WriteLine("[SolicitarInscricao] Time atual não encontrado.");
                    await _alertService.DisplayAlert("Atenção", "Você precisa pertencer a um time para solicitar a inscrição em um campeonato.", "OK");
                    return;
                }

                // CRUCIAL: Re-checar as vagas antes de solicitar, caso a lista tenha sido atualizada recentemente.
                var timesAceitos = await _databaseService.ObterTimesAceitosAsync(campeonatoItemVM.Campeonato.Id);
                if (timesAceitos.Count >= campeonatoItemVM.Campeonato.NumeroMaximoEquipes) {
                    await _alertService.DisplayAlert("Atenção", "As vagas para este campeonato foram esgotadas.", "OK");
                    // Atualiza a UI para refletir o status de esgotado imediatamente
                    MainThread.BeginInvokeOnMainThread(() => {
                        campeonatoItemVM.ButtonText = "Vagas Esgotadas";
                        campeonatoItemVM.IsButtonEnabled = false;
                        campeonatoItemVM.ButtonColor = Color.FromArgb("#FF0000");
                    });
                    return;
                }

                // Criação da solicitação
                var solicitacao = new Convite {
                    ClientAppId = Guid.NewGuid(),
                    TimeClientAppId = timeAtual.ClientAppId,
                    CampeonatoClientAppId = campeonatoItemVM.Campeonato.ClientAppId,
                    Tipo = TipoConvite.InscricaoCampeonato,
                    Status = StatusConvite.Pendente,
                    DataCriacao = DateTime.UtcNow
                };

                // Inserir a solicitação no banco de dados local
                await _databaseService.InserirConviteAsync(solicitacao);

                Debug.WriteLine($"[SolicitarInscricao] Solicitação criada para o time {timeAtual.Nome} no campeonato {campeonatoItemVM.Nome}.");

                // ⚡️ Exibir a mensagem de sucesso usando o serviço de alerta
                await _alertService.DisplayAlert(
                    "Sucesso",
                    $"Sua solicitação de inscrição no campeonato '{campeonatoItemVM.Nome}' foi enviada com sucesso e agora está pendente.",
                    "OK");

                // Atualizar a UI
                MainThread.BeginInvokeOnMainThread(() => {
                    campeonatoItemVM.ButtonText = "Pendente";
                    campeonatoItemVM.IsButtonEnabled = false;
                    campeonatoItemVM.ButtonColor = Color.FromArgb("#9E9E9E");
                });

            } catch (Exception ex) {
                Debug.WriteLine($"Erro ao solicitar inscrição: {ex.Message}");
                // ⚡️ Exibir um alerta de erro caso algo dê errado
                await _alertService.DisplayAlert(
                    "Erro",
                    "Ocorreu um erro ao enviar sua solicitação. Por favor, tente novamente.",
                    "OK");
            } finally {
                IsBusy = false;
            }
        }
    }
}