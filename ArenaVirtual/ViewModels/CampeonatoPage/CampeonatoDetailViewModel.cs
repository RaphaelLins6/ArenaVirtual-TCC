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

        public CampeonatoDetailViewModel(IAlertService alertService, DatabaseService databaseService, SyncService syncService) {
            TabelaClassificacao = new ObservableCollection<Time>();
            TabelaJogos = new ObservableCollection<Jogo>();
            _alertService = alertService;
            _databaseService = databaseService;
            _syncService = syncService;

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

            // 🆕 Chama a Geração de Jogos após carregar a classificação para usar a lista de times.
            await GerarTabelaJogosAsync(campeonato);

            // 🆕 Inicia a Rodada Atual.
            RodadaAtual = _jogosPorRodada.Keys.Any() ? _jogosPorRodada.Keys.Min() : 0;
            if (RodadaAtual > 0) {
                LoadRodada(RodadaAtual);
            }

            // Carregamento de Banner e Logo (mantido inalterado)
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

        // NOVO MÉTODO: Geração da Tabela de Jogos usando Round-Robin com distribuição de datas por rodada
        private async Task GerarTabelaJogosAsync(Campeonato campeonato) {
            _jogosPorRodada.Clear();
            var times = TabelaClassificacao.ToList();
            int n = times.Count;

            if (n < 2) return; // Mínimo de 2 times para jogos

            // Trata número ímpar: adiciona um "time dummy" (Folga)
            if (n % 2 != 0) {
                // Adiciona um time temporário para facilitar o Round-Robin
                times.Add(new Time { Id = -1, Nome = "Folga", LogoUrl = "" });
                n++; // O número de elementos para o algoritmo se torna par
            }

            int numRodadas = n - 1; // Para turno único (ex: 4 times -> 3 rodadas)
            int numJogosPorRodada = n / 2; // (ex: 4 times -> 2 jogos por rodada)

            // --- LÓGICA DE DISTRIBUIÇÃO DE DATAS POR RODADA ---

            // Data/Hora base do jogo (ex: 18:00h no dia de início)
            DateTime dataHoraBase = campeonato.DataInicio.Date.AddHours(18);

            // Calcula a duração total do campeonato (dias)
            TimeSpan duracaoCampeonato = campeonato.DataFim.Date - campeonato.DataInicio.Date;
            int totalDias = (int)duracaoCampeonato.TotalDays;

            // Se o campeonato tiver mais de um dia e mais de uma rodada, calcula o espaçamento médio.
            double intervaloDiasPorRodada = 0;
            if (totalDias > 0 && numRodadas > 1) {
                // Divide a duração total pelos intervalos de rodada (numRodadas - 1)
                intervaloDiasPorRodada = (double)totalDias / (numRodadas - 1);
            }
            // ----------------------------------------------------

            // Implementação do Algoritmo Cíclico (Round-Robin)
            // O primeiro time (times[0]) fica fixo.
            var timesRotativos = times.Skip(1).ToList();

            for (int r = 1; r <= numRodadas; r++) {
                var rodadaJogos = new ObservableCollection<Jogo>();

                // 1. Definição da Data/Hora para toda a Rodada
                // A data avança a cada rodada, baseada no intervalo calculado.
                DateTime dataHoraRodada = dataHoraBase.AddDays((r - 1) * intervaloDiasPorRodada);

                // Garante que a data não ultrapasse o final do campeonato
                if (dataHoraRodada.Date > campeonato.DataFim.Date) {
                    dataHoraRodada = campeonato.DataFim.Date.AddHours(dataHoraBase.Hour);
                }

                // 2. Jogo Fixo (Times[0] vs Times Rotativos[0])
                Time timeA = times[0];
                Time timeB = timesRotativos[0];

                if (timeB.Id != -1) { // Verifica se não é a Folga
                    rodadaJogos.Add(CriarNovoJogo(timeA, timeB, r, campeonato.Local, dataHoraRodada));
                } else {
                    Debug.WriteLine($"Rodada {r}: {timeA.Nome} Folga.");
                }

                // 3. Outros Jogos (Times Rotativos emparelhados de fora para dentro)
                for (int i = 1; i < numJogosPorRodada; i++) {
                    Time timeX = timesRotativos[i];
                    Time timeY = timesRotativos[numRodadas - i];

                    if (timeX.Id != -1 && timeY.Id != -1) { // Garante que nenhum é a Folga
                        // Todos os jogos na rodada usam a mesma DataHora e Local
                        rodadaJogos.Add(CriarNovoJogo(timeX, timeY, r, campeonato.Local, dataHoraRodada));
                    }
                }

                // Adiciona a rodada ao dicionário
                if (rodadaJogos.Any()) {
                    _jogosPorRodada.Add(r, rodadaJogos);
                }

                // 4. Rotaciona os times rotativos (mecanismo do Round-Robin)
                // O último time rotativo vai para a primeira posição (logo após o fixo)
                var ultimoTime = timesRotativos.Last();
                timesRotativos.RemoveAt(timesRotativos.Count - 1);
                timesRotativos.Insert(0, ultimoTime);
            }
        }

        // Método auxiliar para criar um novo Jogo
        private Jogo CriarNovoJogo(Time timeA, Time timeB, int rodada, string localCampeonato, DateTime dataHora) {
            return new Jogo {
                TimeA = timeA,
                TimeB = timeB,
                PlacarA = "X", // Placeholder
                PlacarB = "Y", // Placeholder
                Rodada = rodada,
                DataHora = dataHora,
                Local = localCampeonato
            };
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