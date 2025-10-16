using ArenaVirtual.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using ArenaVirtual.Services;
using System.Collections.ObjectModel;
using ArenaVirtual.Popups;
using ArenaVirtual.Views.CampeonatoPage;
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
    public class RodadaGrouping : Grouping<int, Jogo> {
        public RodadaGrouping(int key, IEnumerable<Jogo> items) : base(key, items) { }
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

        // NOVA PROPRIEDADE → Adicionada na primeira correção (mantida)
        [ObservableProperty]
        private bool isFiltroFaseVisivel = false;

        // Dicionários privados
        private readonly Dictionary<int, ObservableCollection<Jogo>> _jogosPorRodada = new();
        private readonly Dictionary<string, List<Time>> _timesPorGrupo = new();

        // ** ADICIONADO: Lista privada de todos os jogos para cálculo da sequência
        private List<Jogo> _todosOsJogosDoCampeonato = new();

        // Serviços injetados
        private readonly IAlertService _alertService;
        private readonly DatabaseService _databaseService;
        private readonly SyncService _syncService;
        private readonly UsuarioService _usuarioService;
        private readonly IJogoService _jogoService;

        // Propriedades calculadas
        public bool IsFormatoComGrupos =>
            Campeonato?.FormatoCampeonato?.IndexOf("Grupos", StringComparison.OrdinalIgnoreCase) >= 0 ||
            Campeonato?.FormatoCampeonato?.IndexOf("Fase de Grupos", StringComparison.OrdinalIgnoreCase) >= 0;

        public bool IsTabelaFormat =>
            Campeonato?.FormatoCampeonato?.IndexOf("Pontos", StringComparison.OrdinalIgnoreCase) >= 0 ||
            IsFormatoComGrupos;

        // CORREÇÃO: Removemos '|| IsFormatoHibrido' para que esta propriedade
        // descreva o *formato base* do campeonato, e não o estado da fase atual.
        public bool IsMataMataFormat =>
            Campeonato?.FormatoCampeonato?.IndexOf("Mata-mata", StringComparison.OrdinalIgnoreCase) >= 0 ||
            Campeonato?.FormatoCampeonato?.IndexOf("Eliminação", StringComparison.OrdinalIgnoreCase) >= 0;

        public bool IsFaseTabelaEJogos => FaseAtual == "Tabela & Jogos";
        public bool IsFaseMataMata => FaseAtual == "Mata-Mata";

        // Construtor
        public CampeonatoDetailViewModel(
            IAlertService alertService,
            DatabaseService databaseService,
            SyncService syncService,
            IJogoService jogoService,
            UsuarioService usuarioService) {
            TabelaClassificacao = new ObservableCollection<Time>();
            TabelaJogos = new ObservableCollection<Jogo>();

            _alertService = alertService;
            _databaseService = databaseService;
            _syncService = syncService;
            _jogoService = jogoService;
            _usuarioService = usuarioService;

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
                // A visibilidade do filtro de grupo só afeta a UI do filtro, não o conteúdo
                IsFiltroGrupoVisivel = IsFormatoComGrupos;
                await LoadTabelaClassificacaoAsync();
                LoadRodada(RodadaAtual);
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query) {
            Debug.WriteLine("[DEBUG-ATTRIBUTES] ApplyQueryAttributes chamado.");
            // 1. Atualização de Jogo (Árbitro) ou Jogo (Placar/Outros)
            if (query.TryGetValue("jogoAtualizado", out object jogoObj) && jogoObj is Jogo jogoAtualizado) {
                Debug.WriteLine($"[DEBUG-ATTRIBUTES] Jogo ID {jogoAtualizado.Id} foi atualizado.");
                query.Remove("jogoAtualizado");

                _ = LoadTabelaClassificacaoAsync();

                _ = RecarregarJogosESelecaoAsync();

                if (IsMataMataFormat || IsFormatoHibrido)
                    _ = GerarJogosMataMata();

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
            }
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

        private async Task GerarJogosMataMata() {
            if (!IsMataMataFormat && !IsFormatoHibrido || Campeonato is null) return;

            MainThread.BeginInvokeOnMainThread(() => {
                JogosMataMata.Clear();
            });

            // Usamos TabelaClassificacao (que já é carregada no LoadTabelaClassificacaoAsync)
            // para obter os times que avançaram (todos, neste mock)
            var timesAceitos = TabelaClassificacao.OrderByDescending(t => t.PontuacaoTotal).ToList();

            if (timesAceitos.Count < 2) {
                Debug.WriteLine("[MATA-MATA] Não há times suficientes para gerar o bracket.");
                return;
            }

            int mockIdCounter = -1;
            var mockJogosFlat = new List<Jogo>();

            // Lógica de Geração de Bracket (Mock) - Mantida a lógica original
            if (timesAceitos.Count == 2) {
                var jogoFinal = new Jogo {
                    Id = mockIdCounter--,
                    TimeA = timesAceitos[0],
                    TimeAId = timesAceitos[0].Id,
                    TimeB = timesAceitos[1],
                    TimeBId = timesAceitos[1].Id,
                    Rodada = 1,
                    IsOrganizador = IsOrganizador,
                    NomeArbitro = string.Empty,
                    Local = "A Definir",
                };
                mockJogosFlat.Add(jogoFinal);
                Debug.WriteLine("[MATA-MATA] Bracket de 2 times (Final) gerado.");

            } else if (timesAceitos.Count == 3) {
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

                var jogoFinal = new Jogo {
                    Id = mockIdCounter--,
                    TimeA = timesAceitos[0], // Time que ganhou o Bye
                    TimeAId = timesAceitos[0].Id,
                    TimeB = new Time { Nome = "Vencedor Jogo 1", LogoUrl = "default_logo.png", Id = mockIdCounter-- },
                    TimeBId = jogoSemi1.Id,
                    Rodada = 2,
                    IsOrganizador = IsOrganizador,
                    NomeArbitro = string.Empty,
                    Local = "A Definir",
                };
                mockJogosFlat.Add(jogoFinal);
                Debug.WriteLine("[MATA-MATA] Bracket de 3 times gerado (Semi + Final com Bye).");

            } else if (timesAceitos.Count >= 4) {
                // Primeira Rodada (Quartas, se houver 4)
                for (int i = 0; i < timesAceitos.Count; i += 2) {
                    if (i + 1 < timesAceitos.Count) {
                        mockJogosFlat.Add(new Jogo {
                            Id = mockIdCounter--,
                            TimeA = timesAceitos[i],
                            TimeAId = timesAceitos[i].Id,
                            TimeB = timesAceitos[i + 1],
                            TimeBId = timesAceitos[i + 1].Id,
                            Rodada = 1,
                            IsOrganizador = IsOrganizador,
                            NomeArbitro = string.Empty,
                            Local = "A Definir",
                        });
                    }
                }

                // Simulação da próxima rodada (Semi)
                if (mockJogosFlat.Count >= 2) {
                    var vencedoresMock = new List<Time>();
                    for (int i = 0; i < mockJogosFlat.Count; i += 2) {
                        var jogo1 = mockJogosFlat[i];
                        var jogo2 = i + 1 < mockJogosFlat.Count ? mockJogosFlat[i + 1] : null;

                        var vencedor1 = new Time { Nome = $"Vencedor Jogo {i + 1}", LogoUrl = "default_logo.png", Id = mockIdCounter-- };
                        var vencedor2 = jogo2 != null ? new Time { Nome = $"Vencedor Jogo {i + 2}", LogoUrl = "default_logo.png", Id = mockIdCounter-- } : null;

                        if (jogo2 != null) {
                            var jogoProximaRodada = new Jogo {
                                Id = mockIdCounter--,
                                TimeA = vencedor1,
                                TimeAId = jogo1.Id, // Referência ao Jogo anterior
                                TimeB = vencedor2,
                                TimeBId = jogo2.Id, // Referência ao Jogo anterior
                                Rodada = 2,
                                IsOrganizador = IsOrganizador,
                                NomeArbitro = string.Empty,
                                Local = "A Definir",
                            };
                            mockJogosFlat.Add(jogoProximaRodada);
                        }
                    }
                }

                Debug.WriteLine("[MATA-MATA] Bracket de 4+ times gerado (Rodadas simuladas).");
            }

            Debug.WriteLine($"[MATA-MATA] Total de jogos planos gerados: {mockJogosFlat.Count}");

            var groupedJogos = mockJogosFlat
                .OrderBy(j => j.Rodada)
                .GroupBy(j => j.Rodada)
                .Select(g => new RodadaGrouping(g.Key, g))
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

        public string ObterNomeDaRodada(int rodada, int totalJogos) {
            int divisor = (int)Math.Pow(2, rodada - 1);
            if (divisor == 0) return $"Rodada {rodada}";

            int jogosNaRodada = totalJogos / Math.Max(1, divisor);

            if (jogosNaRodada == 1) return "FINAL";
            if (jogosNaRodada == 2) return "Semifinal";
            if (jogosNaRodada == 4) return "Quartas de Final";
            if (jogosNaRodada == 8) return "Oitavas de Final";

            return $"Rodada {rodada}";
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
                    IsFiltroGrupoVisivel = false;
                });
                return;
            }

            // 2. Obter Jogos e Estatísticas
            // (Implementação crítica da nova abordagem)
            _todosOsJogosDoCampeonato = await _databaseService.ObterJogosPorCampeonatoAsync(Campeonato.ClientAppId);

            // CRÍTICO: Recalcular as estatísticas totais (Pontos, Vitórias, etc.)
            await RecalcularEstatisticasDosTimesAsync(todosOsTimes);

            // ** NOVO: 3. Calcular a sequência V/D/E/-
            CalcularSequenciaDeJogos(todosOsTimes, _todosOsJogosDoCampeonato);

            // 4. Processamento de Grupos e Visualização

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
                // Zera APENAS as propriedades que existem no Time.cs
                time.PontuacaoTotal = 0;
                time.Vitorias = 0;
                time.Derrotas = 0;
                time.Empates = 0;
                // Nota: O seu modelo Time não possui PontosFeitos/Sofridos.
            }

            // Usar um Dictionary para fácil acesso, usando o ID inteiro (Time.Id)
            var timesMap = times.ToDictionary(t => t.Id);

            // 3. Processar cada jogo finalizado
            foreach (var jogo in todosOsJogos) {
                // Só processa jogos com placar lançado (indicando que foi finalizado)
                if (jogo.PlacarTimeAInt >= 0 && jogo.PlacarTimeBInt >= 0) {

                    // CORREÇÃO: Tentando obter os times usando Jogo.TimeAId e Jogo.TimeBId (IDs inteiros)
                    if (timesMap.TryGetValue(jogo.TimeAId, out var timeA) &&
                        timesMap.TryGetValue(jogo.TimeBId, out var timeB)) {

                        if (jogo.PlacarTimeAInt > jogo.PlacarTimeBInt) {
                            // Time A Venceu
                            timeA.Vitorias++;
                            timeB.Derrotas++;
                            timeA.PontuacaoTotal += 3; // 3 pontos por vitória (mantido para a lógica de Pontos Ganhos)
                        } else if (jogo.PlacarTimeBInt > jogo.PlacarTimeAInt) {
                            // Time B Venceu
                            timeB.Vitorias++;
                            timeA.Derrotas++;
                            timeB.PontuacaoTotal += 3; // 3 pontos por vitória (mantido para a lógica de Pontos Ganhos)
                        } else if (jogo.PlacarTimeAInt == jogo.PlacarTimeBInt) {
                            // Empate 
                            timeA.Empates++;
                            timeB.Empates++;
                            timeA.PontuacaoTotal += 1; // 1 ponto por empate (mantido para a lógica de Pontos Ganhos)
                            timeB.PontuacaoTotal += 1;
                        }
                    }
                }
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
    }
}