// Em ArenaVirtual/ViewModels/Atleta/ProcurarCampeonatosViewModel.cs
using ArenaVirtual.Models;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

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

        [ObservableProperty]
        private ObservableCollection<CampeonatoItemViewModel> campeonatosDisponiveis;

        public ProcurarCampeonatosViewModel(CampeonatoService campeonatoService, DatabaseService databaseService, SessaoService sessaoService, TimeService timeService) {
            _campeonatoService = campeonatoService;
            _databaseService = databaseService;
            _sessaoService = sessaoService;
            _timeService = timeService;
            CampeonatosDisponiveis = new ObservableCollection<CampeonatoItemViewModel>();

            // ALTERAÇÃO: Removido Task.Run do construtor.
            // O ideal é chamar um método de inicialização da View,
            // por exemplo, no evento OnAppearing.
            // await PesquisarCampeonatosAsync(string.Empty);
        }

        // ⚡️ ADICIONE ESTE MÉTODO
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
                        var solicitacaoExistente = await _databaseService.ObterSolicitacaoPorTimeECampeonatoAsync(timeAtual.ClientAppId.ToString(), campeonato.ClientAppId.ToString());

                        string buttonText = "Solicitar Inscrição";
                        bool isEnabled = true;
                        Color buttonColor = Color.FromArgb("#FF9800");

                        // CORREÇÃO: Comparando diretamente com o enum para evitar erros
                        if (solicitacaoExistente?.Status == StatusConvite.Pendente) {
                            buttonText = "Pendente";
                            isEnabled = false;
                            buttonColor = Color.FromArgb("#9E9E9E");
                        }

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
                // TODO: Adicione a lógica para enviar a solicitação de inscrição
                // Chamando o TimeService ou outro serviço apropriado
                // await _timeService.SolicitarEntradaNoTimeAsync(...);
            } finally {
                IsBusy = false;
            }
        }
    }
}