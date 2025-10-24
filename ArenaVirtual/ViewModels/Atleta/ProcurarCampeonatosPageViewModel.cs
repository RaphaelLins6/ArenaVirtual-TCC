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
            //Debug.WriteLine("[ProcurarCampeonatosViewModel] OnAppearingAsync chamado.");
            await PesquisarCampeonatosAsync(string.Empty);
        }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        public async Task PesquisarCampeonatosAsync(string query) {
            if (IsBusy) return;
            IsBusy = true; 
            try {
                var todosCampeonatos = await _campeonatoService.ObterTodosAsync();
                var timeAtual = await _sessaoService.GetTimeAtualAsync();

                if (timeAtual == null) {
                    // ... (Lógica de time não encontrado)
                    MainThread.BeginInvokeOnMainThread(() => {
                        CampeonatosDisponiveis.Clear();
                    });
                    return;
                }

                var campeonatosFiltrados = string.IsNullOrWhiteSpace(query)
                    ? todosCampeonatos
                    : todosCampeonatos.Where(c => c.Nome.ToLower().Contains(query.ToLower())).ToList();

                var novosItens = new List<CampeonatoItemViewModel>(); 

                foreach (var campeonato in campeonatosFiltrados) {
                    var timesAceitos = await _databaseService.ObterTimesAceitosAsync(campeonato.Id);
                    var solicitacaoExistente = await _databaseService.ObterSolicitacaoPorTimeECampeonatoAsync(timeAtual.ClientAppId.ToString(), campeonato.ClientAppId.ToString());

                    string buttonText = "Solicitar Inscrição";
                    bool isEnabled = true;
                    Color buttonColor = Color.FromArgb("#FF9800");

                    if (solicitacaoExistente?.Status == StatusConvite.Aceito) {
                        buttonText = "Inscrito";
                        isEnabled = false;
                        buttonColor = Color.FromArgb("#4CAF50");
                    }
                    else if (timesAceitos.Count >= campeonato.NumeroMaximoEquipes) {
                        buttonText = "Vagas Esgotadas";
                        isEnabled = false;
                        buttonColor = Color.FromArgb("#FF0000");
                    }
                    else if (solicitacaoExistente?.Status == StatusConvite.Pendente) {
                        buttonText = "Pendente";
                        isEnabled = false;
                        buttonColor = Color.FromArgb("#9E9E9E");
                    }
                    novosItens.Add(new CampeonatoItemViewModel(campeonato, buttonText, isEnabled, buttonColor));
                }

                MainThread.BeginInvokeOnMainThread(() => {
                    CampeonatosDisponiveis.Clear();
                    foreach (var item in novosItens) {
                        CampeonatosDisponiveis.Add(item);
                    }
                });

            } catch (Exception ex) {
                //Debug.WriteLine($"[PesquisarCampeonatosAsync] ERRO CRÍTICO: {ex.Message}");
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
                    //Debug.WriteLine("[SolicitarInscricao] Time atual não encontrado.");
                    await _alertService.DisplayAlert("Atenção", "Você precisa pertencer a um time para solicitar a inscrição em um campeonato.", "OK");
                    return;
                }

                var timesAceitos = await _databaseService.ObterTimesAceitosAsync(campeonatoItemVM.Campeonato.Id);
                if (timesAceitos.Count >= campeonatoItemVM.Campeonato.NumeroMaximoEquipes) {
                    await _alertService.DisplayAlert("Atenção", "As vagas para este campeonato foram esgotadas.", "OK");
                    MainThread.BeginInvokeOnMainThread(() => {
                        campeonatoItemVM.ButtonText = "Vagas Esgotadas";
                        campeonatoItemVM.IsButtonEnabled = false;
                        campeonatoItemVM.ButtonColor = Color.FromArgb("#FF0000");
                    });
                    return;
                }

                var solicitacao = new Convite {
                    ClientAppId = Guid.NewGuid(),
                    TimeClientAppId = timeAtual.ClientAppId,
                    CampeonatoClientAppId = campeonatoItemVM.Campeonato.ClientAppId,
                    Tipo = TipoConvite.InscricaoCampeonato,
                    Status = StatusConvite.Pendente,
                    DataCriacao = DateTime.UtcNow
                };

                await _databaseService.InserirConviteAsync(solicitacao);

                //Debug.WriteLine($"[SolicitarInscricao] Solicitação criada para o time {timeAtual.Nome} no campeonato {campeonatoItemVM.Nome}.");

                await _alertService.DisplayAlert(
                    "Sucesso",
                    $"Sua solicitação de inscrição no campeonato '{campeonatoItemVM.Nome}' foi enviada com sucesso e agora está pendente.",
                    "OK");

                MainThread.BeginInvokeOnMainThread(() => {
                    campeonatoItemVM.ButtonText = "Pendente";
                    campeonatoItemVM.IsButtonEnabled = false;
                    campeonatoItemVM.ButtonColor = Color.FromArgb("#9E9E9E");
                });

            } catch (Exception ex) {
                //Debug.WriteLine($"Erro ao solicitar inscrição: {ex.Message}");
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