using ArenaVirtual.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using ArenaVirtual.Services;
using System.Collections.ObjectModel;
using ArenaVirtual.Popups;
using ArenaVirtual.Views.CampeonatoPage;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.Maui.Controls;
using System.IO;

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

        // Dicionário privado para armazenar todos os jogos, separados por rodada
        private readonly Dictionary<int, ObservableCollection<Jogo>> _jogosPorRodada = new();

        // NOVO CORRIGIDO: Dicionário para armazenar todos os times, separados por grupo
        private readonly Dictionary<string, List<Time>> _timesPorGrupo = new();

        private readonly IAlertService _alertService;
        private readonly DatabaseService _databaseService;
        private readonly SyncService _syncService;
        private readonly UsuarioService _usuarioService;
        private readonly IJogoService _jogoService;

        // Propriedade calculada para controlar a visibilidade do Picker de Grupos no XAML
        public bool IsFormatoComGrupos =>
            Campeonato?.FormatoCampeonato?.Contains("Grupos") == true ||
            Campeonato?.FormatoCampeonato?.Contains("Fase de Grupos") == true;

        // Construtor com Injeção de Dependência
        public CampeonatoDetailViewModel(IAlertService alertService, DatabaseService databaseService, SyncService syncService, IJogoService jogoService, UsuarioService usuarioService) {
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
            // A propriedade GrupoSelecionado mudou!
            if (value != null) {
                // Chame seu método de carregamento da tabela de classificação
                // para filtrar os dados com base no grupo recém-selecionado.
                LoadTabelaClassificacaoPorGrupo(value);
            } else {
                // Se o grupo for nulo (ex: Picker limpo), você pode decidir o que fazer.
                // Por exemplo, carregar a tabela geral ou limpar a exibição.
            }
        }


        // --- Implementação IQueryAttributable (Lógica de Recarga) ---

        public void ApplyQueryAttributes(IDictionary<string, object> query) {
            Debug.WriteLine("[DEBUG-ATTRIBUTES] ApplyQueryAttributes chamado.");
            // 1. **VERIFICAÇÃO DE ATUALIZAÇÃO DE JOGOS (Pop-up de Árbitro)**
            if (query.TryGetValue("jogoAtualizado", out object jogoObj) && jogoObj is Jogo jogoAtualizado) {
                Debug.WriteLine($"[DEBUG-ATTRIBUTES] Jogo ID {jogoAtualizado.Id} foi atualizado.");
                query.Remove("jogoAtualizado");

                // Encontra o jogo na lista da UI e atualiza suas propriedades
                var jogoNaLista = TabelaJogos.FirstOrDefault(j => j.Id == jogoAtualizado.Id);
                if (jogoNaLista != null) {
                    MainThread.BeginInvokeOnMainThread(() => {
                        jogoNaLista.ArbitroId = jogoAtualizado.ArbitroId;
                        jogoNaLista.NomeArbitro = jogoAtualizado.NomeArbitro;
                        // Notifica a UI que as propriedades que afetam o botão mudaram
                        jogoNaLista.NotifyArbitroStatusChanged();
                        Debug.WriteLine($"[DEBUG-ATTRIBUTES] UI do Jogo ID {jogoAtualizado.Id} atualizada diretamente na lista.");
                    });
                } else {
                    // Se não encontrar (ex: mudou de rodada), recarrega como fallback
                    _ = RecarregarJogosESelecaoAsync();
                }
                return;
            }

            // 2. **VERIFICAÇÃO DE ATUALIZAÇÃO DE TIMES (Gerenciamento de Times)**
            if (query.ContainsKey("TimesAtualizados")) {
                Debug.WriteLine("[DEBUG-ATTRIBUTES] Lista de Times foi atualizada. Recarregando Classificação e Jogos.");
                query.Remove("TimesAtualizados");

                if (Campeonato != null) {
                    // Recarrega a classificação, jogos e depois a rodada
                    _ = LoadTabelaClassificacaoAsync()
                        .ContinueWith(t => RecarregarJogosESelecaoAsync(), TaskScheduler.FromCurrentSynchronizationContext());
                }
                return;
            }

            // 3. LÓGICA DE NAVEGAÇÃO NORMAL
            if (query.ContainsKey("Campeonato")) {
                var campeonatoRecebido = query["Campeonato"] as Campeonato;
                if (Campeonato == null || Campeonato.Id != campeonatoRecebido.Id) {
                    _ = LoadCampeonato(campeonatoRecebido);
                } else {
                    Campeonato = campeonatoRecebido;
                    Debug.WriteLine("[DEBUG-ATTRIBUTES] ApplyQueryAttributes ignorou LoadCampeonato (Campeonato já carregado).");
                }
            }
        }

        // 💡 Método para garantir que a recarga da rodada só ocorra APÓS a tabela de jogos ser atualizada
        private async Task RecarregarJogosESelecaoAsync() {
            if (Campeonato == null) return;
            Debug.WriteLine("[DEBUG-RELOAD] Iniciando RecarregarJogosESelecaoAsync.");

            // 1. **Aguardamos** a recarga de TODOS os jogos e a atualização do dicionário _jogosPorRodada
            await GerarTabelaJogosAsync(Campeonato);
            Debug.WriteLine($"[DEBUG-RELOAD] GerarTabelaJogosAsync concluído. RodadaAtual: {RodadaAtual}");

            // 2. Recarrega a rodada atual na MainThread com os NOVOS dados
            if (RodadaAtual > 0) {
                // Executa na MainThread para garantir que o UI reaja
                MainThread.BeginInvokeOnMainThread(() => {
                    Debug.WriteLine($"[DEBUG-RELOAD] Chamando LoadRodada({RodadaAtual}) na MainThread.");
                    LoadRodada(RodadaAtual);
                });
            }
            Debug.WriteLine("[DEBUG-RELOAD] Finalizando RecarregarJogosESelecaoAsync.");
        }

        // --- Lógica de Carregamento Principal ---

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
                // CRÍTICO: Notifica a visibilidade do Picker de Grupos
                OnPropertyChanged(nameof(IsFormatoComGrupos));

                var usuarioAtual = SessaoService.Instancia.GetUsuarioAtual();
                IsOrganizador = (campeonato.OrganizadorId == usuarioAtual?.Id);

                Debug.WriteLine($"[CampeonatoDetailViewModel] É organizador? {IsOrganizador}");

                await LoadTabelaClassificacaoAsync(); // Chamada que agora cuida da lógica de grupos
                // 💡 Aqui o await é mantido porque é a primeira carga
                await GerarTabelaJogosAsync(campeonato);
                RodadaAtual = _jogosPorRodada.Keys.Any() ? _jogosPorRodada.Keys.Min() : 0;
                if (RodadaAtual > 0) {
                    LoadRodada(RodadaAtual);
                }

                // 💡 Manter a chamada.
                // O tratamento de erro foi movido para dentro do método.
                LoadImageSources();
            } catch (Exception ex) {
                // Este catch deve pegar apenas erros não tratados pelos try/catch internos
                Debug.WriteLine($"[ERRO CRÍTICO] LoadCampeonato falhou: {ex.Message}");
                await _alertService.DisplayAlert("Erro de Carregamento", "Ocorreu um erro ao carregar os detalhes do campeonato. Tente novamente.", "OK");
            } finally {
                IsBusy = false;
            }
        }

        // --- Métodos Auxiliares de Carregamento ---

        private void LoadImageSources() {
            // 💡 CORREÇÃO: Bloco try/catch para evitar o erro "Index and length" se a string da URL/Path estiver malformada.
            try {
                // --- Lógica de Banner ---
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

                // --- Lógica de Logo ---
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
            } catch (Exception ex) {
                Debug.WriteLine($"[ERRO-IMAGEM-ISOLADO] Falha ao carregar imagens: {ex.Message}");
                // Garante que o aplicativo não quebre e exiba algo padrão
                BannerSource = ImageSource.FromFile("default_banner.png");
                LogoSource = ImageSource.FromFile("default_logo.png");
            }
        }

        // MODIFICADO: Agora carrega todos os times e prepara para filtragem por grupo, se necessário
        private async Task LoadTabelaClassificacaoAsync() {
            if (Campeonato is null) return;

            _timesPorGrupo.Clear();
            GruposDisponiveis.Clear();

            // Assumimos que ObterTimesAceitosAsync retorna times com a propriedade 'Grupo' preenchida
            var todosOsTimes = await _databaseService.ObterTimesAceitosAsync(Campeonato.Id);

            if (todosOsTimes.Any() && IsFormatoComGrupos) {
                // NOVO BLOCO DE LÓGICA: Atribuição dinâmica de grupos para equilíbrio
                int numTimes = todosOsTimes.Count;
                int numGruposNecessarios = 1; // Valor padrão, caso não haja times ou a lógica abaixo não se aplique

                if (numTimes > 0) {
                    // Tenta encontrar o número de grupos que resulta em grupos mais equilibrados
                    // Prioriza grupos com 3 ou 4 times, se possível, e busca a divisão mais uniforme.

                    // Opção 1: Tentar grupos de 3
                    if (numTimes % 3 == 0 && numTimes / 3 >= 1) {
                        numGruposNecessarios = numTimes / 3; // Ex: 9 times -> 3 grupos de 3
                    }
                    // Opção 2: Tentar grupos de 4
                    else if (numTimes % 4 == 0 && numTimes / 4 >= 1) {
                        numGruposNecessarios = numTimes / 4; // Ex: 8 times -> 2 grupos de 4
                    }
                    // Opção 3: Tentar grupos de 2
                    else if (numTimes % 2 == 0 && numTimes / 2 >= 1) {
                        numGruposNecessarios = numTimes / 2; // Ex: 6 times -> 3 grupos de 2
                    }
                    // Opção 4: Se não houver divisores perfeitos para 2, 3 ou 4, 
                    // tenta criar o máximo de grupos possível com pelo menos 2 times por grupo.
                    // Ou, se o número de times for pequeno (ex: 3), cria 3 grupos de 1.
                    else if (numTimes >= 2) {
                        // Busca o maior divisor que resulte em grupos de tamanho razoável
                        for (int i = (int)Math.Sqrt(numTimes); i >= 1; i--) {
                            if (numTimes % i == 0) {
                                // i é um divisor. numTimes / i é o outro divisor.
                                // Queremos que o número de times por grupo seja razoável (ex: 2 a 5)
                                // E que o número de grupos seja o maior possível para distribuir melhor.
                                int divisor1 = i;
                                int divisor2 = numTimes / i;

                                // Prioriza ter mais grupos, mas com um mínimo de times por grupo (ex: 2)
                                if (divisor1 >= 2 && divisor2 >= 2) // Ambos os divisores resultam em grupos de pelo menos 2 times
                                {
                                    // Escolhe o que resulta em mais grupos (menor número de times por grupo)
                                    numGruposNecessarios = Math.Max(divisor1, divisor2);
                                    break;
                                } else if (divisor1 >= 2) // Se apenas um divisor resulta em grupos de pelo menos 2 times
                                  {
                                    numGruposNecessarios = numTimes / divisor1; // Usa o divisor para ter grupos de tamanho divisor1
                                    break;
                                } else if (divisor2 >= 2) {
                                    numGruposNecessarios = numTimes / divisor2; // Usa o divisor para ter grupos de tamanho divisor2
                                    break;
                                }
                            }
                        }
                        // Se ainda for 1 (não encontrou divisor adequado), e tiver times, cria grupos de 1
                        if (numGruposNecessarios == 1 && numTimes > 0) {
                            numGruposNecessarios = numTimes; // Ex: 3 times -> 3 grupos de 1
                        }
                    } else if (numTimes == 1) // Apenas 1 time, 1 grupo
                      {
                        numGruposNecessarios = 1;
                    }
                }

                // Agora, a lógica de atribuição sequencial de grupos:
                for (int i = 0; i < numTimes; i++) {
                    var time = todosOsTimes[i];
                    // Se o time não tem grupo (PONTO CRÍTICO), atribui um sequencialmente.
                    if (string.IsNullOrEmpty(time.Grupo)) {
                        // Atribuição sequencial (A, B, C, A, B, C...)
                        int grupoIndex = i % numGruposNecessarios;
                        time.Grupo = $"Grupo {((char)('A' + grupoIndex)).ToString()}";
                    }
                }
                // FIM DO NOVO BLOCO
                // 1. Agrupa os times e popula o dicionário de grupos
                var grupos = todosOsTimes
                                .Where(t => !string.IsNullOrEmpty(t.Grupo))
                                .GroupBy(t => t.Grupo)
                                .OrderBy(g => g.Key);

                MainThread.BeginInvokeOnMainThread(() => {
                    foreach (var group in grupos) {
                        GruposDisponiveis.Add(group.Key);
                        _timesPorGrupo.Add(group.Key, group.ToList());
                    }

                    // 2. Define o primeiro grupo como selecionado para carregar a primeira tabela
                    if (GruposDisponiveis.Any()) {
                        GrupoSelecionado = GruposDisponiveis.First();
                    } else {
                        // Limpa a tabela se não houver grupos válidos
                        TabelaClassificacao.Clear();
                    }
                });

            } else {
                // Lógica de classificação única (Pontos Corridos)

                var timesOrdenados = todosOsTimes
                                         .OrderByDescending(t => t.PontuacaoTotal)
                                         .ToList();

                // 💡 Garante que a manipulação da ObservableCollection seja na MainThread
                MainThread.BeginInvokeOnMainThread(() => {
                    TabelaClassificacao.Clear();

                    for (int i = 0; i < timesOrdenados.Count; i++) {
                        var time = timesOrdenados[i];

                        time.Posicao = i + 1;
                        int totalJogosDecididos = time.Vitorias + time.Derrotas;
                        time.PorcentagemVitoria = (totalJogosDecididos > 0) ? (double)time.Vitorias / totalJogosDecididos : 0.0;
                        time.JogosAtras = 0;
                        time.Sequencia = time.Vitorias > 0 ? "V" : (time.Derrotas > 0 ? "D" : "N/A");

                        TabelaClassificacao.Add(time);
                    }
                    Debug.WriteLine($"[DEBUG-LOAD] Tabela de Classificação recarregada (Geral). Total: {TabelaClassificacao.Count}");
                });
            }
        }

        // NOVO: Método para carregar a classificação de um grupo específico
        private void LoadTabelaClassificacaoPorGrupo(string grupo) {
            if (_timesPorGrupo.TryGetValue(grupo, out var timesDoGrupo)) {

                // 1. Classifica os times dentro do grupo (aplica o mesmo critério de ordenação)
                var timesOrdenados = timesDoGrupo
                                        .OrderByDescending(t => t.PontuacaoTotal)
                                        .ToList();

                // 2. Atualiza a ObservableCollection na MainThread
                MainThread.BeginInvokeOnMainThread(() => {
                    TabelaClassificacao.Clear();

                    for (int i = 0; i < timesOrdenados.Count; i++) {
                        var time = timesOrdenados[i];

                        // Recalcula a posição DENTRO do grupo
                        time.Posicao = i + 1;
                        int totalJogosDecididos = time.Vitorias + time.Derrotas;
                        time.PorcentagemVitoria = (totalJogosDecididos > 0) ? (double)time.Vitorias / totalJogosDecididos : 0.0;
                        time.JogosAtras = 0;
                        time.Sequencia = time.Vitorias > 0 ? "V" : (time.Derrotas > 0 ? "D" : "N/A");

                        TabelaClassificacao.Add(time);
                    }
                    Debug.WriteLine($"[DEBUG-LOAD] Tabela de Classificação recarregada (Grupo {grupo}). Total: {TabelaClassificacao.Count}");
                });
            }
        }

        // O restante dos métodos (GerarTabelaJogosAsync, LoadRodada, Comandos) permanecem inalterados.
        // ...
        private async Task GerarTabelaJogosAsync(Campeonato campeonato) {
            Debug.WriteLine("[DEBUG-JOGOS] Iniciando GerarTabelaJogosAsync.");
            _jogosPorRodada.Clear();

            var times = TabelaClassificacao.ToList();

            // ASSUMIMOS que esta função busca no DB e retorna JOGOS ATUALIZADOS
            var jogosGeradosPorRodada = await _jogoService.GerarTabelaJogosAsync(campeonato, times);
            var todosOsArbitrosIds = jogosGeradosPorRodada.Values
                                             .SelectMany(col => col.Select(j => j.ArbitroId))
                                             .Where(id => id.HasValue
                && id.Value != Guid.Empty)
                                             .Select(id => id.Value)
                                             .Distinct()

                                             .ToList();
            var arbitrosMap = await _usuarioService.ObterNomesUsuariosPorIdsAsync(todosOsArbitrosIds);

            bool isOrganizador = this.IsOrganizador;

            foreach (var rodadaEntry in jogosGeradosPorRodada) {
                var rodadaJogos = rodadaEntry.Value;
                foreach (var jogo in rodadaJogos) {
                    jogo.IsOrganizador = isOrganizador;
                    // 💡 CORREÇÃO CRÍTICA: Torna a operação Substring segura para o debug
                    string debugId = jogo.Id.ToString().Length > 4 ?
                    jogo.Id.ToString().Substring(0, 4) : jogo.Id.ToString();

                    if (jogo.ArbitroId.HasValue && jogo.ArbitroId.Value != Guid.Empty && arbitrosMap.TryGetValue(jogo.ArbitroId.Value, out var nome)) {
                        jogo.NomeArbitro = nome;
                    } else {
                        jogo.NomeArbitro = string.Empty;
                    }

                    // NOVO LOG: Mostra os IDs dos times e o status do árbitro para diagnóstico
                    Debug.WriteLine($"[TRACE-JOGOS] Jogo ID {debugId} (Rodada {rodadaEntry.Key}) -> TimeAId: {jogo.TimeAId}, TimeBId: {jogo.TimeBId}. Árbitro: '{jogo.NomeArbitro}'");
                    // Garante que a propriedade ligada ao texto do botão seja notificada (se o objeto Jogo for diferente)
                    jogo.NotifyArbitroStatusChanged();
                }

                _jogosPorRodada.Add(rodadaEntry.Key, rodadaJogos);
            }
            Debug.WriteLine($"[DEBUG-JOGOS] Tabela de Jogos recarregada. Total de Rodadas: {_jogosPorRodada.Count}");
        }

        private void LoadRodada(int rodada) {
            Debug.WriteLine($"[DEBUG-LOADRODADA] Iniciando LoadRodada({rodada}).");
            if (_jogosPorRodada.ContainsKey(rodada)) {
                var jogosDaRodada = _jogosPorRodada[rodada];
                // CRÍTICO: Limpar e Adicionar NOVOS objetos força o redesenho da CollectionView
                MainThread.BeginInvokeOnMainThread(() => {
                    TabelaJogos.Clear();
                    foreach (var jogo in jogosDaRodada) {
                        TabelaJogos.Add(jogo);
                    }
                    Debug.WriteLine($"[DEBUG-LOADRODADA] Rodada {rodada} carregada com {TabelaJogos.Count} jogos.");

                    // DEBUG APRIMORADO: Verifica o status do primeiro jogo após a recarga
                    if (TabelaJogos.Any()) {
                        var firstGame = TabelaJogos.First();
                        Debug.WriteLine($"[DEBUG-LOADRODADA] Jogo 1 ({firstGame.Id}): TimeAId={firstGame.TimeAId}, TimeBId={firstGame.TimeBId}, Árbitro='{firstGame.NomeArbitro}'");
                    }
                });
            } else {
                MainThread.BeginInvokeOnMainThread(() => {
                    TabelaJogos.Clear();
                });
                Debug.WriteLine($"[DEBUG-LOADRODADA] Rodada {rodada} não encontrada. TabelaJogos limpa.");
            }
        }

        // ... (Comandos de Ação e Navegação - Sem Alterações) ...

        [RelayCommand]
        private async Task AlterarBanner() {
            Debug.WriteLine("[CampeonatoDetailViewModel] Botão 'Alterar Banner' clicado.");
            var popup = new AlterarBannerPopup(Campeonato, _alertService, _databaseService, _syncService);

            popup.BannerAtualizado += (s, newBannerPath) => {
                Debug.WriteLine($"[CampeonatoDetailViewModel] Evento BannerAtualizado recebido com caminho: '{newBannerPath}'");
                MainThread.BeginInvokeOnMainThread(() => {
                    if (!string.IsNullOrEmpty(newBannerPath)) {
                        if (File.Exists(newBannerPath)) {
                            Debug.WriteLine("[CampeonatoDetailViewModel] Arquivo de banner local. Atualizando BannerSource.");
                            BannerSource = ImageSource.FromFile(newBannerPath);
                        } else if (Uri.IsWellFormedUriString(newBannerPath, UriKind.Absolute)) {
                            Debug.WriteLine("[CampeonatoDetailViewModel] URL de banner válida. Atualizando BannerSource.");
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
    }
}