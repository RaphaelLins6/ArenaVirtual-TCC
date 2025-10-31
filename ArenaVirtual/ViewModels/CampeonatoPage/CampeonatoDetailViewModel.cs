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

        [ObservableProperty]
        private Campeonato campeonato;

        [ObservableProperty]
        private ObservableCollection<Time> tabelaClassificacao;

        [ObservableProperty]
        private ObservableCollection<Jogo> tabelaJogos;

        [ObservableProperty]
        private ObservableCollection<TimeEstatisticaViewModel> estatisticasTimes;

        [ObservableProperty]
        private ObservableCollection<JogadorEstatisticaViewModel> lideresEstatisticas = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ItensEstatisticas))]
        private string estatisticaSelecionada = "Pontos"; 

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

        private readonly Dictionary<int, ObservableCollection<Jogo>> _jogosPorRodada = new();
        private readonly Dictionary<string, List<Time>> _timesPorGrupo = new();
        private CampanhaPatrocinio? _campanhaPatrocinioAtiva;
        private List<Jogo> _todosOsJogosDoCampeonato = new();

        private readonly IAlertService _alertService;
        private readonly DatabaseService _databaseService; // Usando a interface para ser mais coerente
        private readonly SyncService _syncService;
        private readonly UsuarioService _usuarioService;
        private readonly IJogoService _jogoService;
        private readonly PatrocinioService _patrocinioService;

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

        public ObservableCollection<JogadorEstatisticaViewModel> ItensEstatisticas =>
            GetItensEstatisticasOrdenados(EstatisticaSelecionada);

        public CampeonatoDetailViewModel(
            IAlertService alertService,
            DatabaseService databaseService, 
            SyncService syncService,
            IJogoService jogoService,
            UsuarioService usuarioService,
            PatrocinioService patrocinioService) {

            TabelaClassificacao = new ObservableCollection<Time>();
            TabelaJogos = new ObservableCollection<Jogo>();
            EstatisticasTimes = new ObservableCollection<TimeEstatisticaViewModel>();

            _alertService = alertService;
            _databaseService = (DatabaseService)databaseService; 
            _syncService = syncService;
            _jogoService = jogoService;
            _usuarioService = usuarioService;
            _patrocinioService = patrocinioService;

            IsDesktop = DeviceInfo.Idiom == DeviceIdiom.Desktop || DeviceInfo.Idiom == DeviceIdiom.Tablet;
            //Debug.WriteLine($"[CampeonatoDetailViewModel] Device Idiom: {DeviceInfo.Idiom}. IsDesktop: {IsDesktop}");
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
            //Debug.WriteLine($"[DEBUG-FASES] Nova fase selecionada: {newValue}");

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
            //Debug.WriteLine("[DEBUG-ATTRIBUTES] ApplyQueryAttributes chamado.");

            if (query.TryGetValue("jogoAtualizado", out object jogoObj) && jogoObj is Jogo jogoAtualizado) {
                //Debug.WriteLine($"[DEBUG-ATTRIBUTES] Jogo ID {jogoAtualizado.Id} foi atualizado.");
                query.Remove("jogoAtualizado");

                _ = LoadTabelaClassificacaoAsync();
                _ = RecarregarJogosESelecaoAsync();

                if (IsMataMataFormat || IsFormatoHibrido)
                    _ = GerarJogosMataMata();

                _ = LoadPatrocinadoresAsync();
                _ = LoadLideresEstatisticasAsync();

                //Debug.WriteLine($"[DEBUG-ATTRIBUTES] Recarga completa do Campeonato após atualização do Jogo ID {jogoAtualizado.Id}.");
                return;
            }

            if (query.ContainsKey("TimesAtualizados")) {
                //Debug.WriteLine("[DEBUG-ATTRIBUTES] Lista de Times foi atualizada. Recarregando Classificação e Jogos.");
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
                    //Debug.WriteLine("[DEBUG-ATTRIBUTES] ApplyQueryAttributes ignorou LoadCampeonato (Campeonato já carregado).");
                }

                if (Campeonato != null) {
                    _ = LoadPatrocinadoresAsync();
                    _ = LoadLideresEstatisticasAsync();
                }
            }
            query.Clear();
        }

        private async Task RecarregarJogosESelecaoAsync() {
            if (Campeonato == null || !IsTabelaFormat) return;
            //Debug.WriteLine("[DEBUG-RELOAD] Iniciando RecarregarJogosESelecaoAsync.");

            await GerarTabelaJogosAsync(Campeonato);
            //Debug.WriteLine($"[DEBUG-RELOAD] GerarTabelaJogosAsync concluído. RodadaAtual: {RodadaAtual}");
            if (RodadaAtual > 0) {
                MainThread.BeginInvokeOnMainThread(() => {
                    //Debug.WriteLine($"[DEBUG-RELOAD] Chamando LoadRodada({RodadaAtual}) na MainThread.");
                    LoadRodada(RodadaAtual);
                });
            }

            //Debug.WriteLine("[DEBUG-RELOAD] Finalizando RecarregarJogosESelecaoAsync.");
        }

        public async Task LoadCampeonato(Campeonato campeonato) {
            //Debug.WriteLine("[CampeonatoDetailViewModel] LoadCampeonato chamado.");
            if (IsBusy) return;

            try {
                IsBusy = true;
                if (campeonato == null) {
                    //Debug.WriteLine("[CampeonatoDetailViewModel] Campeonato é nulo, retornando.");
                    return;
                }

                Campeonato = campeonato;
                OnPropertyChanged(nameof(IsFormatoComGrupos));
                OnPropertyChanged(nameof(IsTabelaFormat));
                OnPropertyChanged(nameof(IsMataMataFormat));

                AtualizarFormatoCampeonato();

                var usuarioAtual = SessaoService.Instancia.GetUsuarioAtual();
                IsOrganizador = (campeonato.OrganizadorId == usuarioAtual?.Id);
                //Debug.WriteLine($"[CampeonatoDetailViewModel] É organizador? {IsOrganizador}. Formato Tabela: {IsTabelaFormat}, Mata-Mata: {IsMataMataFormat}, Híbrido: {IsFormatoHibrido}");

                await LoadTabelaClassificacaoAsync();

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
                //Debug.WriteLine($"[ERRO CRÍTICO] LoadCampeonato falhou: {ex.Message}");
                await _alertService.DisplayAlert("Erro de Carregamento", "Ocorreu um erro ao carregar os detalhes do campeonato. Tente novamente.", "OK");
            } finally {
                IsBusy = false;
            }
        }

        private void AtualizarFormatoCampeonato() {
            if (Campeonato is null) return;

            //Debug.WriteLine($"[DEBUG-FORMATO] Valor de FormatoCampeonato: '{Campeonato.FormatoCampeonato}'");

            bool isPontosMaisEliminatoria = Campeonato.FormatoCampeonato.IndexOf("Pontos + Eliminatórias", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isGruposMaisEliminatoria = Campeonato.FormatoCampeonato.IndexOf("Grupos + Eliminatórias", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isMaisEliminatoria = Campeonato.FormatoCampeonato.IndexOf("Mais Eliminatória", StringComparison.OrdinalIgnoreCase) >= 0;

            IsFormatoHibrido = isPontosMaisEliminatoria || isGruposMaisEliminatoria || isMaisEliminatoria;

            IsFiltroFaseVisivel = IsFormatoHibrido;

            //Debug.WriteLine($"[DEBUG-FORMATO] IsFormatoHibrido set to: {IsFormatoHibrido}. IsFiltroFaseVisivel: {IsFiltroFaseVisivel}");

            FasesDisponiveis.Clear(); // Limpar a lista para reconstruir

            if (IsFormatoHibrido) {
                FasesDisponiveis.Add("Tabela & Jogos");
                FasesDisponiveis.Add("Mata-Mata");
                FaseAtual = "Tabela & Jogos"; // Padrão
            } else {
                if (IsMataMataFormat)
                    FaseAtual = "Mata-Mata";
                else
                    FaseAtual = "Tabela & Jogos";

                FasesDisponiveis.Add(FaseAtual);
            }

            if (IsFaseTabelaEJogos) {
                IsFiltroGrupoVisivel = IsFormatoComGrupos && GruposDisponiveis.Any();
            } else {
                IsFiltroGrupoVisivel = false;
            }
        }

        private string GetNomeRodada(int rodadaAtual, int totalRodadas) {
            int rodadaContagemReversa = totalRodadas - rodadaAtual + 1;

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
                    return $"Rodada {rodadaAtual} (Preliminar)";
            }
        }


        private async Task GerarJogosMataMata() {
            if (!IsMataMataFormat && !IsFormatoHibrido || Campeonato is null) return;

            MainThread.BeginInvokeOnMainThread(() => {
                JogosMataMata.Clear();
            });

            var jogosMataMataSalvos = await _jogoService.ObterJogosMataMataPorCampeonatoAsync(Campeonato.ClientAppId);
            if (jogosMataMataSalvos == null) jogosMataMataSalvos = new List<Jogo>();

            // --------------------------------------------------------------------------
            // LOG 1: Verificar os jogos que vieram do banco (Persistência)
            // --------------------------------------------------------------------------
            Debug.WriteLine("[DEBUG-MATA-MATA] Iniciando GerarJogosMataMata. Jogos salvos carregados:");
            foreach (var j in jogosMataMataSalvos) {
                // Agora, todos os jogos com Id positivo DEVEM ter o ArbitroId persistido se o árbitro foi anexado.
                Debug.WriteLine($"[DEBUG-MATA-MATA - SALVO] Jogo ID: {j.Id}. Rodada: {j.Rodada}. ArbitroId: {j.ArbitroId}.");
            }
            // --------------------------------------------------------------------------

            // Carrega TODOS os IDs de árbitros de TODOS os jogos SALVOS
            var todosOsArbitrosIds = jogosMataMataSalvos
                .Select(j => j.ArbitroId)
                .Where(id => id.HasValue && id.Value != Guid.Empty)
                .Select(id => id.Value)
                .Distinct()
                .ToList();

            var arbitrosMap = await _usuarioService.ObterNomesUsuariosPorIdsAsync(todosOsArbitrosIds);
            if (arbitrosMap == null) arbitrosMap = new Dictionary<Guid, string>();

            // LOG 2: Verificar o mapa de nomes carregados
            Debug.WriteLine($"[DEBUG-MATA-MATA - MAP] Total de árbitros mapeados: {arbitrosMap.Count}");


            bool isOrganizador = this.IsOrganizador;
            foreach (var jogoSalvo in jogosMataMataSalvos) {
                jogoSalvo.IsOrganizador = isOrganizador;
            }


            var timesAceitos = TabelaClassificacao.OrderByDescending(t => t.PontuacaoTotal).ToList();

            if (timesAceitos.Count < 2) {
                return;
            }

            int mockIdCounter = -1; // Mantido apenas para IDs de Vencedores/Bye (placeholders)
            var mockJogosFlat = new List<Jogo>();
            int totalRodadas = 0;

            // *************************************************************
            // FUNÇÃO REFATORADA: Encontra ou Cria e PERSISTE o jogo no banco
            // *************************************************************
            async Task<Jogo> EncontrarOuCriarEPersistirJogo(Time tA, Time tB, int rodada) {

                // 1. Tenta encontrar um jogo existente no banco de dados
                var jogoExistente = jogosMataMataSalvos
                    .FirstOrDefault(j => j.Rodada == rodada &&
                                         ((j.TimeAId == tA.Id && j.TimeBId == tB.Id) ||
                                          (j.TimeAId == tB.Id && j.TimeBId == tA.Id)));

                if (jogoExistente != null) {
                    // LOG 3: Jogo real encontrado
                    Debug.WriteLine($"[DEBUG-MATA-MATA - MATCH] Jogo Real Encontrado. Rodada {rodada}. ID: {jogoExistente.Id}. ArbitroId: {jogoExistente.ArbitroId}.");
                    return jogoExistente;
                }

                // 2. Se não encontrou, é um novo jogo da primeira rodada. Cria o objeto para PERSISTÊNCIA.
                var novoJogo = new Jogo {
                    Id = 0, // Id = 0 indica um novo registro para o banco de dados auto-incrementar
                    TimeA = tA,
                    TimeAId = tA.Id,
                    TimeB = tB,
                    TimeBId = tB.Id,
                    Rodada = rodada,
                    IsOrganizador = IsOrganizador,
                    NomeArbitro = string.Empty,
                    Local = "A Definir",
                    CampeonatoClientAppId = Campeonato.ClientAppId
                };

                // 3. Persiste o jogo Imediatamente! O objeto novoJogo DEVE ser atualizado com o ID real.
                await _jogoService.SalvarJogoAsync(novoJogo); // Assumimos que SalvarJogoAsync insere e atualiza o novoJogo.Id

                Debug.WriteLine($"[DEBUG-MATA-MATA - PERSIST] Jogo Novo Criado e Salvo. Novo ID Real: {novoJogo.Id}.");

                // 4. Adiciona o novo jogo à lista de jogos salvos para que seja encontrado em buscas futuras (nesta sessão)
                jogosMataMataSalvos.Add(novoJogo);

                return novoJogo;
            }
            // *************************************************************


            // --- LÓGICA DE GERAÇÃO DO BRACKET (AJUSTADA PARA USAR ASYNC) ---
            if (timesAceitos.Count == 2) {
                totalRodadas = 1;
                var jogoFinal = await EncontrarOuCriarEPersistirJogo(timesAceitos[0], timesAceitos[1], 1);
                mockJogosFlat.Add(jogoFinal);
            } else if (timesAceitos.Count == 3) {
                totalRodadas = 2;
                var jogoSemi1 = await EncontrarOuCriarEPersistirJogo(timesAceitos[1], timesAceitos[2], 1);
                mockJogosFlat.Add(jogoSemi1);

                var vencedorPlaceholder = new Time { Nome = $"Vencedor Jogo {jogoSemi1.Id}", LogoUrl = "default_logo.png", Id = jogoSemi1.Id };
                // O Jogo Final ainda precisa ser um Mock, pois o Time B é um placeholder
                var jogoFinal = new Jogo {
                    Id = mockIdCounter--,
                    TimeA = timesAceitos[0],
                    TimeAId = timesAceitos[0].Id,
                    TimeB = vencedorPlaceholder,
                    TimeBId = jogoSemi1.Id,
                    Rodada = 2,
                    IsOrganizador = IsOrganizador,
                    NomeArbitro = string.Empty,
                    Local = "A Definir",
                };
                mockJogosFlat.Add(jogoFinal);
            } else if (timesAceitos.Count >= 4) {
                var mockTimeBye = new Time { Nome = "BYE", LogoUrl = "default_logo.png", Id = mockIdCounter-- };
                var participantesRodadaAtual = timesAceitos.ToList();
                int rodadaAtual = 1;

                while (participantesRodadaAtual.Count > 1) {
                    var participantesProximaRodada = new List<Time>();
                    int numJogosRodada = (int)Math.Ceiling(participantesRodadaAtual.Count / 2.0);

                    for (int i = 0; i < numJogosRodada; i++) {
                        Time timeA = participantesRodadaAtual[i * 2];
                        Time timeB;

                        int indexB = i * 2 + 1;

                        if (indexB < participantesRodadaAtual.Count) {
                            timeB = participantesRodadaAtual[indexB];
                        } else {
                            timeB = mockTimeBye;
                        }

                        Jogo jogoParaAdicionar;

                        // Só persistimos se for a primeira rodada E os dois times forem reais (IDs > 0)
                        if (rodadaAtual == 1 && timeA.Id > 0 && timeB.Id > 0) {
                            // Chamada ASYNC
                            jogoParaAdicionar = await EncontrarOuCriarEPersistirJogo(timeA, timeB, rodadaAtual);
                        } else {
                            // Se for rodada futura ou envolver BYE/VencedorPlaceholder, cria mock
                            jogoParaAdicionar = new Jogo {
                                Id = mockIdCounter--, // ID de mock APENAS para jogos futuros
                                TimeA = timeA,
                                TimeAId = timeA.Id,
                                TimeB = timeB,
                                TimeBId = timeB.Id,
                                Rodada = rodadaAtual,
                                IsOrganizador = IsOrganizador,
                                NomeArbitro = string.Empty,
                                Local = "A Definir",
                            };
                        }

                        mockJogosFlat.Add(jogoParaAdicionar);

                        if (timeB.Nome == "BYE") {
                            participantesProximaRodada.Add(timeA);
                        } else {
                            var idPlaceholder = (jogoParaAdicionar.Id > 0) ? jogoParaAdicionar.Id : Math.Abs(jogoParaAdicionar.Id);
                            var vencedorPlaceholder = new Time {
                                Nome = $"Vencedor Jogo {idPlaceholder}",
                                LogoUrl = "default_logo.png",
                                Id = jogoParaAdicionar.Id // Usa o ID do jogo como ID do Placeholder
                            };
                            participantesProximaRodada.Add(vencedorPlaceholder);
                        }
                    }
                    participantesRodadaAtual = participantesProximaRodada;
                    rodadaAtual++;
                }
                totalRodadas = rodadaAtual - 1;
            }
            // --- FIM DA LÓGICA DE GERAÇÃO DO BRACKET ---


            // *************************************************************
            // LOG 4: Aplicar NomeArbitro a TODOS os jogos na lista final (Funciona 100% agora)
            // *************************************************************
            foreach (var jogo in mockJogosFlat) {
                string nomeSetado = string.Empty;

                if (jogo.ArbitroId.HasValue && arbitrosMap.TryGetValue(jogo.ArbitroId.Value, out var nome)) {
                    jogo.NomeArbitro = nome;
                    nomeSetado = nome;
                } else {
                    jogo.NomeArbitro = string.Empty;
                    nomeSetado = jogo.ArbitroId.HasValue ? "ERRO: ID VÁLIDO, NOME NÃO MAPEADO" : "ID VAZIO";
                }

                // LOG 4: Atribuição final de nome
                Debug.WriteLine($"[DEBUG-MATA-MATA - FINAL] Jogo ID: {jogo.Id}. Rodada: {jogo.Rodada}. ArbitroId: {jogo.ArbitroId}. NomeAtribuído: {nomeSetado}.");

                jogo.NotifyArbitroStatusChanged();
            }
            // *************************************************************

            var groupedJogos = mockJogosFlat
                .OrderBy(j => j.Rodada)
                .GroupBy(j => j.Rodada)
                .Select(g => new RodadaGrouping(GetNomeRodada(g.Key, totalRodadas), g))
                .ToList();

            MainThread.BeginInvokeOnMainThread(() => {
                JogosMataMata.Clear();
                foreach (var group in groupedJogos) {
                    JogosMataMata.Add(group);
                }
            });
        }

        private void LoadImageSources() {
            try {
                //Debug.WriteLine($"[DEBUG-IMAGEM] BannerUrl: {Campeonato?.BannerUrl}");
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
                //Debug.WriteLine($"[ERRO-IMAGEM-ISOLADO] Falha ao carregar imagens: {ex.Message}");
                BannerSource = ImageSource.FromFile("default_banner.png");
                LogoSource = ImageSource.FromFile("default_logo.png");
            }
        }

        private async Task LoadTabelaClassificacaoAsync() {
            if (Campeonato is null) return;

            _timesPorGrupo.Clear();
            GruposDisponiveis.Clear();

            var todosOsTimes = await _databaseService.ObterTimesAceitosAsync(Campeonato.Id) ?? new List<Time>();

            if (!todosOsTimes.Any()) {
                MainThread.BeginInvokeOnMainThread(() => {
                    TabelaClassificacao.Clear();
                    EstatisticasTimes.Clear(); // Limpa a nova lista também
                    IsFiltroGrupoVisivel = false;
                });
                return;
            }

            _todosOsJogosDoCampeonato = await _databaseService.ObterJogosPorCampeonatoAsync(Campeonato.ClientAppId);

            await RecalcularEstatisticasDosTimesAsync(todosOsTimes);

            CalcularSequenciaDeJogos(todosOsTimes, _todosOsJogosDoCampeonato);

            await CalcularEstatisticasGeraisAsync(todosOsTimes, _todosOsJogosDoCampeonato);

            if (IsFormatoComGrupos) {

                int numTimes = todosOsTimes.Count;
                int numGruposNecessarios = 1;

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

                MainThread.BeginInvokeOnMainThread(() => {
                    GruposDisponiveis.Clear();

                    foreach (var group in grupos) {
                        GruposDisponiveis.Add(group.Key);
                        _timesPorGrupo[group.Key] = group.ToList();
                    }

                    if (GruposDisponiveis.Any()) {
                        if (string.IsNullOrEmpty(GrupoSelecionado) || !GruposDisponiveis.Contains(GrupoSelecionado))
                            GrupoSelecionado = GruposDisponiveis.First();

                        IsFiltroGrupoVisivel = IsFaseTabelaEJogos;

                        LoadTabelaClassificacaoPorGrupo(GrupoSelecionado);

                    } else {
                        TabelaClassificacao.Clear();
                        IsFiltroGrupoVisivel = false;
                    }
                });

            } else {
                var timesOrdenados = todosOsTimes
                    .OrderByDescending(t => t.Vitorias)
                    .ThenByDescending(t => t.PorcentagemVitoria)
                    .ToList();

                MainThread.BeginInvokeOnMainThread(() => {
                    TabelaClassificacao.Clear();
                    IsFiltroGrupoVisivel = false;

                    for (int i = 0; i < timesOrdenados.Count; i++) {
                        var time = timesOrdenados[i];
                        time.Posicao = i + 1;
                        int totalJogosJogados = time.Vitorias + time.Derrotas + time.Empates;
                        time.PorcentagemVitoria = (totalJogosJogados > 0) ? (double)time.Vitorias / totalJogosJogados : 0.0;
                        time.JogosAtras = 0;

                        TabelaClassificacao.Add(time);
                    }

                    //Debug.WriteLine($"[DEBUG-LOAD] Tabela de Classificação recarregada (Geral). Total: {TabelaClassificacao.Count}");
                });
            }
        }

        private async Task CalcularEstatisticasGeraisAsync(List<Time> times, List<Jogo> todosOsJogosDoCampeonato) {

            var listaEstatisticas = new List<TimeEstatisticaViewModel>();

            var todasAsEstatisticasDoCampeonato = await _databaseService.GetEstatisticasByCampeonatoIdAsync(Campeonato.Id);

            var jogosFinalizados = todosOsJogosDoCampeonato
                .Where(j =>
                    j.PlacarTimeAInt >= 0 && j.PlacarTimeBInt >= 0 &&
                    !(j.PlacarTimeAInt == 0 && j.PlacarTimeBInt == 0) 
                )
                .ToList();

            foreach (var time in times) {
                var statsViewModel = new TimeEstatisticaViewModel(time);

                statsViewModel.JogosDisputados = jogosFinalizados
                    .Count(j => j.TimeAId == time.Id || j.TimeBId == time.Id);

                var estatisticasDoTime = todasAsEstatisticasDoCampeonato
                    .Where(e => e.TimeId == time.Id)
                    .ToList();

                statsViewModel.TotalPontos = estatisticasDoTime.Sum(e => e.Pontos);
                statsViewModel.TotalRebotes = estatisticasDoTime.Sum(e => e.Rebotes);
                statsViewModel.TotalAssistencias = estatisticasDoTime.Sum(e => e.Assistencias);
                statsViewModel.TotalRoubos = estatisticasDoTime.Sum(e => e.Roubos);
                statsViewModel.TotalBloqueios = estatisticasDoTime.Sum(e => e.Bloqueios);
                statsViewModel.TotalTurnovers = estatisticasDoTime.Sum(e => e.Turnovers);
                statsViewModel.TotalFaltas = estatisticasDoTime.Sum(e => e.Faltas);

                statsViewModel.TotalArremessos2PontosConvertidos = estatisticasDoTime.Sum(e => e.Arremessos2PontosConvertidos);
                statsViewModel.TotalArremessos2PontosTentados = estatisticasDoTime.Sum(e => e.Arremessos2PontosTentados);

                statsViewModel.TotalArremessos3PontosConvertidos = estatisticasDoTime.Sum(e => e.Arremessos3PontosConvertidos);
                statsViewModel.TotalArremessos3PontosTentados = estatisticasDoTime.Sum(e => e.Arremessos3PontosTentados);

                statsViewModel.TotalLancesLivresConvertidos = estatisticasDoTime.Sum(e => e.LancesLivresConvertidos);
                statsViewModel.TotalLancesLivresTentados = estatisticasDoTime.Sum(e => e.LancesLivresTentados);

                listaEstatisticas.Add(statsViewModel);
            }

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

            var todosOsJogos = _todosOsJogosDoCampeonato;

            foreach (var time in times) {
                time.Vitorias = 0;
                time.Derrotas = 0;
                time.Empates = 0;
            }

            var timesMap = times.ToDictionary(t => t.Id);

            foreach (var jogo in todosOsJogos) {

                bool placarAZero = jogo.PlacarTimeAInt == 0;
                bool placarBZero = jogo.PlacarTimeBInt == 0;

                if (jogo.PlacarTimeAInt >= 0 && jogo.PlacarTimeBInt >= 0 && !(placarAZero && placarBZero)) {

                    if (timesMap.TryGetValue(jogo.TimeAId, out var timeA) &&
                        timesMap.TryGetValue(jogo.TimeBId, out var timeB)) {

                        if (jogo.PlacarTimeAInt > jogo.PlacarTimeBInt) {
                            timeA.Vitorias++;
                            timeB.Derrotas++;
                            timeA.PontuacaoTotal += 1;
                        } else if (jogo.PlacarTimeBInt > jogo.PlacarTimeAInt) {
                            timeB.Vitorias++;
                            timeA.Derrotas++;
                            timeB.PontuacaoTotal += 1;
                        }
                    }
                }
            }

            foreach (var time in times) {
                //Debug.WriteLine($"[STATS-DEBUG] Time: {time.Nome} | V: {time.Vitorias} | D: {time.Derrotas} | E: {time.Empates}");
            }

        }

        private void LoadTabelaClassificacaoPorGrupo(string grupo) {
            if (_timesPorGrupo.TryGetValue(grupo, out var timesDoGrupo)) {
                var timesOrdenados = timesDoGrupo
                    .OrderByDescending(t => t.Vitorias)
                    .ThenByDescending(t => t.PorcentagemVitoria)
                    .ToList();
                MainThread.BeginInvokeOnMainThread(() => {
                    TabelaClassificacao.Clear();

                    for (int i = 0; i < timesOrdenados.Count; i++) {
                        var time = timesOrdenados[i];
                        time.Posicao = i + 1;
                        int totalJogosJogados = time.Vitorias + time.Derrotas + time.Empates; // Corrigido para incluir Empates
                        time.PorcentagemVitoria = (totalJogosJogados > 0) ? (double)time.Vitorias / totalJogosJogados : 0.0;
                        time.JogosAtras = 0;

                        TabelaClassificacao.Add(time);
                    }

                    //Debug.WriteLine($"[DEBUG-LOAD] Tabela de Classificação recarregada (Grupo {grupo}). Total: {TabelaClassificacao.Count}");
                });
            }
        }

        private async Task GerarTabelaJogosAsync(Campeonato campeonato) {
            //Debug.WriteLine("[DEBUG-JOGOS] Iniciando GerarTabelaJogosAsync.");
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

            //Debug.WriteLine($"[DEBUG-JOGOS] Tabela de Jogos recarregada. Total de Rodadas: {_jogosPorRodada.Count}");
        }

        private void LoadRodada(int rodada) {
            //Debug.WriteLine($"[DEBUG-LOADRODADA] Iniciando LoadRodada({rodada}).");
            if (_jogosPorRodada.ContainsKey(rodada)) {
                var jogosDaRodada = _jogosPorRodada[rodada];
                MainThread.BeginInvokeOnMainThread(() => {
                    TabelaJogos.Clear();
                    foreach (var jogo in jogosDaRodada)
                        TabelaJogos.Add(jogo);

                    //Debug.WriteLine($"[DEBUG-LOADRODADA] Rodada {rodada} carregada com {TabelaJogos.Count} jogos.");
                });
            } else {
                MainThread.BeginInvokeOnMainThread(() => TabelaJogos.Clear());
                //Debug.WriteLine($"[DEBUG-LOADRODADA] Rodada {rodada} não encontrada. TabelaJogos limpa.");
            }
        }

        private async Task LoadPatrocinadoresAsync() {
            if (Campeonato == null) {
                //System.Diagnostics.Debug.WriteLine("[LoadPatrocinadoresAsync] Campeonato é nulo. Abortando.");
                return;
            }

            try {
                //System.Diagnostics.Debug.WriteLine($"[LoadPatrocinadoresAsync] Buscando Campanha para Campeonato ID: {Campeonato.ClientAppId}");

                var campanhaAtiva = await _patrocinioService.ObterCampanhaDeDivulgacaoAtivaAsync(Campeonato.ClientAppId);

                _campanhaPatrocinioAtiva = campanhaAtiva;

                //System.Diagnostics.Debug.WriteLine($"[LoadPatrocinadoresAsync] Campanha recebida do Service: {(campanhaAtiva != null ? $"ID {campanhaAtiva.Id} | Fim: {campanhaAtiva.Fim}" : "NULA")}");

                PatrocinadoresAtivos.Clear();

                if (campanhaAtiva != null) {
                    PatrocinadoresAtivos.Add(campanhaAtiva);
                }

                IsPatrocinioDivulgacaoVisible = PatrocinadoresAtivos.Any();

                //System.Diagnostics.Debug.WriteLine($"[LoadPatrocinadoresAsync] IsPatrocinioDivulgacaoVisible: {IsPatrocinioDivulgacaoVisible}");


                if (IsPatrocinioDivulgacaoVisible) {
                    string? caminhoBanner = PatrocinadoresAtivos.First().ImagemPatrocinador;

                    //System.Diagnostics.Debug.WriteLine($"[LoadPatrocinadoresAsync] Caminho Banner (ImagemPatrocinador): {caminhoBanner ?? "NULO/VAZIO"}");

                    if (string.IsNullOrEmpty(caminhoBanner)) {
                        BannerDivulgacaoSource = "placeholder.png";
                    } else {
                        BannerDivulgacaoSource = caminhoBanner;
                    }

                    //System.Diagnostics.Debug.WriteLine($"[LoadPatrocinadoresAsync] BannerDivulgacaoSource DEFINIDA para: {BannerDivulgacaoSource}");

                } else {
                    BannerDivulgacaoSource = null;
                    //System.Diagnostics.Debug.WriteLine("[LoadPatrocinadoresAsync] BannerDivulgacaoSource limpa.");
                }

            } catch (Exception ex) {
                //System.Diagnostics.Debug.WriteLine($"[PATROCINIO - CRÍTICO] Erro ao carregar patrocinadores: {ex.Message}");
                IsPatrocinioDivulgacaoVisible = false; 
                BannerDivulgacaoSource = null;
            }
        }

        private async Task LoadLideresEstatisticasAsync() {
            //Debug.WriteLine($"[DEBUG-STATS] ⭐️ Iniciando LoadLideresEstatisticasAsync ⭐️");
            if (Campeonato == null) {
                //Debug.WriteLine("[DEBUG-STATS] Campeonato nulo. Abortando carregamento.");
                return;
            }

            try {
                //Debug.WriteLine($"[DEBUG-STATS-STEP] Buscando times aceitos...");
                var todosOsTimes = await _databaseService.ObterTimesAceitosAsync(Campeonato.Id);
                //Debug.WriteLine($"[DEBUG-STATS-STEP] Total de times obtidos: {todosOsTimes.Count}");

                var timesMap = new Dictionary<Guid, Time>();
                foreach (var t in todosOsTimes) {
                    if (!timesMap.ContainsKey(t.ClientAppId))
                        timesMap[t.ClientAppId] = t;
                    //else
                        //Debug.WriteLine($"[WARN-STATS] Time duplicado ignorado: {t.Nome} ({t.ClientAppId})");
                }

                //Debug.WriteLine($"[DEBUG-STATS-STEP] Buscando jogadores do campeonato...");
                var todosOsJogadores = await _databaseService.ObterJogadoresPorCampeonatoAsync(Campeonato.ClientAppId);
                //Debug.WriteLine($"[DEBUG-STATS-STEP] Total de jogadores obtidos: {todosOsJogadores.Count}");

                if (!todosOsJogadores.Any()) {
                    //Debug.WriteLine("[DEBUG-STATS] Nenhum jogador encontrado. Limpando lista.");
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        LideresEstatisticas.Clear();
                    });
                    return;
                }

                //Debug.WriteLine($"[DEBUG-STATS-STEP] Buscando estatísticas agregadas...");
                var todasAsEstatisticasAgregadas = await _databaseService.GetEstatisticasDeJogadorByCampeonatoIdAsync(Campeonato.ClientAppId);
                //Debug.WriteLine($"[DEBUG-STATS-STEP] Total de estatísticas agregadas obtidas: {todasAsEstatisticasAgregadas.Count}");

                var statsMap = new Dictionary<int, EstatisticaAgregadaJogador>();
                foreach (var e in todasAsEstatisticasAgregadas) {
                    if (!statsMap.ContainsKey(e.UsuarioId))
                        statsMap[e.UsuarioId] = e;
                    //else
                        //Debug.WriteLine($"[WARN-STATS] Estatística duplicada ignorada para Usuário ID: {e.UsuarioId}");
                }

                var listaTempLideres = new List<JogadorEstatisticaViewModel>();

                foreach (var jogador in todosOsJogadores) {
                    Time? timeDoJogador = null;
                    if (jogador.TimeClientAppId.HasValue)
                        timesMap.TryGetValue(jogador.TimeClientAppId.Value, out timeDoJogador);

                    statsMap.TryGetValue(jogador.Id, out var statsAgregadas);

                    var statsJogador = new JogadorEstatisticaViewModel {
                        Id = jogador.Id,
                        NomeJogador = jogador.Nome,
                        FotoUrl = jogador.ImagemPath,
                        NomeTime = timeDoJogador?.Nome,
                        LogoTimeUrl = timeDoJogador?.LogoUrl
                    };

                    if (statsAgregadas != null) {
                        //Debug.WriteLine($"[DEBUG-STATS] Jogador: {jogador.Nome}, Pontos: {statsAgregadas.TotalPontos}");

                        statsJogador.Pontos = statsAgregadas.TotalPontos;
                        statsJogador.Assistencias = statsAgregadas.TotalAssistencias;
                        statsJogador.Rebotes = statsAgregadas.TotalRebotes;
                        statsJogador.Roubos = statsAgregadas.TotalRoubos;
                        statsJogador.Bloqueios = statsAgregadas.TotalBloqueios;
                        statsJogador.Turnovers = statsAgregadas.TotalTurnovers;
                        statsJogador.Faltas = statsAgregadas.TotalFaltas;
                        statsJogador.Arremessos2PontosConvertidos = statsAgregadas.Arremessos2PontosConvertidos;
                        statsJogador.Arremessos2PontosTentados = statsAgregadas.Arremessos2PontosTentados;
                        statsJogador.Arremessos3PontosConvertidos = statsAgregadas.Arremessos3PontosConvertidos;
                        statsJogador.Arremessos3PontosTentados = statsAgregadas.Arremessos3PontosTentados;
                        statsJogador.LancesLivresConvertidos = statsAgregadas.LancesLivresConvertidos;
                        statsJogador.LancesLivresTentados = statsAgregadas.LancesLivresTentados;
                    } else {
                        //Debug.WriteLine($"[DEBUG-STATS] Jogador: {jogador.Nome} - sem estatísticas registradas.");
                    }

                    listaTempLideres.Add(statsJogador);
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LideresEstatisticas.Clear();
                    foreach (var item in listaTempLideres)
                        LideresEstatisticas.Add(item);
                    var valorAtual = EstatisticaSelecionada;
                    EstatisticaSelecionada = "Turnovers";
                    EstatisticaSelecionada = valorAtual;
                    //Debug.WriteLine($"[DEBUG-OUTPUT] ItensEstatisticas Count: {ItensEstatisticas?.Count ?? 0}");
                    //Debug.WriteLine($"[DEBUG-OUTPUT] Primeiro item: Nome={ItensEstatisticas.FirstOrDefault()?.NomeJogador}, Valor={ItensEstatisticas.FirstOrDefault()?.ValorEstatisticaPrincipal}");
                    //Debug.WriteLine($"[DEBUG-LEADERS] Total de líderes carregados: {LideresEstatisticas.Count}");
                });
            } catch (Exception ex) {
                //Debug.WriteLine($"[FATAL CRASH] Ocorreu uma exceção crítica em LoadLideresEstatisticasAsync: {ex.Message}");
                //Debug.WriteLine($"[FATAL CRASH] StackTrace: {ex.StackTrace}");
            }
        }

        private ObservableCollection<JogadorEstatisticaViewModel> GetItensEstatisticasOrdenados(string estatistica) {
            IEnumerable<JogadorEstatisticaViewModel> orderedList = LideresEstatisticas;

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
                    orderedList = orderedList.OrderBy(j => j.Turnovers); 
                    break;
                case "Faltas":
                    orderedList = orderedList.OrderBy(j => j.Faltas); 
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


            var top5List = orderedList.Take(5);

            var rankedList = new List<JogadorEstatisticaViewModel>();
            int rank = 1;

            foreach (var item in top5List) {
                item.Posicao = rank++;

                item.ValorEstatisticaPrincipal = GetFormattedEstatisticaValue(item, estatistica);

                rankedList.Add(item);
            }

            return new ObservableCollection<JogadorEstatisticaViewModel>(rankedList);
        }

        private string GetFormattedEstatisticaValue(JogadorEstatisticaViewModel jogadorStats, string estatistica) {
            switch (estatistica) {
                case "Pontos": return jogadorStats.Pontos.ToString();
                case "Assistências": return jogadorStats.Assistencias.ToString();
                case "Rebotes": return jogadorStats.Rebotes.ToString();
                case "Roubos": return jogadorStats.Roubos.ToString();
                case "Bloqueios": return jogadorStats.Bloqueios.ToString();
                case "Turnovers": return jogadorStats.Turnovers.ToString();
                case "Faltas": return jogadorStats.Faltas.ToString();
                case "2 Pontos %": return $"{jogadorStats.Percentual2Pontos:P0}"; 
                case "3 Pontos %": return $"{jogadorStats.Percentual3Pontos:P0}"; 
                case "Lance Livre %": return $"{jogadorStats.PercentualLancesLivres:P0}"; 
                default: return jogadorStats.Pontos.ToString();
            }
        }

        public void MudarEstatisticaLogic(string estatistica) {
            if (EstatisticaSelecionada == estatistica)
                return;

            EstatisticaSelecionada = estatistica; 

            //Debug.WriteLine($"[DEBUG-STATS-LOGIC] Nova Estatística selecionada (Code-Behind): {EstatisticaSelecionada}.");
        }


        [RelayCommand]
        private async Task AlterarBanner() {
            //Debug.WriteLine("[CampeonatoDetailViewModel] Botão 'Alterar Banner' clicado.");
            var popup = new AlterarBannerPopup(Campeonato, _alertService, _databaseService, _syncService);

            popup.BannerAtualizado += (s, newBannerPath) => {
                //Debug.WriteLine($"[CampeonatoDetailViewModel] Evento BannerAtualizado recebido com caminho: '{newBannerPath}'");
                MainThread.BeginInvokeOnMainThread(() => {
                    if (!string.IsNullOrEmpty(newBannerPath)) {
                        if (File.Exists(newBannerPath)) {
                            BannerSource = ImageSource.FromFile(newBannerPath);
                        } else if (Uri.IsWellFormedUriString(newBannerPath, UriKind.Absolute)) {
                            BannerSource = ImageSource.FromUri(new Uri(newBannerPath));
                        } else {
                            //Debug.WriteLine("[CampeonatoDetailViewModel] Caminho/URL do novo banner é inválida ou arquivo não encontrado.");
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
                //Debug.WriteLine($"[DEBUG-CLIQUE] ERRO CRÍTICO: {ex.Message}");
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
                { "CampeonatoId", Campeonato.Id }
            };
            await Shell.Current.GoToAsync(nameof(TimesCadastradosPage), navigationParameters);
        }

        [RelayCommand]
        private async Task ListarArbitrosInscritos() {
            if (Campeonato is null) return;
            var navigationParameters = new ShellNavigationQueryParameters
            {
                { "CampeonatoId", Campeonato.ClientAppId }
            };
            await Shell.Current.GoToAsync(nameof(ArbitrosInscritosPage), navigationParameters);
        }

        [RelayCommand]
        private async Task AlterarBannerDivulgacao() {
            //Debug.WriteLine("[CampeonatoDetailViewModel] Botão 'Alterar Banner Divulgação' clicado.");

            if (_campanhaPatrocinioAtiva == null) {
                await _alertService.DisplayAlert("Sem Patrocínio", "Não há patrocínios ativos para alterar o banner.", "OK");
                return;
            }

            var popup = new AlterarBannerPatrocinioPopup(_campanhaPatrocinioAtiva, _alertService, _databaseService, _syncService);

            popup.BannerAtualizado += (s, newBannerPath) => {
                //Debug.WriteLine($"[CampeonatoDetailViewModel] Evento BannerAtualizado (Patrocínio) recebido com caminho: '{newBannerPath}'");
                MainThread.BeginInvokeOnMainThread(() => {
                    BannerDivulgacaoSource = newBannerPath;

                    _campanhaPatrocinioAtiva.ImagemPatrocinador = newBannerPath;
                });
            };
            await Application.Current.MainPage.Navigation.PushModalAsync(popup);
        }

    }
}