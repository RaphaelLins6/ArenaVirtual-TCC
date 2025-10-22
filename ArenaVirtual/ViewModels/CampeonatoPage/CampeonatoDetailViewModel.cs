using ArenaVirtual.Models;
using ArenaVirtual.Popups;
using ArenaVirtual.Services;
using ArenaVirtual.Views.CampeonatoPage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ArenaVirtual.ViewModels.CampeonatoPage {
    public class Grouping<K, T> : ObservableCollection<T> {
        public K Key { get; private set; }
        public Grouping(K key, IEnumerable<T> items) {
            Key = key;
            foreach (var item in items)
                this.Items.Add(item);
        }
    }
    public class RodadaGrouping : Grouping<string, Jogo> {
        public RodadaGrouping(string key, IEnumerable<Jogo> items) : base(key, items) { }
        public string NomeRodada => Key;
    }

    public partial class CampeonatoDetailViewModel : ObservableObject, IQueryAttributable {

        // =====================================================================================
        // PROPRIEDADES OBSERVÁVEIS
        // =====================================================================================
        [ObservableProperty]
        private Campeonato campeonato;

        [ObservableProperty]
        private ObservableCollection<Time> tabelaClassificacao;

        [ObservableProperty]
        private ObservableCollection<Jogo> tabelaJogos;

        [ObservableProperty]
        private ObservableCollection<TimeEstatisticaViewModel> estatisticasTimes;

        // NOVO: Propriedade para a lista de líderes de jogadores
        [ObservableProperty]
        private ObservableCollection<JogadorEstatisticaViewModel> lideresEstatisticas = new();

        // NOVO: Propriedade para o filtro de estatística (o que está selecionado)
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ItensEstatisticas))]
        private string estatisticaSelecionada = "Pontos"; // Padrão: Pontos

        // NOVO: Coleção com as estatísticas disponíveis para o filtro
        [ObservableProperty]
        private ObservableCollection<string> tiposEstatisticas = new()
        {
            "Pontos", "Assistências", "Rebotes", "Roubos", "Bloqueios",
            "Turnovers", "Faltas", "2 Pontos %", "3 Pontos %", "Lance Livre %"
        };


        [ObservableProperty]
        private ObservableCollection<RodadaGrouping> jogosMataMata = new();

        [ObservableProperty]
        private int rodadaAtual;

        [ObservableProperty]
        private bool isOrganizador = false;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private ImageSource bannerSource;

        [ObservableProperty]
        private ImageSource logoSource;

        [ObservableProperty]
        private bool isDesktop;

        [ObservableProperty]
        private ObservableCollection<string> gruposDisponiveis = new();

        [ObservableProperty]
        private string? grupoSelecionado;

        // PROPRIEDADES DE CONTROLE DE FASE
        [ObservableProperty]
        private bool isFormatoHibrido = false;

        [ObservableProperty]
        private ObservableCollection<string> fasesDisponiveis = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsFaseTabelaEJogos))]
        [NotifyPropertyChangedFor(nameof(IsFaseMataMata))]
        private string faseAtual = "Tabela & Jogos";

        [ObservableProperty]
        private bool isFiltroGrupoVisivel = false;

        [ObservableProperty]
        private bool isFiltroFaseVisivel = false;

        [ObservableProperty]
        private ObservableCollection<CampanhaPatrocinio> patrocinadoresAtivos = new();

        [ObservableProperty]
        private string bannerDivulgacaoSource;

        [ObservableProperty]
        private bool isPatrocinioDivulgacaoVisible;

        // Campos privado
        private readonly Dictionary<int, ObservableCollection<Jogo>> _jogosPorRodada = new();
        private readonly Dictionary<string, List<Time>> _timesPorGrupo = new();
        private CampanhaPatrocinio? _campanhaPatrocinioAtiva;
        private List<Jogo> _todosOsJogosDoCampeonato = new();

        // =====================================================================================
        // SERVIÇOS INJETADOS (INCLUSÃO DO IPATROCINIOSERVICE)
        // =====================================================================================
        private readonly IAlertService _alertService;
        private readonly DatabaseService _databaseService; // Usando a interface para ser mais coerente
        private readonly SyncService _syncService;
        private readonly UsuarioService _usuarioService;
        private readonly IJogoService _jogoService;
        private readonly PatrocinioService _patrocinioService;

        // Propriedades calculadas
        public bool IsFormatoComGrupos =>
            Campeonato?.FormatoCampeonato?.IndexOf("Grupos", StringComparison.OrdinalIgnoreCase) >= 0 ||
            Campeonato?.FormatoCampeonato?.IndexOf("Fase de Grupos", StringComparison.OrdinalIgnoreCase) >= 0;

        public bool IsTabelaFormat =>
            Campeonato?.FormatoCampeonato?.IndexOf("Pontos", StringComparison.OrdinalIgnoreCase) >= 0 ||
            IsFormatoComGrupos;

        public bool IsMataMataFormat =>
            Campeonato?.FormatoCampeonato?.IndexOf("Mata-mata", StringComparison.OrdinalIgnoreCase) >= 0 ||
            Campeonato?.FormatoCampeonato?.IndexOf("Eliminação", StringComparison.OrdinalIgnoreCase) >= 0;

        public bool IsFaseTabelaEJogos => FaseAtual == "Tabela & Jogos";
        public bool IsFaseMataMata => FaseAtual == "Mata-Mata";

        // NOVO: Propriedade para o CollectionView, filtrada e ordenada com base no 'EstatisticaSelecionada'
        public ObservableCollection<JogadorEstatisticaViewModel> ItensEstatisticas =>
            GetItensEstatisticasOrdenados(EstatisticaSelecionada);

        // =====================================================================================
        // CONSTRUTOR (INCLUSÃO DO IPATROCINIOSERVICE)
        // =====================================================================================
        public CampeonatoDetailViewModel(
            IAlertService alertService,
            DatabaseService databaseService, // Assume-se que DatabaseService implementa IDatabaseService
            SyncService syncService,
            IJogoService jogoService,
            UsuarioService usuarioService,
            // ⭐️ PARÂMETRO NOVO: Recebendo o serviço por injeção ⭐️
            PatrocinioService patrocinioService) {

            TabelaClassificacao = new ObservableCollection<Time>();
            TabelaJogos = new ObservableCollection<Jogo>();
            EstatisticasTimes = new ObservableCollection<TimeEstatisticaViewModel>();

            _alertService = alertService;
            _databaseService = (DatabaseService)databaseService; // Pode ser necessário um cast se a interface não estiver sendo usada
            _syncService = syncService;
            _jogoService = jogoService;
            _usuarioService = usuarioService;
            // ⭐️ ATRIBUIÇÃO NO CONSTRUTOR ⭐️
            _patrocinioService = patrocinioService;

            IsDesktop = DeviceInfo.Idiom == DeviceIdiom.Desktop || DeviceInfo.Idiom == DeviceIdiom.Tablet;
            Debug.WriteLine($"[CampeonatoDetailViewModel] Device Idiom: {DeviceInfo.Idiom}. IsDesktop: {IsDesktop}");
        }

        partial void OnGrupoSelecionadoChanged(string? value) {
            if (value != null) {
                LoadTabelaClassificacaoPorGrupo(value);
            }
        }

        partial void OnFaseAtualChanged(string? value) {
            if (value != null) {
                _ = ExecuteOnFaseAtualChangedAsync(value);
            }
        }

        private async Task ExecuteOnFaseAtualChangedAsync(string newValue) {
            Debug.WriteLine($"[DEBUG-FASES] Nova fase selecionada: {newValue}");

            if (newValue == "Mata-Mata") {
                IsFiltroGrupoVisivel = false;
                await LoadTabelaClassificacaoAsync();
                await GerarJogosMataMata();

            } else if (newValue == "Tabela & Jogos") {
                IsFiltroGrupoVisivel = IsFormatoComGrupos;
                await LoadTabelaClassificacaoAsync();
                LoadRodada(RodadaAtual);
            }
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query) {
            Debug.WriteLine("[DEBUG-ATTRIBUTES] ApplyQueryAttributes chamado.");

            // 1. Atualização de Jogo (Árbitro) ou Jogo (Placar/Outros)
            if (query.TryGetValue("jogoAtualizado", out object jogoObj) && jogoObj is Jogo jogoAtualizado) {
                Debug.WriteLine($"[DEBUG-ATTRIBUTES] Jogo ID {jogoAtualizado.Id} foi atualizado.");
                query.Remove("jogoAtualizado");

                _ = LoadTabelaClassificacaoAsync();
                _ = RecarregarJogosESelecaoAsync();

                if (IsMataMataFormat || IsFormatoHibrido)
                    _ = GerarJogosMataMata();

                _ = LoadPatrocinadoresAsync();
                _ = LoadLideresEstatisticasAsync();

                Debug.WriteLine($"[DEBUG-ATTRIBUTES] Recarga completa do Campeonato após atualização do Jogo ID {jogoAtualizado.Id}.");
                return;
            }

            // 2. Atualização de Times (Inscrição)
            if (query.ContainsKey("TimesAtualizados")) {
                Debug.WriteLine("[DEBUG-ATTRIBUTES] Lista de Times foi atualizada. Recarregando Classificação e Jogos.");
                query.Remove("TimesAtualizados");

                if (Campeonato != null) {
                    _ = LoadTabelaClassificacaoAsync()
                        .ContinueWith(t => {
                            if (IsFaseTabelaEJogos && IsTabelaFormat)
                                _ = RecarregarJogosESelecaoAsync();
                            else if (IsFaseMataMata && IsMataMataFormat)
                                _ = GerarJogosMataMata();
                        }, TaskScheduler.FromCurrentSynchronizationContext());

                    _ = LoadPatrocinadoresAsync();
                    _ = LoadLideresEstatisticasAsync();
                }
                return;
            }

            // 3. Carregamento Inicial do Campeonato
            if (query.ContainsKey("Campeonato")) {
                var campeonatoRecebido = query["Campeonato"] as Campeonato;
                if (campeonatoRecebido == null) return;

                if (Campeonato == null || Campeonato.Id != campeonatoRecebido.Id) {
                    _ = LoadCampeonato(campeonatoRecebido);
                } else {
                    Campeonato = campeonatoRecebido;
                    OnPropertyChanged(nameof(IsFormatoComGrupos));
                    OnPropertyChanged(nameof(IsTabelaFormat));
                    OnPropertyChanged(nameof(IsMataMataFormat));
                    AtualizarFormatoCampeonato();
                    Debug.WriteLine("[DEBUG-ATTRIBUTES] ApplyQueryAttributes ignorou LoadCampeonato (Campeonato já carregado).");
                }

                // ----------------------------------------------------
                // CARREGAR PATROCINADORES E BANNER DE DIVULGAÇÃO COM FALLBACK
                // ----------------------------------------------------
                if (Campeonato != null) {
                    _ = LoadPatrocinadoresAsync();
                    _ = LoadLideresEstatisticasAsync();
                }
            }
            query.Clear();
        }

        private async Task RecarregarJogosESelecaoAsync() {
            if (Campeonato == null || !IsTabelaFormat) return;
            Debug.WriteLine("[DEBUG-RELOAD] Iniciando RecarregarJogosESelecaoAsync.");

            await GerarTabelaJogosAsync(Campeonato);
            Debug.WriteLine($"[DEBUG-RELOAD] GerarTabelaJogosAsync concluído. RodadaAtual: {RodadaAtual}");
            if (RodadaAtual > 0) {
                MainThread.BeginInvokeOnMainThread(() => {
                    Debug.WriteLine($"[DEBUG-RELOAD] Chamando LoadRodada({RodadaAtual}) na MainThread.");
                    LoadRodada(RodadaAtual);
                });
            }

            Debug.WriteLine("[DEBUG-RELOAD] Finalizando RecarregarJogosESelecaoAsync.");
        }

        public async Task LoadCampeonato(Campeonato campeonato) {
            Debug.WriteLine("[CampeonatoDetailViewModel] LoadCampeonato chamado.");
            if (IsBusy) return;

            try {
                IsBusy = true;
                if (campeonato == null) {
                    Debug.WriteLine("[CampeonatoDetailViewModel] Campeonato é nulo, retornando.");
                    return;
                }

                Campeonato = campeonato;
                OnPropertyChanged(nameof(IsFormatoComGrupos));
                OnPropertyChanged(nameof(IsTabelaFormat));
                OnPropertyChanged(nameof(IsMataMataFormat));

                AtualizarFormatoCampeonato();

                var usuarioAtual = SessaoService.Instancia.GetUsuarioAtual();
                IsOrganizador = (campeonato.OrganizadorId == usuarioAtual?.Id);
                Debug.WriteLine($"[CampeonatoDetailViewModel] É organizador? {IsOrganizador}. Formato Tabela: {IsTabelaFormat}, Mata-Mata: {IsMataMataFormat}, Híbrido: {IsFormatoHibrido}");

                await LoadTabelaClassificacaoAsync();

                // 4.1.a - Exibir jogos conforme formato base
                if (IsTabelaFormat) {
                    await GerarTabelaJogosAsync(campeonato);
                    RodadaAtual = _jogosPorRodada.Keys.Any() ? _jogosPorRodada.Keys.Min() : 0;
                }

                if (IsFaseTabelaEJogos && IsTabelaFormat && RodadaAtual > 0) {
                    LoadRodada(RodadaAtual);
                } else if (IsFaseMataMata && IsMataMataFormat) {
                    await GerarJogosMataMata();
                }

                LoadImageSources();
            } catch (Exception ex) {
                Debug.WriteLine($"[ERRO CRÍTICO] LoadCampeonato falhou: {ex.Message}");
                await _alertService.DisplayAlert("Erro de Carregamento", "Ocorreu um erro ao carregar os detalhes do campeonato. Tente novamente.", "OK");
            } finally {
                IsBusy = false;
            }
        }

        private void AtualizarFormatoCampeonato() {
            if (Campeonato is null) return;

            Debug.WriteLine($"[DEBUG-FORMATO] Valor de FormatoCampeonato: '{Campeonato.FormatoCampeonato}'");

            bool isPontosMaisEliminatoria = Campeonato.FormatoCampeonato.IndexOf("Pontos + Eliminatórias", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isGruposMaisEliminatoria = Campeonato.FormatoCampeonato.IndexOf("Grupos + Eliminatórias", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isMaisEliminatoria = Campeonato.FormatoCampeonato.IndexOf("Mais Eliminatória", StringComparison.OrdinalIgnoreCase) >= 0;

            IsFormatoHibrido = isPontosMaisEliminatoria || isGruposMaisEliminatoria || isMaisEliminatoria;

            // Filtro de fases visível apenas em híbridos
            IsFiltroFaseVisivel = IsFormatoHibrido;

            Debug.WriteLine($"[DEBUG-FORMATO] IsFormatoHibrido set to: {IsFormatoHibrido}. IsFiltroFaseVisivel: {IsFiltroFaseVisivel}");

            FasesDisponiveis.Clear(); // Limpar a lista para reconstruir

            if (IsFormatoHibrido) {
                FasesDisponiveis.Add("Tabela & Jogos");
                FasesDisponiveis.Add("Mata-Mata");
                FaseAtual = "Tabela & Jogos"; // Padrão
            } else {
                // Em não híbridos, define a FaseAtual com base no formato principal.
                if (IsMataMataFormat)
                    FaseAtual = "Mata-Mata";
                else
                    FaseAtual = "Tabela & Jogos";

                // CORREÇÃO CRÍTICA: Adiciona a fase única para evitar problemas de binding
                // em componentes que esperam um item na lista de fases disponíveis.
                FasesDisponiveis.Add(FaseAtual);
            }

            // Garante que o filtro de grupo esteja visível na fase "Tabela & Jogos" se houver grupos
            if (IsFaseTabelaEJogos) {
                // A visibilidade do filtro de grupo só afeta a UI do filtro, não o conteúdo
                IsFiltroGrupoVisivel = IsFormatoComGrupos && GruposDisponiveis.Any();
            } else {
                IsFiltroGrupoVisivel = false;
            }
        }

        // Assumindo que esta é uma função membro da sua classe (ex: ViewModel)
        private string GetNomeRodada(int rodadaAtual, int totalRodadas) {
            // A Final é sempre a rodada com o maior número.
            // A contagem reversa (1 = Final, 2 = Semi, 3 = Quartas...) simplifica o mapeamento.
            int rodadaContagemReversa = totalRodadas - rodadaAtual + 1;

            // Mapeamento padrão, funciona para 4, 8, 16, 32... times
            switch (rodadaContagemReversa) {
                case 1:
                    return "FINAL";
                case 2:
                    return "Semi-Final";
                case 3:
                    return "Quartas de Final";
                case 4:
                    return "Oitavas de Final";
                case 5:
                    return "16 avos de Final";
                case 6:
                    return "32 avos de Final";
                default:
                    // Para rodadas preliminares muito grandes (ex: 64 avos), ou casos não mapeados
                    return $"Rodada {rodadaAtual} (Preliminar)";
            }
        }


        private async Task GerarJogosMataMata() {
            if (!IsMataMataFormat && !IsFormatoHibrido || Campeonato is null) return;

            MainThread.BeginInvokeOnMainThread(() => {
                JogosMataMata.Clear();
            });

            var timesAceitos = TabelaClassificacao.OrderByDescending(t => t.PontuacaoTotal).ToList();

            if (timesAceitos.Count < 2) {
                Debug.WriteLine("[MATA-MATA] Não há times suficientes para gerar o bracket.");
                return;
            }

            int mockIdCounter = -1;
            var mockJogosFlat = new List<Jogo>();
            int totalRodadas = 0; // Variável para armazenar o total de rodadas geradas

            // ===================================================================
            // LÓGICA DE GERAÇÃO E CÁLCULO DE TOTAL DE RODADAS
            // ===================================================================

            if (timesAceitos.Count == 2) {
                // Apenas Final
                totalRodadas = 1;

                var jogoFinal = new Jogo {
                    Id = mockIdCounter--,
                    TimeA = timesAceitos[0],
                    TimeAId = timesAceitos[0].Id,
                    TimeB = timesAceitos[1],
                    TimeBId = timesAceitos[1].Id,
                    Rodada = 1, // Rodada 1
                    IsOrganizador = IsOrganizador,
                    NomeArbitro = string.Empty,
                    Local = "A Definir",
                };
                mockJogosFlat.Add(jogoFinal);
                Debug.WriteLine("[MATA-MATA] Bracket de 2 times (Final) gerado.");

            } else if (timesAceitos.Count == 3) {
                // Semi (Rodada 1) e Final (Rodada 2)
                totalRodadas = 2;

                // Jogo da Semifinal (Times 2 vs 3)
                var jogoSemi1 = new Jogo {
                    Id = mockIdCounter--,
                    TimeA = timesAceitos[1],
                    TimeAId = timesAceitos[1].Id,
                    TimeB = timesAceitos[2],
                    TimeBId = timesAceitos[2].Id,
                    Rodada = 1,
                    IsOrganizador = IsOrganizador,
                    NomeArbitro = string.Empty,
                    Local = "A Definir",
                };
                mockJogosFlat.Add(jogoSemi1);

                // Jogo da Final (Time 1 (Bye) vs Vencedor Jogo 1)
                var vencedorPlaceholder = new Time {
                    Nome = $"Vencedor Jogo {Math.Abs(jogoSemi1.Id)}",
                    LogoUrl = "default_logo.png",
                    Id = jogoSemi1.Id
                };

                var jogoFinal = new Jogo {
                    Id = mockIdCounter--,
                    TimeA = timesAceitos[0], // Time que ganhou o Bye
                    TimeAId = timesAceitos[0].Id,
                    TimeB = vencedorPlaceholder,
                    TimeBId = jogoSemi1.Id, // Referência ao ID do Jogo de Semifinal
                    Rodada = 2,
                    IsOrganizador = IsOrganizador,
                    NomeArbitro = string.Empty,
                    Local = "A Definir",
                };
                mockJogosFlat.Add(jogoFinal);
                Debug.WriteLine("[MATA-MATA] Bracket de 3 times gerado (Semi + Final com Bye).");

            } else if (timesAceitos.Count >= 4) {
                // Geração de Bracket Completo (N >= 4)

                var mockTimeBye = new Time { Nome = "BYE", LogoUrl = "default_logo.png", Id = mockIdCounter-- };
                var participantesRodadaAtual = timesAceitos.ToList();
                int rodadaAtual = 1;

                // Variável de controle (para calcular totalRodadas após o loop)
                // O total de rodadas será (rodadaAtual - 1) após o loop.

                while (participantesRodadaAtual.Count > 1) {
                    var participantesProximaRodada = new List<Time>();
                    int numJogosRodada = (int)Math.Ceiling(participantesRodadaAtual.Count / 2.0);

                    Debug.WriteLine($"[MATA-MATA] Gerando Rodada {rodadaAtual}. Número de times: {participantesRodadaAtual.Count}. Jogos: {numJogosRodada}");

                    // Emparelhamento dos times na rodada atual
                    for (int i = 0; i < numJogosRodada; i++) {
                        Time timeA = participantesRodadaAtual[i * 2];
                        Time timeB;

                        int indexB = i * 2 + 1;

                        if (indexB < participantesRodadaAtual.Count) {
                            timeB = participantesRodadaAtual[indexB];
                        } else {
                            timeB = mockTimeBye;
                        }

                        var novoJogo = new Jogo {
                            Id = mockIdCounter--,
                            TimeA = timeA,
                            TimeAId = timeA.Id,
                            TimeB = timeB,
                            TimeBId = timeB.Id,
                            Rodada = rodadaAtual,
                            IsOrganizador = IsOrganizador,
                            NomeArbitro = string.Empty,
                            Local = "A Definir",
                        };
                        mockJogosFlat.Add(novoJogo);

                        // Adiciona o placeholder do vencedor para a próxima rodada
                        if (timeB.Nome == "BYE") {
                            participantesProximaRodada.Add(timeA);
                        } else {
                            var vencedorPlaceholder = new Time {
                                Nome = $"Vencedor Jogo {Math.Abs(novoJogo.Id)}",
                                LogoUrl = "default_logo.png",
                                Id = novoJogo.Id
                            };
                            participantesProximaRodada.Add(vencedorPlaceholder);
                        }
                    }

                    // Prepara para a próxima iteração
                    participantesRodadaAtual = participantesProximaRodada;
                    rodadaAtual++;
                }

                // O número total de rodadas é a última rodada gerada
                totalRodadas = rodadaAtual - 1;

                Debug.WriteLine($"[MATA-MATA] Bracket de Múltiplas Rodadas gerado. Total de Rodadas: {totalRodadas}");
            }

            // ===================================================================
            // AGRUPAMENTO E USO DA FUNÇÃO DE NOMENCLATURA
            // ===================================================================

            Debug.WriteLine($"[MATA-MATA] Total de jogos planos gerados: {mockJogosFlat.Count}");

            // Agrupamento final por número da rodada e conversão para nome
            var groupedJogos = mockJogosFlat
                .OrderBy(j => j.Rodada)
                .GroupBy(j => j.Rodada)
                // AQUI USAMOS A FUNÇÃO DE NOMENCLATURA!
                .Select(g => new RodadaGrouping(GetNomeRodada(g.Key, totalRodadas), g))
                .ToList();

            Debug.WriteLine($"[MATA-MATA] Total de grupos (Rodadas) gerados: {groupedJogos.Count}");


            MainThread.BeginInvokeOnMainThread(() => {
                JogosMataMata.Clear();
                foreach (var group in groupedJogos) {
                    JogosMataMata.Add(group);
                }
                Debug.WriteLine($"[MATA-MATA] Total de grupos adicionados: {JogosMataMata.Count}");
            });
        }

        private void LoadImageSources() {
            try {
                Debug.WriteLine($"[DEBUG-IMAGEM] BannerUrl: {Campeonato?.BannerUrl}");
                if (!string.IsNullOrEmpty(Campeonato?.BannerUrl)) {
                    if (File.Exists(Campeonato.BannerUrl))
                        BannerSource = ImageSource.FromFile(Campeonato.BannerUrl);
                    else if (Uri.IsWellFormedUriString(Campeonato.BannerUrl, UriKind.Absolute))
                        BannerSource = ImageSource.FromUri(new Uri(Campeonato.BannerUrl));
                    else
                        BannerSource = ImageSource.FromFile("default_banner.png");
                } else {
                    BannerSource = ImageSource.FromFile("default_banner.png");
                }

                if (!string.IsNullOrEmpty(Campeonato?.LogoUrl)) {
                    if (File.Exists(Campeonato.LogoUrl))
                        LogoSource = ImageSource.FromFile(Campeonato.LogoUrl);
                    else if (Uri.IsWellFormedUriString(Campeonato.LogoUrl, UriKind.Absolute))
                        LogoSource = ImageSource.FromUri(new Uri(Campeonato.LogoUrl));
                    else
                        LogoSource = ImageSource.FromFile("default_logo.png");
                } else {
                    LogoSource = ImageSource.FromFile("default_logo.png");
                }
            } catch (Exception ex) {
                Debug.WriteLine($"[ERRO-IMAGEM-ISOLADO] Falha ao carregar imagens: {ex.Message}");
                BannerSource = ImageSource.FromFile("default_banner.png");
                LogoSource = ImageSource.FromFile("default_logo.png");
            }
        }

        private async Task LoadTabelaClassificacaoAsync() {
            if (Campeonato is null) return;

            // Limpeza inicial (manter do código atual)
            _timesPorGrupo.Clear();
            GruposDisponiveis.Clear();

            // 1. Obter Times
            var todosOsTimes = await _databaseService.ObterTimesAceitosAsync(Campeonato.Id) ?? new List<Time>();

            if (!todosOsTimes.Any()) {
                MainThread.BeginInvokeOnMainThread(() => {
                    TabelaClassificacao.Clear();
                    EstatisticasTimes.Clear(); // Limpa a nova lista também
                    IsFiltroGrupoVisivel = false;
                });
                return;
            }

            // 2. Obter Jogos e Estatísticas
            _todosOsJogosDoCampeonato = await _databaseService.ObterJogosPorCampeonatoAsync(Campeonato.ClientAppId);

            // CRÍTICO: Recalcular as estatísticas totais (Pontos, Vitórias, etc.)
            await RecalcularEstatisticasDosTimesAsync(todosOsTimes);

            // ** NOVO: 3. Calcular a sequência V/D/E/-
            CalcularSequenciaDeJogos(todosOsTimes, _todosOsJogosDoCampeonato);

            // ** NOVO: 4. Calcular estatísticas de agregação (média de pontos, rebotes, etc.)
            // Chamada do novo método
            await CalcularEstatisticasGeraisAsync(todosOsTimes, _todosOsJogosDoCampeonato);

            // 5. Processamento de Grupos e Visualização
            if (IsFormatoComGrupos) {

                // --- Lógica de atribuição/organização de grupos (mantida do código original) ---
                int numTimes = todosOsTimes.Count;
                int numGruposNecessarios = 1;

                // Lógica de atribuição de grupos mantida (simplificada)
                if (numTimes > 0) {
                    if (numTimes % 3 == 0 && numTimes / 3 >= 1)
                        numGruposNecessarios = numTimes / 3;
                    else if (numTimes % 4 == 0 && numTimes / 4 >= 1)
                        numGruposNecessarios = numTimes / 4;
                    else if (numTimes % 2 == 0 && numTimes / 2 >= 1)
                        numGruposNecessarios = numTimes / 2;
                    else if (numTimes >= 2) {
                        for (int i = (int)Math.Sqrt(numTimes); i >= 1; i--) {
                            if (numTimes % i == 0) {
                                int divisor1 = i;
                                int divisor2 = numTimes / i;
                                if (divisor1 >= 2 && divisor2 >= 2) {
                                    numGruposNecessarios = Math.Max(divisor1, divisor2);
                                    break;
                                } else if (divisor1 >= 2) {
                                    numGruposNecessarios = numTimes / divisor1;
                                    break;
                                } else if (divisor2 >= 2) {
                                    numGruposNecessarios = numTimes / divisor2;
                                    break;
                                }
                            }
                        }

                        if (numGruposNecessarios == 1 && numTimes > 0)
                            numGruposNecessarios = numTimes;
                    } else if (numTimes == 1) {
                        numGruposNecessarios = 1;
                    }
                }

                // Garante que todos os times tenham um grupo (mock)
                for (int i = 0; i < numTimes; i++) {
                    var time = todosOsTimes[i];
                    if (string.IsNullOrEmpty(time.Grupo)) {
                        int grupoIndex = i % numGruposNecessarios;
                        time.Grupo = $"Grupo {((char)('A' + grupoIndex)).ToString()}";
                    }
                }

                var grupos = todosOsTimes
                    .Where(t => !string.IsNullOrEmpty(t.Grupo))
                    .GroupBy(t => t.Grupo)
                    .OrderBy(g => g.Key);
                // --- Fim da Lógica de grupos ---

                MainThread.BeginInvokeOnMainThread(() => {
                    foreach (var group in grupos) {
                        GruposDisponiveis.Add(group.Key);
                        // Adiciona a lista de times com estatísticas JÁ ATUALIZADAS
                        _timesPorGrupo[group.Key] = group.ToList();
                    }

                    if (GruposDisponiveis.Any()) {
                        if (string.IsNullOrEmpty(GrupoSelecionado) || !GruposDisponiveis.Contains(GrupoSelecionado))
                            GrupoSelecionado = GruposDisponiveis.First();

                        IsFiltroGrupoVisivel = IsFaseTabelaEJogos;

                        // Chama LoadTabelaClassificacaoPorGrupo para processar a tabela do grupo
                        // (Este método deve ordenar e processar a lista _timesPorGrupo[GrupoSelecionado]
                        // que já contém as estatísticas recalculadas)
                        LoadTabelaClassificacaoPorGrupo(GrupoSelecionado);

                    } else {
                        TabelaClassificacao.Clear();
                        IsFiltroGrupoVisivel = false;
                    }
                });

            } else {
                // Sem grupos - Os objetos Time já contêm as estatísticas recalculadas
                var timesOrdenados = todosOsTimes
                    // ** ALTERADO: Prioriza Vitórias, depois PorcentagemVitoria
                    .OrderByDescending(t => t.Vitorias)
                    .ThenByDescending(t => t.PorcentagemVitoria)
                    // ... (outros critérios de desempate, se houver)
                    .ToList();

                MainThread.BeginInvokeOnMainThread(() => {
                    TabelaClassificacao.Clear();
                    IsFiltroGrupoVisivel = false;

                    for (int i = 0; i < timesOrdenados.Count; i++) {
                        var time = timesOrdenados[i];
                        time.Posicao = i + 1;
                        int totalJogosJogados = time.Vitorias + time.Derrotas + time.Empates; // Corrigido para incluir Empates
                        // ** Proteção 2.A: Garantida a divisão segura
                        time.PorcentagemVitoria = (totalJogosJogados > 0) ? (double)time.Vitorias / totalJogosJogados : 0.0;
                        time.JogosAtras = 0; // Valor a ser recalculado se necessário ou mantido 0
                        // time.Sequencia e os SequenciaChar já foram calculados no CalcularSequenciaDeJogos(todosOsTimes)

                        TabelaClassificacao.Add(time);
                    }

                    Debug.WriteLine($"[DEBUG-LOAD] Tabela de Classificação recarregada (Geral). Total: {TabelaClassificacao.Count}");
                });
            }
        }

        private async Task CalcularEstatisticasGeraisAsync(List<Time> times, List<Jogo> todosOsJogosDoCampeonato) {

            var listaEstatisticas = new List<TimeEstatisticaViewModel>();

            // 1. Obter todas as estatísticas de partida para o campeonato
            // (Assumindo que o GetEstatisticasByCampeonatoIdAsync existe no DatabaseService)
            var todasAsEstatisticasDoCampeonato = await _databaseService.GetEstatisticasByCampeonatoIdAsync(Campeonato.Id);

            // 2. Filtrar apenas jogos que foram FINALIZADOS para contar os "Jogos Disputados"
            var jogosFinalizados = todosOsJogosDoCampeonato
                .Where(j => j.PlacarTimeAInt >= 0 && j.PlacarTimeBInt >= 0)
                .ToList();

            // 3. Processar cada time
            foreach (var time in times) {
                var statsViewModel = new TimeEstatisticaViewModel(time);

                // A. Calcular Jogos Disputados (necessário para a média)
                statsViewModel.JogosDisputados = jogosFinalizados
                    .Count(j => j.TimeAId == time.Id || j.TimeBId == time.Id);

                // B. Somar as Estatísticas por Jogador para este Time
                var estatisticasDoTime = todasAsEstatisticasDoCampeonato
                    .Where(e => e.TimeId == time.Id)
                    .ToList();

                // C. Agregação - COMPLETANDO OS DEMAIS CAMPOS
                statsViewModel.TotalPontos = estatisticasDoTime.Sum(e => e.Pontos);
                statsViewModel.TotalRebotes = estatisticasDoTime.Sum(e => e.Rebotes);
                statsViewModel.TotalAssistencias = estatisticasDoTime.Sum(e => e.Assistencias);
                statsViewModel.TotalRoubos = estatisticasDoTime.Sum(e => e.Roubos);
                statsViewModel.TotalBloqueios = estatisticasDoTime.Sum(e => e.Bloqueios);
                statsViewModel.TotalTurnovers = estatisticasDoTime.Sum(e => e.Turnovers);
                statsViewModel.TotalFaltas = estatisticasDoTime.Sum(e => e.Faltas);

                // Agregação de Arremessos 2 Pontos
                statsViewModel.TotalArremessos2PontosConvertidos = estatisticasDoTime.Sum(e => e.Arremessos2PontosConvertidos);
                statsViewModel.TotalArremessos2PontosTentados = estatisticasDoTime.Sum(e => e.Arremessos2PontosTentados);

                // Agregação de Arremessos 3 Pontos
                statsViewModel.TotalArremessos3PontosConvertidos = estatisticasDoTime.Sum(e => e.Arremessos3PontosConvertidos);
                statsViewModel.TotalArremessos3PontosTentados = estatisticasDoTime.Sum(e => e.Arremessos3PontosTentados);

                // Agregação de Lances Livres
                statsViewModel.TotalLancesLivresConvertidos = estatisticasDoTime.Sum(e => e.LancesLivresConvertidos);
                statsViewModel.TotalLancesLivresTentados = estatisticasDoTime.Sum(e => e.LancesLivresTentados);

                listaEstatisticas.Add(statsViewModel);
            }

            // 4. Ordenar a lista pela métrica principal (MediaPontos)
            // A ordenação é feita pela propriedade calculada MediaPontos
            EstatisticasTimes = new ObservableCollection<TimeEstatisticaViewModel>(
                listaEstatisticas.OrderByDescending(t => t.MediaPontos)
            );
        }

        private void CalcularSequenciaDeJogos(List<Time> times, List<Jogo> todosOsJogosDoCampeonato) {
            var jogosFinalizados = todosOsJogosDoCampeonato
                .Where(j => j.Status == JogoStatus.Finalizado)
                .OrderByDescending(j => j.DataHora) 
                .ToList();

            foreach (var time in times) {
                var jogosDoTime = jogosFinalizados
                    .Where(j => j.TimeAId == time.Id || j.TimeBId == time.Id)
                    .Take(5)
                    .ToList();

                var sequencia = new StringBuilder();

                foreach (var jogo in jogosDoTime.AsEnumerable().Reverse()) {
                    char resultado = '-'; 

                    if (jogo.TimeAId == time.Id) {
                        if (jogo.PlacarTimeAInt > jogo.PlacarTimeBInt)
                            resultado = 'V';
                        else if (jogo.PlacarTimeAInt < jogo.PlacarTimeBInt)
                            resultado = 'D';
                        else
                            resultado = 'E'; 
                    } else { 
                        if (jogo.PlacarTimeBInt > jogo.PlacarTimeAInt)
                            resultado = 'V';
                        else if (jogo.PlacarTimeBInt < jogo.PlacarTimeAInt)
                            resultado = 'D';
                        else
                            resultado = 'E'; 
                    }

                    sequencia.Append(resultado); 
                }

                while (sequencia.Length < 5) {
                    sequencia.Insert(0, '-'); 
                }

                string seqFinal = sequencia.ToString();

                time.SequenciaChar1 = seqFinal[0].ToString();
                time.SequenciaChar2 = seqFinal[1].ToString();
                time.SequenciaChar3 = seqFinal[2].ToString();
                time.SequenciaChar4 = seqFinal[3].ToString();
                time.SequenciaChar5 = seqFinal[4].ToString();
            }
        }

        private async Task RecalcularEstatisticasDosTimesAsync(List<Time> times) {
            if (Campeonato is null) return;

            // 1. Obter todos os jogos do campeonato usando CampeonatoClientAppId (Guid)
            // Já carregado em _todosOsJogosDoCampeonato no LoadTabelaClassificacaoAsync
            var todosOsJogos = _todosOsJogosDoCampeonato;

            // 2. Inicializar as estatísticas de todos os times para zero
            foreach (var time in times) {
                time.Vitorias = 0;
                time.Derrotas = 0;
                time.Empates = 0;
            }

            // Usar um Dictionary para fácil acesso, usando o ID inteiro (Time.Id)
            var timesMap = times.ToDictionary(t => t.Id);

            // 3. Processar cada jogo finalizado
            foreach (var jogo in todosOsJogos) {

                bool placarAZero = jogo.PlacarTimeAInt == 0;
                bool placarBZero = jogo.PlacarTimeBInt == 0;

                if (jogo.PlacarTimeAInt >= 0 && jogo.PlacarTimeBInt >= 0 && !(placarAZero && placarBZero)) {

                    if (timesMap.TryGetValue(jogo.TimeAId, out var timeA) &&
                        timesMap.TryGetValue(jogo.TimeBId, out var timeB)) {

                        if (jogo.PlacarTimeAInt > jogo.PlacarTimeBInt) {
                            // Time A Venceu
                            timeA.Vitorias++;
                            timeB.Derrotas++;
                            timeA.PontuacaoTotal += 1;
                        } else if (jogo.PlacarTimeBInt > jogo.PlacarTimeAInt) {
                            // Time B Venceu
                            timeB.Vitorias++;
                            timeA.Derrotas++;
                            timeB.PontuacaoTotal += 1;
                        }
                    }
                }
            }

            foreach (var time in times) {
                Debug.WriteLine($"[STATS-DEBUG] Time: {time.Nome} | V: {time.Vitorias} | D: {time.Derrotas} | E: {time.Empates}");
            }

        }

        private void LoadTabelaClassificacaoPorGrupo(string grupo) {
            if (_timesPorGrupo.TryGetValue(grupo, out var timesDoGrupo)) {
                var timesOrdenados = timesDoGrupo
                    // ** ALTERADO: Prioriza Vitórias, depois PorcentagemVitoria
                    .OrderByDescending(t => t.Vitorias)
                    .ThenByDescending(t => t.PorcentagemVitoria)
                    .ToList();
                MainThread.BeginInvokeOnMainThread(() => {
                    TabelaClassificacao.Clear();

                    for (int i = 0; i < timesOrdenados.Count; i++) {
                        var time = timesOrdenados[i];
                        time.Posicao = i + 1;
                        int totalJogosJogados = time.Vitorias + time.Derrotas + time.Empates; // Corrigido para incluir Empates
                        // ** Proteção 2.A: Garantida a divisão segura
                        time.PorcentagemVitoria = (totalJogosJogados > 0) ? (double)time.Vitorias / totalJogosJogados : 0.0;
                        time.JogosAtras = 0;
                        // time.Sequencia e os SequenciaChar já foram calculados no CalcularSequenciaDeJogos(todosOsTimes)

                        TabelaClassificacao.Add(time);
                    }

                    Debug.WriteLine($"[DEBUG-LOAD] Tabela de Classificação recarregada (Grupo {grupo}). Total: {TabelaClassificacao.Count}");
                });
            }
        }

        private async Task GerarTabelaJogosAsync(Campeonato campeonato) {
            Debug.WriteLine("[DEBUG-JOGOS] Iniciando GerarTabelaJogosAsync.");
            _jogosPorRodada.Clear();

            var times = TabelaClassificacao.ToList();

            var jogosGeradosPorRodada = await _jogoService.GerarTabelaJogosAsync(campeonato, times);
            if (jogosGeradosPorRodada == null) jogosGeradosPorRodada = new Dictionary<int, ObservableCollection<Jogo>>();

            var todosOsArbitrosIds = jogosGeradosPorRodada.Values
                .SelectMany(col => col.Select(j => j.ArbitroId))
                .Where(id => id.HasValue && id.Value != Guid.Empty)
                .Select(id => id.Value)
                .Distinct()
                .ToList();

            var arbitrosMap = await _usuarioService.ObterNomesUsuariosPorIdsAsync(todosOsArbitrosIds);
            if (arbitrosMap == null) arbitrosMap = new Dictionary<Guid, string>();

            bool isOrganizador = this.IsOrganizador;
            foreach (var rodadaEntry in jogosGeradosPorRodada) {
                var rodadaJogos = rodadaEntry.Value;
                foreach (var jogo in rodadaJogos) {
                    jogo.IsOrganizador = isOrganizador;

                    if (jogo.ArbitroId.HasValue && jogo.ArbitroId.Value != Guid.Empty && arbitrosMap.TryGetValue(jogo.ArbitroId.Value, out var nome))
                        jogo.NomeArbitro = nome;
                    else
                        jogo.NomeArbitro = string.Empty;
                    jogo.NotifyArbitroStatusChanged();
                }

                _jogosPorRodada[rodadaEntry.Key] = rodadaJogos;
            }

            Debug.WriteLine($"[DEBUG-JOGOS] Tabela de Jogos recarregada. Total de Rodadas: {_jogosPorRodada.Count}");
        }

        private void LoadRodada(int rodada) {
            Debug.WriteLine($"[DEBUG-LOADRODADA] Iniciando LoadRodada({rodada}).");
            if (_jogosPorRodada.ContainsKey(rodada)) {
                var jogosDaRodada = _jogosPorRodada[rodada];
                MainThread.BeginInvokeOnMainThread(() => {
                    TabelaJogos.Clear();
                    foreach (var jogo in jogosDaRodada)
                        TabelaJogos.Add(jogo);

                    Debug.WriteLine($"[DEBUG-LOADRODADA] Rodada {rodada} carregada com {TabelaJogos.Count} jogos.");
                });
            } else {
                MainThread.BeginInvokeOnMainThread(() => TabelaJogos.Clear());
                Debug.WriteLine($"[DEBUG-LOADRODADA] Rodada {rodada} não encontrada. TabelaJogos limpa.");
            }
        }

        private async Task LoadPatrocinadoresAsync() {
            // ⭐️ LOG 1: Verifica o estado inicial (Campeonato) ⭐️
            if (Campeonato == null) {
                System.Diagnostics.Debug.WriteLine("[LoadPatrocinadoresAsync] Campeonato é nulo. Abortando.");
                return;
            }

            try {
                System.Diagnostics.Debug.WriteLine($"[LoadPatrocinadoresAsync] Buscando Campanha para Campeonato ID: {Campeonato.ClientAppId}");

                // Assume-se que o ViewModel tem acesso ao _patrocinioService.
                // O service já deve aplicar a lógica de inativação por data.
                var campanhaAtiva = await _patrocinioService.ObterCampanhaDeDivulgacaoAtivaAsync(Campeonato.ClientAppId);

                // ⭐️ CORREÇÃO ESSENCIAL: Salvar a Campanha Ativa na variável usada pelo Comando de Edição ⭐️
                // (A variável _propostaPatrocinioPrincipal é do tipo CampanhaPatrocinio, pois foi criada a partir de uma Proposta)
                _campanhaPatrocinioAtiva = campanhaAtiva;

                // ⭐️ LOG 2: Registra o valor retornado pelo Service (PONTO CRÍTICO) ⭐️
                System.Diagnostics.Debug.WriteLine($"[LoadPatrocinadoresAsync] Campanha recebida do Service: {(campanhaAtiva != null ? $"ID {campanhaAtiva.Id} | Fim: {campanhaAtiva.Fim}" : "NULA")}");

                PatrocinadoresAtivos.Clear();

                // Adiciona apenas se uma CampanhaPatrocinio não expirada foi encontrada (assumindo que o service cuida da expiração)
                if (campanhaAtiva != null) {
                    PatrocinadoresAtivos.Add(campanhaAtiva);
                }

                // PASSO CRÍTICO: Define a visibilidade da seção com base se a lista tem itens
                IsPatrocinioDivulgacaoVisible = PatrocinadoresAtivos.Any();

                // ⭐️ LOG 3: Registra a decisão final de visibilidade ⭐️
                System.Diagnostics.Debug.WriteLine($"[LoadPatrocinadoresAsync] IsPatrocinioDivulgacaoVisible: {IsPatrocinioDivulgacaoVisible}");


                if (IsPatrocinioDivulgacaoVisible) {
                    // O objeto CampanhaPatrocinio deve ter a propriedade ImagemPatrocinador
                    string? caminhoBanner = PatrocinadoresAtivos.First().ImagemPatrocinador;

                    // ⭐️ LOG 4: Registra o caminho do banner antes da atribuição ⭐️
                    System.Diagnostics.Debug.WriteLine($"[LoadPatrocinadoresAsync] Caminho Banner (ImagemPatrocinador): {caminhoBanner ?? "NULO/VAZIO"}");

                    // Se tem patrocinador, mas não tem imagem customizada, usa o placeholder
                    if (string.IsNullOrEmpty(caminhoBanner)) {
                        BannerDivulgacaoSource = "placeholder.png";
                    } else {
                        // Usa o caminho salvo
                        BannerDivulgacaoSource = caminhoBanner;
                    }

                    // ⭐️ LOG 5: Registra a Source final do Banner ⭐️
                    System.Diagnostics.Debug.WriteLine($"[LoadPatrocinadoresAsync] BannerDivulgacaoSource DEFINIDA para: {BannerDivulgacaoSource}");

                } else {
                    // Se não tem patrocinador (IsPatrocinioDivulgacaoVisible = false), limpa a Source.
                    BannerDivulgacaoSource = null;
                    System.Diagnostics.Debug.WriteLine("[LoadPatrocinadoresAsync] BannerDivulgacaoSource limpa.");
                }

            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"[PATROCINIO - CRÍTICO] Erro ao carregar patrocinadores: {ex.Message}");
                IsPatrocinioDivulgacaoVisible = false; // Garante que a seção não aparece em caso de erro
                BannerDivulgacaoSource = null;
            }
        }

        // NOVO: Método para carregar as estatísticas dos jogadores
        private async Task LoadLideresEstatisticasAsync() {
            Debug.WriteLine($"[DEBUG-STATS] ⭐️ Iniciando LoadLideresEstatisticasAsync ⭐️");
            if (Campeonato == null) {
                Debug.WriteLine("[DEBUG-STATS] Campeonato nulo. Abortando carregamento.");
                return;
            }

            try {
                // 1️⃣ OBTENÇÃO DOS TIMES
                Debug.WriteLine($"[DEBUG-STATS-STEP] Buscando times aceitos...");
                var todosOsTimes = await _databaseService.ObterTimesAceitosAsync(Campeonato.Id);
                Debug.WriteLine($"[DEBUG-STATS-STEP] Total de times obtidos: {todosOsTimes.Count}");

                // 2️⃣ CRIAÇÃO DO MAPA DE TIMES
                var timesMap = new Dictionary<Guid, Time>();
                foreach (var t in todosOsTimes) {
                    if (!timesMap.ContainsKey(t.ClientAppId))
                        timesMap[t.ClientAppId] = t;
                    else
                        Debug.WriteLine($"[WARN-STATS] Time duplicado ignorado: {t.Nome} ({t.ClientAppId})");
                }

                // 3️⃣ OBTENÇÃO DOS JOGADORES
                Debug.WriteLine($"[DEBUG-STATS-STEP] Buscando jogadores do campeonato...");
                var todosOsJogadores = await _databaseService.ObterJogadoresPorCampeonatoAsync(Campeonato.ClientAppId);
                Debug.WriteLine($"[DEBUG-STATS-STEP] Total de jogadores obtidos: {todosOsJogadores.Count}");

                if (!todosOsJogadores.Any()) {
                    Debug.WriteLine("[DEBUG-STATS] Nenhum jogador encontrado. Limpando lista.");
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        LideresEstatisticas.Clear();
                    });
                    return;
                }

                // 4️⃣ OBTENÇÃO DAS ESTATÍSTICAS AGREGADAS
                Debug.WriteLine($"[DEBUG-STATS-STEP] Buscando estatísticas agregadas...");
                var todasAsEstatisticasAgregadas = await _databaseService.GetEstatisticasDeJogadorByCampeonatoIdAsync(Campeonato.Id);
                Debug.WriteLine($"[DEBUG-STATS-STEP] Total de estatísticas agregadas obtidas: {todasAsEstatisticasAgregadas.Count}");

                // 5️⃣ CRIAÇÃO DO MAPA DE ESTATÍSTICAS
                var statsMap = new Dictionary<int, EstatisticaAgregadaJogador>();
                foreach (var e in todasAsEstatisticasAgregadas) {
                    if (!statsMap.ContainsKey(e.UsuarioId))
                        statsMap[e.UsuarioId] = e;
                    else
                        Debug.WriteLine($"[WARN-STATS] Estatística duplicada ignorada para Usuário ID: {e.UsuarioId}");
                }

                // 6️⃣ PROCESSAMENTO DOS JOGADORES
                var listaTempLideres = new List<JogadorEstatisticaViewModel>();

                foreach (var jogador in todosOsJogadores) {
                    // Lookup do Time
                    Time? timeDoJogador = null;
                    if (jogador.TimeClientAppId.HasValue)
                        timesMap.TryGetValue(jogador.TimeClientAppId.Value, out timeDoJogador);

                    // Lookup das Estatísticas Agregadas
                    statsMap.TryGetValue(jogador.Id, out var statsAgregadas);

                    // Criação do ViewModel do jogador
                    var statsJogador = new JogadorEstatisticaViewModel {
                        Id = jogador.Id,
                        NomeJogador = jogador.Nome,
                        FotoUrl = jogador.ImagemPath,
                        NomeTime = timeDoJogador?.Nome,
                        LogoTimeUrl = timeDoJogador?.LogoUrl
                    };

                    // Atribuição das Estatísticas Agregadas
                    if (statsAgregadas != null) {
                        Debug.WriteLine($"[DEBUG-STATS] Jogador: {jogador.Nome}, Pontos: {statsAgregadas.TotalPontos}");
                        statsJogador.Pontos = (int)statsAgregadas.TotalPontos;
                        statsJogador.Assistencias = (int)statsAgregadas.TotalAssistencias;
                        statsJogador.Rebotes = (int)statsAgregadas.TotalRebotes;
                        statsJogador.Roubos = (int)statsAgregadas.TotalRoubos;
                        statsJogador.Bloqueios = (int)statsAgregadas.TotalBloqueios;
                        statsJogador.Turnovers = (int)statsAgregadas.TotalTurnovers;
                        statsJogador.Faltas = (int)statsAgregadas.TotalFaltas;
                        statsJogador.Arremessos2PontosConvertidos = (int)statsAgregadas.Arremessos2PontosConvertidos;
                        statsJogador.Arremessos2PontosTentados = (int)statsAgregadas.Arremessos2PontosTentados;
                        statsJogador.Arremessos3PontosConvertidos = (int)statsAgregadas.Arremessos3PontosConvertidos;
                        statsJogador.Arremessos3PontosTentados = (int)statsAgregadas.Arremessos3PontosTentados;
                        statsJogador.LancesLivresConvertidos = (int)statsAgregadas.LancesLivresConvertidos;
                        statsJogador.LancesLivresTentados = (int)statsAgregadas.LancesLivresTentados;
                    } else {
                        Debug.WriteLine($"[DEBUG-STATS] Jogador: {jogador.Nome} - sem estatísticas registradas.");
                    }

                    listaTempLideres.Add(statsJogador);
                }

                // 7️⃣ ATUALIZAÇÃO NA MAIN THREAD
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LideresEstatisticas.Clear();
                    foreach (var item in listaTempLideres)
                        LideresEstatisticas.Add(item);

                    OnPropertyChanged(nameof(ItensEstatisticas));

                    Debug.WriteLine($"[DEBUG-OUTPUT] ItensEstatisticas Count: {ItensEstatisticas?.Count ?? 0}");
                    Debug.WriteLine($"[DEBUG-OUTPUT] Primeiro item: Nome={ItensEstatisticas.FirstOrDefault()?.NomeJogador}, Valor={ItensEstatisticas.FirstOrDefault()?.ValorEstatisticaPrincipal}");
                    Debug.WriteLine($"[DEBUG-LEADERS] Total de líderes carregados: {LideresEstatisticas.Count}");
                });
            } catch (Exception ex) {
                Debug.WriteLine($"[FATAL CRASH] Ocorreu uma exceção crítica em LoadLideresEstatisticasAsync: {ex.Message}");
                Debug.WriteLine($"[FATAL CRASH] StackTrace: {ex.StackTrace}");
            }
        }

        // NOVO: Método auxiliar para ordenar e filtrar os itens de estatísticas
        private ObservableCollection<JogadorEstatisticaViewModel> GetItensEstatisticasOrdenados(string estatistica) {
            IEnumerable<JogadorEstatisticaViewModel> orderedList = LideresEstatisticas;

            // A. LÓGICA DE ORDENAÇÃO (SUA VERSÃO ORIGINAL)
            switch (estatistica) {
                case "Pontos":
                    orderedList = orderedList.OrderByDescending(j => j.Pontos);
                    break;
                case "Assistências":
                    orderedList = orderedList.OrderByDescending(j => j.Assistencias);
                    break;
                case "Rebotes":
                    orderedList = orderedList.OrderByDescending(j => j.Rebotes);
                    break;
                case "Roubos":
                    orderedList = orderedList.OrderByDescending(j => j.Roubos);
                    break;
                case "Bloqueios":
                    orderedList = orderedList.OrderByDescending(j => j.Bloqueios);
                    break;
                case "Turnovers":
                    orderedList = orderedList.OrderBy(j => j.Turnovers); // Menor é melhor
                    break;
                case "Faltas":
                    orderedList = orderedList.OrderBy(j => j.Faltas); // Menor é melhor
                    break;
                case "2 Pontos %":
                    orderedList = orderedList.OrderByDescending(j => j.Percentual2Pontos);
                    break;
                case "3 Pontos %":
                    orderedList = orderedList.OrderByDescending(j => j.Percentual3Pontos);
                    break;
                case "Lance Livre %":
                    orderedList = orderedList.OrderByDescending(j => j.PercentualLancesLivres);
                    break;
                default:
                    orderedList = orderedList.OrderByDescending(j => j.Pontos);
                    break;
            }

            // B. FILTRO TOP 5 E ATRIBUIÇÃO DE RANK

            // ⭐️ CORREÇÃO PRINCIPAL: Aplica o filtro TOP 5 após a ordenação
            var top5List = orderedList.Take(5);

            // Atribuir posição (rank) e formatar o ValorEstatisticaPrincipal
            var rankedList = new List<JogadorEstatisticaViewModel>();
            int rank = 1;

            // Itera apenas sobre os 5 melhores
            foreach (var item in top5List) {
                // 1. Atribui a posição (1º, 2º, etc.)
                item.Posicao = rank++;

                // 2. Formata o valor de exibição (ex: 50 ou 45%)
                item.ValorEstatisticaPrincipal = GetFormattedEstatisticaValue(item, estatistica);

                rankedList.Add(item);
            }

            return new ObservableCollection<JogadorEstatisticaViewModel>(rankedList);
        }

        // NOVO: Método auxiliar para formatar o valor da estatística
        private string GetFormattedEstatisticaValue(JogadorEstatisticaViewModel jogadorStats, string estatistica) {
            switch (estatistica) {
                case "Pontos": return jogadorStats.Pontos.ToString();
                case "Assistências": return jogadorStats.Assistencias.ToString();
                case "Rebotes": return jogadorStats.Rebotes.ToString();
                case "Roubos": return jogadorStats.Roubos.ToString();
                case "Bloqueios": return jogadorStats.Bloqueios.ToString();
                case "Turnovers": return jogadorStats.Turnovers.ToString();
                case "Faltas": return jogadorStats.Faltas.ToString();
                case "2 Pontos %": return $"{jogadorStats.Percentual2Pontos:P0}"; // Formato percentual
                case "3 Pontos %": return $"{jogadorStats.Percentual3Pontos:P0}"; // Formato percentual
                case "Lance Livre %": return $"{jogadorStats.PercentualLancesLivres:P0}"; // Formato percentual
                default: return jogadorStats.Pontos.ToString();
            }
        }

        public void MudarEstatisticaLogic(string estatistica) {
            if (EstatisticaSelecionada == estatistica)
                return;

            EstatisticaSelecionada = estatistica; // ⭐️ Esta atualização já dispara a UI

            Debug.WriteLine($"[DEBUG-STATS-LOGIC] Nova Estatística selecionada (Code-Behind): {EstatisticaSelecionada}.");
        }

        // =====================================================================================
        // COMANDOS (RelayCommand)
        // =====================================================================================

        [RelayCommand]
        private async Task AlterarBanner() {
            Debug.WriteLine("[CampeonatoDetailViewModel] Botão 'Alterar Banner' clicado.");
            var popup = new AlterarBannerPopup(Campeonato, _alertService, _databaseService, _syncService);

            popup.BannerAtualizado += (s, newBannerPath) => {
                Debug.WriteLine($"[CampeonatoDetailViewModel] Evento BannerAtualizado recebido com caminho: '{newBannerPath}'");
                MainThread.BeginInvokeOnMainThread(() => {
                    if (!string.IsNullOrEmpty(newBannerPath)) {
                        if (File.Exists(newBannerPath)) {
                            BannerSource = ImageSource.FromFile(newBannerPath);
                        } else if (Uri.IsWellFormedUriString(newBannerPath, UriKind.Absolute)) {
                            BannerSource = ImageSource.FromUri(new Uri(newBannerPath));
                        } else {
                            Debug.WriteLine("[CampeonatoDetailViewModel] Caminho/URL do novo banner é inválida ou arquivo não encontrado.");
                        }
                    }
                });
            };

            await Application.Current.MainPage.Navigation.PushModalAsync(popup);
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

        [RelayCommand]
        public async Task AnexarArbitros(Jogo jogo) {
            if (!IsOrganizador) {
                await _alertService.DisplayAlert("Acesso Negado", "Somente o organizador pode anexar árbitros a um jogo.", "OK");
                return;
            }

            if (jogo is null) {
                await _alertService.DisplayAlert("Erro de Dados", "O jogo selecionado não pôde ser carregado.", "OK");
                return;
            }

            try {
                var navigationParameters = new ShellNavigationQueryParameters
                {
                    { "Campeonato", Campeonato },
                    { "Jogo", jogo }
                };
                await Shell.Current.GoToAsync("AtribuirArbitros", navigationParameters);
            } catch (Exception ex) {
                Debug.WriteLine($"[DEBUG-CLIQUE] ERRO CRÍTICO: {ex.Message}");
                await _alertService.DisplayAlert("Erro de Navegação", $"Não foi possível abrir a tela de árbitros: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task GerenciarSolicitacoes() {
            if (Campeonato is null) return;
            var navigationParameters = new ShellNavigationQueryParameters
            {
                { "Campeonato", Campeonato }
            };
            await Shell.Current.GoToAsync(nameof(GerenciarSolicitacoesPage), navigationParameters);
        }

        [RelayCommand]
        private async Task ListarTimesInscritos() {
            if (Campeonato is null) return;
            var navigationParameters = new ShellNavigationQueryParameters
            {
                // Passando Campeonato.Id (int)
                { "CampeonatoId", Campeonato.Id }
            };
            await Shell.Current.GoToAsync(nameof(TimesCadastradosPage), navigationParameters);
        }

        [RelayCommand]
        private async Task ListarArbitrosInscritos() {
            if (Campeonato is null) return;
            var navigationParameters = new ShellNavigationQueryParameters
            {
                // Passando Campeonato.ClientAppId (int)
                { "CampeonatoId", Campeonato.ClientAppId }
            };
            await Shell.Current.GoToAsync(nameof(ArbitrosInscritosPage), navigationParameters);
        }

        [RelayCommand]
        private async Task AlterarBannerDivulgacao() {
            Debug.WriteLine("[CampeonatoDetailViewModel] Botão 'Alterar Banner Divulgação' clicado.");

            // ⭐️ _propostaPatrocinioPrincipal agora contém a CampanhaPatrocinio ativa (ou null se não houver) ⭐️
            if (_campanhaPatrocinioAtiva == null) {
                // Se ainda for null, significa que LoadPatrocinadoresAsync não encontrou nada (expirou/erro real)
                await _alertService.DisplayAlert("Sem Patrocínio", "Não há patrocínios ativos para alterar o banner.", "OK");
                return;
            }

            // Passamos a Campanha Patrocínio ativa para o pop-up, que fará o upload e atualizará o ImagemPatrocinador.
            // É recomendável renomear a variável no ViewModel para _campanhaPatrocinioPrincipal no futuro para maior clareza.
            var popup = new AlterarBannerPatrocinioPopup(_campanhaPatrocinioAtiva, _alertService, _databaseService, _syncService);

            popup.BannerAtualizado += (s, newBannerPath) => {
                Debug.WriteLine($"[CampeonatoDetailViewModel] Evento BannerAtualizado (Patrocínio) recebido com caminho: '{newBannerPath}'");
                MainThread.BeginInvokeOnMainThread(() => {
                    BannerDivulgacaoSource = newBannerPath;

                    // Opcional, mas recomendado: Atualizar a variável de suporte para refletir o novo caminho imediatamente.
                    _campanhaPatrocinioAtiva.ImagemPatrocinador = newBannerPath;
                });
            };
            await Application.Current.MainPage.Navigation.PushModalAsync(popup);
        }

    }
}