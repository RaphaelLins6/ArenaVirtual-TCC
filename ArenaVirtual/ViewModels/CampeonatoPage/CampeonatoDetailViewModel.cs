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

            Debug.WriteLine($"[DEBUG-VM] IsOrganizador SET TO: {IsOrganizador}");
            Debug.WriteLine($"[DEBUG-VM] -> Campeonato.OrganizadorId: {campeonato.OrganizadorId}");
            Debug.WriteLine($"[DEBUG-VM] -> UsuarioAtual.Id: {usuarioAtual?.Id}");

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
                var jogosDaRodada = _jogosPorRodada[rodada];
                foreach (var jogo in jogosDaRodada) {
                    jogo.IsOrganizador = this.IsOrganizador; // Atribui a propriedade reativa
                }

                // Força a reavaliação do CollectionView:
                // 1. Limpa a coleção existente
                TabelaJogos.Clear();
                // 2. Adiciona todos os jogos da rodada novamente
                foreach (var jogo in jogosDaRodada) {
                    TabelaJogos.Add(jogo);
                }
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

        [RelayCommand]
        public async Task ListarArbitrosInscritos() {
            if (Campeonato is null) {
                Debug.WriteLine("Erro: Campeonato é nulo.");
                return;
            }

            var navigationParameters = new ShellNavigationQueryParameters
            {
                { "CampeonatoClientAppId", Campeonato.ClientAppId }
            };

            await Shell.Current.GoToAsync(nameof(ArbitrosInscritosPage), navigationParameters);
        }

        [RelayCommand]
        public async Task AnexarArbitros(Jogo jogo) {
            Debug.WriteLine("========================================================================");
            Debug.WriteLine("[DEBUG-CLIQUE] INÍCIO: O comando AnexarArbitros foi acionado.");
            Debug.WriteLine($"[DEBUG-CLIQUE] PARAM: O objeto 'jogo' recebido é: {(jogo == null ? "NULO" : "NÃO NULO")}");

            if (!IsOrganizador) {
                Debug.WriteLine("[DEBUG-CLIQUE] VERIFICAÇÃO: Usuário não é o Organizador. Acesso negado.");
                await _alertService.DisplayAlert("Acesso Negado", "Somente o organizador pode anexar árbitros a um jogo.", "OK");
                return;
            }

            Debug.WriteLine("[DEBUG-CLIQUE] VERIFICAÇÃO: Usuário é o Organizador. Prosseguindo...");

            if (jogo is null) {
                Debug.WriteLine("[DEBUG-CLIQUE] ERRO LÓGICO: O objeto Jogo recebido é NULO.");
                await _alertService.DisplayAlert("Erro de Dados", "O jogo selecionado não pôde ser carregado.", "OK");
                return;
            }

            Debug.WriteLine($"[DEBUG-CLIQUE] DADOS RECEBIDOS: Jogo ID: {jogo.Id} | Times: {jogo.TimeA?.Nome ?? "N/A"} vs {jogo.TimeB?.Nome ?? "N/A"}.");

            try {
                Debug.WriteLine("[DEBUG-CLIQUE] NAVEGAÇÃO: Criando a instância do AtribuirArbitrosPopup...");

                var popup = new AtribuirArbitrosPopup(
              Campeonato,
              jogo,
              _alertService,
              _databaseService,
              _syncService
                );

                Debug.WriteLine("[DEBUG-CLIQUE] NAVEGAÇÃO: Chamando Application.Current.MainPage.Navigation.PushModalAsync...");

                await Application.Current.MainPage.Navigation.PushModalAsync(popup);

                Debug.WriteLine("[DEBUG-CLIQUE] FIM: PushModalAsync acionado com sucesso. Pop-up deve estar visível.");
            } catch (Exception ex) {
                Debug.WriteLine($"[DEBUG-CLIQUE] ERRO CRÍTICO: Falha ao abrir o Pop-up. Detalhes: {ex.Message}");
                await _alertService.DisplayAlert("Erro de Navegação", $"Não foi possível abrir a tela de árbitros: {ex.Message}", "OK");
            }

            Debug.WriteLine("========================================================================");
        }
    }
}