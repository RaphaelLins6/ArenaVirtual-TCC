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

        // Dicionário privado para armazenar todos os jogos, separados por rodada
        private readonly Dictionary<int, ObservableCollection<Jogo>> _jogosPorRodada = new();

        private readonly IAlertService _alertService;
        private readonly DatabaseService _databaseService;
        private readonly SyncService _syncService;
        private readonly UsuarioService _usuarioService;
        private readonly IJogoService _jogoService;

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
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
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

                var usuarioAtual = SessaoService.Instancia.GetUsuarioAtual();
                IsOrganizador = (campeonato.OrganizadorId == usuarioAtual?.Id);

                Debug.WriteLine($"[CampeonatoDetailViewModel] É organizador? {IsOrganizador}");

                await LoadTabelaClassificacaoAsync();

                // 💡 Aqui o await é mantido porque é a primeira carga
                await GerarTabelaJogosAsync(campeonato);

                RodadaAtual = _jogosPorRodada.Keys.Any() ? _jogosPorRodada.Keys.Min() : 0;
                if (RodadaAtual > 0) {
                    LoadRodada(RodadaAtual);
                }

                // 💡 Manter a chamada. O tratamento de erro foi movido para dentro do método.
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

        private async Task LoadTabelaClassificacaoAsync() {
            if (Campeonato is null) return;

            var timesInscritos = await _databaseService.ObterTimesAceitosAsync(Campeonato.Id);

            var timesOrdenados = timesInscritos
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
                Debug.WriteLine($"[DEBUG-LOAD] Tabela de Classificação recarregada. Total: {TabelaClassificacao.Count}");
            });
        }

        private async Task GerarTabelaJogosAsync(Campeonato campeonato) {
            Debug.WriteLine("[DEBUG-JOGOS] Iniciando GerarTabelaJogosAsync.");
            _jogosPorRodada.Clear();

            var times = TabelaClassificacao.ToList();

            // ASSUMIMOS que esta função busca no DB e retorna JOGOS ATUALIZADOS
            var jogosGeradosPorRodada = await _jogoService.GerarTabelaJogosAsync(campeonato, times);

            var todosOsArbitrosIds = jogosGeradosPorRodada.Values
                                           .SelectMany(col => col.Select(j => j.ArbitroId))
                                           .Where(id => id.HasValue && id.Value != Guid.Empty)
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
                    string debugId = jogo.Id.ToString().Length > 4 ? jogo.Id.ToString().Substring(0, 4) : jogo.Id.ToString();

                    if (jogo.ArbitroId.HasValue && jogo.ArbitroId.Value != Guid.Empty && arbitrosMap.TryGetValue(jogo.ArbitroId.Value, out var nome)) {
                        jogo.NomeArbitro = nome;
                        Debug.WriteLine($"[DEBUG-JOGOS] Jogo ID {debugId}: Arbitro NOVO '{nome}'");
                    } else {
                        jogo.NomeArbitro = string.Empty;
                        Debug.WriteLine($"[DEBUG-JOGOS] Jogo ID {debugId}: Arbitro NÃO ATRIBUÍDO");
                    }

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

                    // DEBUG: Verifica o status do primeiro jogo após a recarga
                    if (TabelaJogos.Any()) {
                        Debug.WriteLine($"[DEBUG-LOADRODADA] Jogo 1 status: NomeArbitro='{TabelaJogos.First().NomeArbitro}'");
                    }
                });
            } else {
                MainThread.BeginInvokeOnMainThread(() => {
                    TabelaJogos.Clear();
                });
                Debug.WriteLine($"[DEBUG-LOADRODADA] Rodada {rodada} não encontrada. TabelaJogos limpa.");
            }
        }

        // --- Comandos de Ação e Navegação (Funcionalidades Mantidas) ---

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