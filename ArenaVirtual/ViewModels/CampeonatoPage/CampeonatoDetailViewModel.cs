using ArenaVirtual.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using ArenaVirtual.Services;
using System.Collections.ObjectModel;
using ArenaVirtual.Popups;
using ArenaVirtual.Views.CampeonatoPage;

namespace ArenaVirtual.ViewModels.CampeonatoPage {
    public partial class CampeonatoDetailViewModel : ObservableObject, IQueryAttributable {
        [ObservableProperty]
        private Campeonato campeonato;

        [ObservableProperty]
        private ObservableCollection<Time> tabelaClassificacao;

        [ObservableProperty]
        private ObservableCollection<Jogo> tabelaJogos;

        [ObservableProperty]
        private int rodadaAtual;

        [ObservableProperty]
        private bool isOrganizador = false;

        [ObservableProperty]
        private ImageSource bannerSource;

        [ObservableProperty]
        private ImageSource logoSource;

        [ObservableProperty]
        private bool isDesktop;

        private readonly Dictionary<int, ObservableCollection<Jogo>> _jogosPorRodada = new();

        private readonly IAlertService _alertService;
        private readonly DatabaseService _databaseService;
        private readonly SyncService _syncService;
        // 🆕 Adição do JogoService
        private readonly IJogoService _jogoService;

        // 🆕 Construtor injetando o novo serviço
        public CampeonatoDetailViewModel(IAlertService alertService, DatabaseService databaseService, SyncService syncService, IJogoService jogoService) {
            TabelaClassificacao = new ObservableCollection<Time>();
            TabelaJogos = new ObservableCollection<Jogo>();
            _alertService = alertService;
            _databaseService = databaseService;
            _syncService = syncService;
            _jogoService = jogoService; // Armazenando o serviço

            // Verifica o idioma do dispositivo para definir IsDesktop
            IsDesktop = DeviceInfo.Idiom == DeviceIdiom.Desktop || DeviceInfo.Idiom == DeviceIdiom.Tablet;

            Debug.WriteLine($"[CampeonatoDetailViewModel] Device Idiom: {DeviceInfo.Idiom}. IsDesktop: {IsDesktop}");
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query) {
            Debug.WriteLine("[CampeonatoDetailViewModel] ApplyQueryAttributes chamado.");
            if (query.ContainsKey("Campeonato")) {
                var campeonatoRecebido = query["Campeonato"] as Campeonato;
                // Chamada LoadCampeonato com await para garantir que a UI não trave e que os dados sejam carregados
                await LoadCampeonato(campeonatoRecebido);
            }
        }

        public async Task LoadCampeonato(Campeonato campeonato) {
            Debug.WriteLine("[CampeonatoDetailViewModel] LoadCampeonato chamado.");
            if (campeonato == null) {
                Debug.WriteLine("[CampeonatoDetailViewModel] Campeonato é nulo, retornando.");
                return;
            }

            Campeonato = campeonato;

            var usuarioAtual = SessaoService.Instancia.GetUsuarioAtual();
            IsOrganizador = (campeonato.OrganizadorId == usuarioAtual?.Id);

            Debug.WriteLine($"[CampeonatoDetailViewModel] É organizador? {IsOrganizador}");

            // 🚀 CHAMADA DE DADOS REAIS E POPULAÇÃO DAS TABELAS
            await LoadTabelaClassificacaoAsync();

            // 🆕 Chama a Geração de Jogos usando o Service
            await GerarTabelaJogosAsync(campeonato);

            // 🆕 Inicia a Rodada Atual.
            RodadaAtual = _jogosPorRodada.Keys.Any() ? _jogosPorRodada.Keys.Min() : 0;
            if (RodadaAtual > 0) {
                LoadRodada(RodadaAtual);
            }

            // ... (Lógica de Carregamento de Banner e Logo permanece inalterada)
            if (!string.IsNullOrEmpty(Campeonato.BannerUrl)) {
                if (File.Exists(Campeonato.BannerUrl)) {
                    BannerSource = ImageSource.FromFile(Campeonato.BannerUrl);
                } else if (Uri.IsWellFormedUriString(Campeonato.BannerUrl, UriKind.Absolute)) {
                    BannerSource = ImageSource.FromUri(new Uri(Campeonato.BannerUrl));
                } else {
                    BannerSource = ImageSource.FromFile("default_banner.png");
                }
            } else {
                BannerSource = ImageSource.FromFile("default_banner.png");
            }

            if (!string.IsNullOrEmpty(Campeonato.LogoUrl)) {
                if (File.Exists(Campeonato.LogoUrl)) {
                    LogoSource = ImageSource.FromFile(Campeonato.LogoUrl);
                } else if (Uri.IsWellFormedUriString(Campeonato.LogoUrl, UriKind.Absolute)) {
                    LogoSource = ImageSource.FromUri(new Uri(Campeonato.LogoUrl));
                } else {
                    LogoSource = ImageSource.FromFile("default_logo.png");
                }
            } else {
                LogoSource = ImageSource.FromFile("default_logo.png");
            }
        }

        [RelayCommand]
        private async Task AlterarBanner() {
            Debug.WriteLine("[CampeonatoDetailViewModel] Botão 'Alterar Banner' clicado.");
            var popup = new AlterarBannerPopup(Campeonato, _alertService, _databaseService, _syncService);

            popup.BannerAtualizado += (s, newBannerPath) => {
                Debug.WriteLine($"[CampeonatoDetailViewModel] Evento BannerAtualizado recebido com caminho: '{newBannerPath}'");
                MainThread.BeginInvokeOnMainThread(() => {
                    if (!string.IsNullOrEmpty(newBannerPath) && File.Exists(newBannerPath)) {
                        Debug.WriteLine("[CampeonatoDetailViewModel] Arquivo de banner existe. Atualizando BannerSource.");
                        BannerSource = ImageSource.FromFile(newBannerPath);
                    } else {
                        Debug.WriteLine("[CampeonatoDetailViewModel] Caminho do novo banner é nulo/vazio ou arquivo não encontrado.");
                    }
                });
            };

            // Em MAUI/Xamarin, o PushModalAsync deve ser feito no MainThread
            await Application.Current.MainPage.Navigation.PushModalAsync(popup);
        }

        // MÉTODO MELHORADO: Carrega times e popula a classificação
        private async Task LoadTabelaClassificacaoAsync() {
            if (Campeonato is null) return;

            // 1. Busca os times reais inscritos
            var timesInscritos = await _databaseService.ObterTimesAceitosAsync(Campeonato.Id);

            // 2. Lógica de Classificação (ordenar e calcular estatísticas)
            var timesOrdenados = timesInscritos
                                .OrderByDescending(t => t.PontuacaoTotal)
                                .ToList();

            TabelaClassificacao.Clear();

            // 3. Popula a Tabela de Classificação
            for (int i = 0; i < timesOrdenados.Count; i++) {
                var time = timesOrdenados[i];

                // 3.1. Atribui a posição e calcula as colunas.
                time.Posicao = i + 1;

                // Garantindo que a Porcentagem de Vitória seja calculada:
                int totalJogosDecididos = time.Vitorias + time.Derrotas;
                time.PorcentagemVitoria = (totalJogosDecididos > 0) ? (double)time.Vitorias / totalJogosDecididos : 0.0;

                // Valores simulados/incompletos para JA e Sequencia até a lógica real ser implementada
                time.JogosAtras = 0;
                time.Sequencia = time.Vitorias > 0 ? "V" : (time.Derrotas > 0 ? "D" : "N/A");

                TabelaClassificacao.Add(time);
            }
        }

        private async Task GerarTabelaJogosAsync(Campeonato campeonato) {
            _jogosPorRodada.Clear();

            var times = TabelaClassificacao.ToList();

            // 🚀 Chama o JogoService para gerar os jogos (Certifique-se que o tipo é List<Time>)
            // Isso resolve a ambiguidade (CS0121) causada pela duplicação de interfaces no JogoService.cs
            var jogosGerados = await _jogoService.GerarTabelaJogosAsync(campeonato, times);

            // Atualiza o dicionário interno do ViewModel
            _jogosPorRodada.Clear();
            foreach (var key in jogosGerados.Keys) {
                _jogosPorRodada.Add(key, jogosGerados[key]);
            }
        }

        [RelayCommand]
        private void MudarRodadaAnterior() {
            if (RodadaAtual > 0 && _jogosPorRodada.Keys.Any() && RodadaAtual > _jogosPorRodada.Keys.Min()) {
                RodadaAtual--;
                LoadRodada(RodadaAtual);
            }
        }

        [RelayCommand]
        private void MudarRodadaProxima() {
            if (RodadaAtual > 0 && _jogosPorRodada.Keys.Any() && RodadaAtual < _jogosPorRodada.Keys.Max()) {
                RodadaAtual++;
                LoadRodada(RodadaAtual);
            }
        }

        private void LoadRodada(int rodada) {
            if (_jogosPorRodada.ContainsKey(rodada)) {
                // ATUALIZA A PROPRIEDADE OBSERVÁVEL
                TabelaJogos = _jogosPorRodada[rodada];
            } else {
                TabelaJogos.Clear();
            }
        }

        [RelayCommand]
        public async Task GerenciarSolicitacoes() {
            if (Campeonato is null) {
                Debug.WriteLine("Erro: Campeonato é nulo.");
                return;
            }

            var navigationParameters = new ShellNavigationQueryParameters
            {
        { "Campeonato", Campeonato }
      };

            await Shell.Current.GoToAsync(nameof(GerenciarSolicitacoesPage), navigationParameters);
        }

        [RelayCommand]
        public async Task ListarTimesInscritos() {
            if (Campeonato is null) {
                Debug.WriteLine("Erro: Campeonato é nulo.");
                return;
            }

            var navigationParameters = new ShellNavigationQueryParameters
            {
                { "CampeonatoId", Campeonato.Id }
            };

            await Shell.Current.GoToAsync(nameof(TimesCadastradosPage), navigationParameters);
        }
    }
}