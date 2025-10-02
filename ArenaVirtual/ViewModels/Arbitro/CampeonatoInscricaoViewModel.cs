using ArenaVirtual.Models;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ArenaVirtual.ViewModels.Arbitro {

    // ViewModel para cada item da lista (Campeonato)
    public partial class CampeonatoItemViewModel : ObservableObject {
        public Campeonato Campeonato { get; set; }
        public string Nome => Campeonato?.Nome;
        public string Descricao => Campeonato?.Descricao;
        public string LogoUrl => Campeonato?.LogoUrl;

        [ObservableProperty] private string buttonText;
        [ObservableProperty] private bool isButtonEnabled;
        [ObservableProperty] private Color buttonColor;

        public CampeonatoItemViewModel(Campeonato campeonato, string buttonText, bool isEnabled, Color buttonColor) {
            Campeonato = campeonato;
            ButtonText = buttonText;
            IsButtonEnabled = isEnabled;
            ButtonColor = buttonColor;

            // NOVO LOG: Confirma a criação de cada item
            Debug.WriteLine($"[CampeonatoItemVM] Item criado: {Nome}, Habilitado: {IsButtonEnabled}");
        }
    }

    // ViewModel da página principal
    public partial class CampeonatoInscricaoViewModel : ObservableObject {

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CarregarCampeonatosCommand))]
        [NotifyCanExecuteChangedFor(nameof(SolicitarArbitragemCommand))]
        private bool isBusy;

        private bool IsNotBusy => !IsBusy;

        private readonly CampeonatoService _campeonatoService;
        private readonly DatabaseService _databaseService;
        private readonly SessaoService _sessaoService;
        private readonly IAlertService _alertService;

        [ObservableProperty]
        private ObservableCollection<CampeonatoItemViewModel> campeonatosDisponiveis;

        public CampeonatoInscricaoViewModel(
          CampeonatoService campeonatoService,
          DatabaseService databaseService,
          SessaoService sessaoService,
          IAlertService alertService) {

            _campeonatoService = campeonatoService;
            _databaseService = databaseService;
            _sessaoService = sessaoService;
            _alertService = alertService;

            // LOG: Confirma a inicialização da ViewModel e da lista
            Debug.WriteLine("[CampeonatoInscricaoViewModel] ViewModel inicializada.");

            CampeonatosDisponiveis = new ObservableCollection<CampeonatoItemViewModel>();
        }

        public async Task OnAppearingAsync() {
            Debug.WriteLine("[CampeonatoInscricaoViewModel] OnAppearingAsync chamado. Iniciando CarregarCampeonatosAsync.");
            await CarregarCampeonatosAsync(string.Empty);
        }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        public async Task CarregarCampeonatosAsync(string query) {
            Debug.WriteLine($"[CarregarCampeonatosAsync] INÍCIO. IsBusy={IsBusy}. Query: '{query}'");

            // Garante que IsBusy seja sempre true no início, mesmo que fosse false
            if (IsBusy) {
                Debug.WriteLine("[CarregarCampeonatosAsync] Já está ocupado. Saindo.");
                return;
            }

            IsBusy = true;

            try {
                var todosCampeonatos = await _campeonatoService.ObterTodosAsync();
                Debug.WriteLine($"[CarregarCampeonatosAsync] Campeonatos obtidos do serviço: {todosCampeonatos?.Count() ?? 0}");

                var arbitroAtual = await _sessaoService.GetArbitroAtualAsync();
                Debug.WriteLine($"[CarregarCampeonatosAsync] Árbitro logado? {(arbitroAtual != null ? $"SIM, ID: {arbitroAtual.ClientAppId}" : "NÃO")}");

                if (arbitroAtual == null) {
                    Debug.WriteLine("[CarregarCampeonatosAsync] Falha na sessão. Limpando lista.");
                    CampeonatosDisponiveis.Clear();
                    await _alertService.DisplayAlert("Atenção", "Você precisa estar logado como Árbitro para solicitar arbitragem.", "OK");
                    return;
                }

                string arbitroClientAppId = arbitroAtual.ClientAppId.ToString();

                var campeonatosFiltrados = string.IsNullOrWhiteSpace(query)
                  ? todosCampeonatos
                  : todosCampeonatos.Where(c => c.Nome.ToLower().Contains(query.ToLower())).ToList();

                Debug.WriteLine($"[CarregarCampeonatosAsync] Após filtro ('{query}'), Total: {campeonatosFiltrados.Count}");

                var novosItens = new List<CampeonatoItemViewModel>();

                foreach (var campeonato in campeonatosFiltrados) {
                    // Verificação de solicitação existente e determinação do botão
                    var solicitacaoExistente = await _databaseService.ObterSolicitacaoPorArbitroECampeonatoAsync(
            arbitroClientAppId,
            campeonato.ClientAppId.ToString(),
            TipoConvite.InscricaoArbitro);

                    Debug.WriteLine($"[CarregarCampeonatosAsync] Processando '{campeonato.Nome}'. Status: {(solicitacaoExistente != null ? solicitacaoExistente.Status.ToString() : "NENHUMA")}");

                    string buttonText = "Solicitar Arbitragem";
                    bool isEnabled = true;
                    Color buttonColor = Color.FromArgb("#1976D2");

                    if (solicitacaoExistente?.Status == StatusConvite.Aceito) {
                        buttonText = "Designado";
                        isEnabled = false;
                        buttonColor = Color.FromArgb("#4CAF50");
                    } else if (solicitacaoExistente?.Status == StatusConvite.Pendente) {
                        buttonText = "Pendente";
                        isEnabled = false;
                        buttonColor = Color.FromArgb("#9E9E9E");
                    }

                    novosItens.Add(new CampeonatoItemViewModel(campeonato, buttonText, isEnabled, buttonColor));
                }

                // NOVO LOG: Quantidade de itens prontos para ir para a UI
                Debug.WriteLine($"[CarregarCampeonatosAsync] Total de ViewModels criadas: {novosItens.Count}. Movendo para a MainThread.");

                MainThread.BeginInvokeOnMainThread(() => {
                    // NOVO LOG: Limpando lista na MainThread
                    Debug.WriteLine($"[MainThread] Limpando CampeonatosDisponiveis. Contagem inicial: {CampeonatosDisponiveis.Count}");

                    CampeonatosDisponiveis.Clear();

                    int itensAdicionados = 0;
                    foreach (var item in novosItens) {
                        CampeonatosDisponiveis.Add(item);
                        itensAdicionados++;
                    }

                    // LOG FINAL: Confirmação da contagem final
                    Debug.WriteLine($"[MainThread] Itens adicionados: {itensAdicionados}. Contagem final de CampeonatosDisponiveis: {CampeonatosDisponiveis.Count}");

                    // Este é o momento onde a UI deveria renderizar.
                });

            } catch (Exception ex) {
                Debug.WriteLine($"[CarregarCampeonatosAsync] ERRO CRÍTICO: {ex.Message} \n {ex.StackTrace}");
            } finally {
                Debug.WriteLine("[CarregarCampeonatosAsync] FIM. Definindo IsBusy=False.");
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        public async Task SolicitarArbitragemAsync(CampeonatoItemViewModel campeonatoItemVM) {
            // Logs de Solicitação
            Debug.WriteLine($"[SolicitarArbitragemAsync] INÍCIO. Solicitando para: {campeonatoItemVM?.Nome ?? "NULO"}");
            if (campeonatoItemVM == null) return;

            // NOTE: IsBusy está sendo gerenciado aqui, mas o CanExecute do RelayCommand não é usado pelo evento Clicked
            IsBusy = true;
            try {
                var arbitroAtual = await _sessaoService.GetArbitroAtualAsync();
                if (arbitroAtual == null) {
                    await _alertService.DisplayAlert("Erro", "Sessão de Árbitro não encontrada.", "OK");
                    return;
                }

                var solicitacao = new Convite {
                    ClientAppId = Guid.NewGuid(),
                    UsuarioClientAppId = arbitroAtual.ClientAppId,
                    CampeonatoClientAppId = campeonatoItemVM.Campeonato.ClientAppId,
                    Tipo = TipoConvite.InscricaoArbitro,
                    Status = StatusConvite.Pendente,
                    DataCriacao = DateTime.UtcNow
                };

                await _databaseService.InserirConviteAsync(solicitacao);

                await _alertService.DisplayAlert(
                  "Sucesso",
                  $"Sua solicitação de arbitragem no campeonato '{campeonatoItemVM.Nome}' foi enviada.",
                  "OK");

                // Atualização local do item para refletir a solicitação
                MainThread.BeginInvokeOnMainThread(() => {
                    campeonatoItemVM.ButtonText = "Pendente";
                    campeonatoItemVM.IsButtonEnabled = false;
                    campeonatoItemVM.ButtonColor = Color.FromArgb("#9E9E9E");
                });

                Debug.WriteLine("[SolicitarArbitragemAsync] Solicitação concluída e item da UI atualizado.");

            } catch (Exception ex) {
                Debug.WriteLine($"[SolicitarArbitragemAsync] ERRO: {ex.Message}");
                await _alertService.DisplayAlert("Erro", "Ocorreu um erro ao enviar sua solicitação.", "OK");
            } finally {
                IsBusy = false;
                Debug.WriteLine("[SolicitarArbitragemAsync] FIM.");
            }
        }
    }
}