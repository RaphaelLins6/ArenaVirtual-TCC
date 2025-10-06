using ArenaVirtual.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using ArenaVirtual.Services;
using System.Collections.ObjectModel;
using ArenaVirtual.Popups;
using ArenaVirtual.Views.CampeonatoPage;

namespace ArenaVirtual.ViewModels.CampeonatoPage {
    // 📝 IQueryAttributable é uma interface síncrona, ajustamos a implementação dela.
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

        // 🆕 PROPRIEDADE PARA CONTROLE DE CARREGAMENTO/UI
        [ObservableProperty]
        private bool isBusy;

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

            // Verifica o idioma do dispositivo para definir IsDesktop
            IsDesktop = DeviceInfo.Idiom == DeviceIdiom.Desktop || DeviceInfo.Idiom == DeviceIdiom.Tablet;

            Debug.WriteLine($"[CampeonatoDetailViewModel] Device Idiom: {DeviceInfo.Idiom}. IsDesktop: {IsDesktop}");
        }

        // 📝 ASSINATURA CORRIGIDA: Implementação da interface IQueryAttributable deve ser void.
        public void ApplyQueryAttributes(IDictionary<string, object> query) {
            Debug.WriteLine("[CampeonatoDetailViewModel] ApplyQueryAttributes chamado.");
            if (query.ContainsKey("Campeonato")) {
                var campeonatoRecebido = query["Campeonato"] as Campeonato;

                // 🛑 CORREÇÃO PRINCIPAL: Previne o recarregamento total ao retornar do modal
                if (Campeonato == null || Campeonato.Id != campeonatoRecebido.Id) {
                    // A chamada async é iniciada aqui (fire-and-forget), com tratamento de erro interno.
                    LoadCampeonato(campeonatoRecebido);
                } else {
                    Campeonato = campeonatoRecebido;
                    Debug.WriteLine("[CampeonatoDetailViewModel] ApplyQueryAttributes ignorou LoadCampeonato (retorno de modal).");
                }
            }
        }

        public async Task LoadCampeonato(Campeonato campeonato) {
            Debug.WriteLine("[CampeonatoDetailViewModel] LoadCampeonato chamado.");

            if (IsBusy) return; // Impede chamadas múltiplas

            try {
                IsBusy = true; // 🚦 Inicia o carregamento

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

                // Chama a Geração de Jogos usando o Service
                // CORREÇÃO APLICADA: GerarTabelaJogosAsync agora lida com o NomeArbitro
                await GerarTabelaJogosAsync(campeonato);

                // Inicia a Rodada Atual.
                RodadaAtual = _jogosPorRodada.Keys.Any() ? _jogosPorRodada.Keys.Min() : 0;
                if (RodadaAtual > 0) {
                    LoadRodada(RodadaAtual);
                }

                // Lógica de Carregamento de Banner (MANTIDA)
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

                // Lógica de Carregamento de Logo (MANTIDA)
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
                Debug.WriteLine($"[ERRO CRÍTICO] LoadCampeonato falhou: {ex.Message}");
                // Notifica o usuário sobre o erro
                await _alertService.DisplayAlert("Erro de Carregamento", "Ocorreu um erro ao carregar os detalhes do campeonato. Tente novamente.", "OK");
            } finally {
                IsBusy = false; // ✅ Finaliza o carregamento, garantindo que a UI seja liberada
            }
        }

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

        // MÉTODO MELHORADO: Carrega times e popula a classificação (MANTIDO)
        private async Task LoadTabelaClassificacaoAsync() {
            if (Campeonato is null) return;

            // 1. Busca os times reais inscritos
            var timesInscritos = await _databaseService.ObterTimesAceitosAsync(Campeonato.Id);

            // 2. Lógica de Classificação (ordenar e calcular estatísticas)
            var timesOrdenados = timesInscritos
                     .OrderByDescending(t => t.PontuacaoTotal)
                     .ToList();

            // 3. Popula a Tabela de Classificação
            TabelaClassificacao.Clear();

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

        // ⚠️ CORREÇÃO: Método agora carrega o NomeArbitro e define IsOrganizador antes de notificar o modelo Jogo.
        private async Task GerarTabelaJogosAsync(Campeonato campeonato) {
            _jogosPorRodada.Clear();

            var times = TabelaClassificacao.ToList();

            // 1. Chama o JogoService para gerar/buscar os jogos
            var jogosGeradosPorRodada = await _jogoService.GerarTabelaJogosAsync(campeonato, times);

            // 2. Coleta IDs de árbitros únicos
            var todosOsArbitrosIds = jogosGeradosPorRodada.Values
                     .SelectMany(col => col.Select(j => j.ArbitroId))
                     .Where(id => id.HasValue)
                     .Select(id => id.Value)
                     .Distinct()
                     .ToList();

            // 3. Carrega os nomes dos árbitros em massa (para otimizar DB access)
            var arbitrosMap = await _usuarioService.ObterNomesUsuariosPorIdsAsync(todosOsArbitrosIds);

            bool isOrganizador = this.IsOrganizador;

            // 4. Processa cada jogo para popular IsOrganizador e NomeArbitro
            foreach (var rodadaEntry in jogosGeradosPorRodada) {
                var rodadaJogos = rodadaEntry.Value;

                foreach (var jogo in rodadaJogos) {

                    // Define IsOrganizador: Isso prepara o estado para o botão.
                    jogo.IsOrganizador = isOrganizador;

                    // Popula o NomeArbitro
                    if (jogo.ArbitroId.HasValue && arbitrosMap.TryGetValue(jogo.ArbitroId.Value, out var nome)) {
                        jogo.NomeArbitro = nome;
                    } else {
                        jogo.NomeArbitro = string.Empty;
                    }

                    // Garante que o estado do botão (TextoBotaoArbitro) seja atualizado 
                    // APÓS todos os dados dependentes estarem setados.
                    // OBS: Essa chamada aqui é importante para definir o estado inicial!
                    jogo.NotifyArbitroStatusChanged();
                }

                // 5. Atualiza o dicionário interno
                _jogosPorRodada.Add(rodadaEntry.Key, rodadaJogos);
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

        // ⚠️ CORREÇÃO: Remoção da reatribuição de 'jogo.IsOrganizador'
        private void LoadRodada(int rodada) {
            if (_jogosPorRodada.ContainsKey(rodada)) {
                var jogosDaRodada = _jogosPorRodada[rodada];

                // [CORRIGIDO]: Não há mais a reatribuição de IsOrganizador aqui.
                // Força a reavaliação do CollectionView:
                TabelaJogos.Clear();
                foreach (var jogo in jogosDaRodada) {
                    TabelaJogos.Add(jogo);
                }
            } else {
                TabelaJogos.Clear();
            }
        }

        // [CÓDIGO ANEXAR ARBITROS MANTIDO - ESTÁ CORRETO]
        [RelayCommand]
        public async Task AnexarArbitros(Jogo jogo) {
            Debug.WriteLine("========================================================================");
            Debug.WriteLine("[DEBUG-CLIQUE] INÍCIO: O comando AnexarArbitros foi acionado.");

            if (!IsOrganizador) {
                Debug.WriteLine("[DEBUG-CLIQUE] VERIFICAÇÃO: Usuário não é o Organizador. Acesso negado.");
                await _alertService.DisplayAlert("Acesso Negado", "Somente o organizador pode anexar árbitros a um jogo.", "OK");
                return;
            }

            if (jogo is null) {
                Debug.WriteLine("[DEBUG-CLIQUE] ERRO LÓGICO: O objeto Jogo recebido é NULO.");
                await _alertService.DisplayAlert("Erro de Dados", "O jogo selecionado não pôde ser carregado.", "OK");
                return;
            }

            Debug.WriteLine($"[DEBUG-CLIQUE] DADOS RECEBIDOS: Jogo ID: {jogo.Id} | Times: {jogo.TimeA?.Nome ?? "N/A"} vs {jogo.TimeB?.Nome ?? "N/A"}.");
            Debug.WriteLine($"[DEBUG-CLIQUE] STATUS INICIAL: ArbitroId: {jogo.ArbitroId} | NomeArbitro: '{jogo.NomeArbitro}' | Botão: '{jogo.TextoBotaoArbitro}' | Desabilitado: {jogo.BotaoArbitroDesabilitado}");


            try {
                var popup = new AtribuirArbitrosPopup(
                  Campeonato,
                  jogo,
                  _alertService,
                  _databaseService,
                  _syncService,
                  _usuarioService
                );

                EventHandler<Usuario> eventHandler = (sender, arbitro) => {
                    Debug.WriteLine($"[DEBUG-POPUP-EVENT] Árbitro '{arbitro.Nome}' atribuído e evento recebido no ViewModel.");

                    MainThread.BeginInvokeOnMainThread(() => {

                        var index = TabelaJogos.IndexOf(jogo);

                        if (index >= 0) {
                            // 📝 Boa prática para forçar a atualização de um item em ObservableCollection
                            TabelaJogos.RemoveAt(index);
                            TabelaJogos.Insert(index, jogo);

                            Debug.WriteLine("[DEBUG-POPUP-EVENT] Jogo removido e reinserido na TabelaJogos para forçar a atualização da UI.");
                        } else {
                            // Se o item não está na rodada atual, garante a notificação
                            jogo.NotifyArbitroStatusChanged();
                        }

                        Debug.WriteLine($"[DEBUG-POPUP-EVENT] STATUS PÓS-NOTIFICAÇÃO: ArbitroId: {jogo.ArbitroId} | NomeArbitro: '{jogo.NomeArbitro}' | Botão: '{jogo.TextoBotaoArbitro}' | Desabilitado: {jogo.BotaoArbitroDesabilitado}");
                        Debug.WriteLine($"[DEBUG-POPUP-EVENT] Árbitro '{arbitro.Nome}' anexado ao jogo com sucesso.");
                    });
                };

                popup.ArbitroAnexado += eventHandler;

                await Application.Current.MainPage.Navigation.PushModalAsync(popup);
                Debug.WriteLine("[DEBUG-CLIQUE] FIM: PushModalAsync acionado com sucesso. Pop-up deve estar visível.");

                // Garante que a remoção do handler aconteça sempre
                popup.ArbitroAnexado -= eventHandler;

            } catch (Exception ex) {
                Debug.WriteLine($"[DEBUG-CLIQUE] ERRO CRÍTICO: Falha ao abrir o Pop-up. Detalhes: {ex.Message}");
                await _alertService.DisplayAlert("Erro de Navegação", $"Não foi possível abrir a tela de árbitros: {ex.Message}", "OK");
            }

            Debug.WriteLine("========================================================================");
        }

        // [MÉTODOS DE NAVEGAÇÃO MANTIDOS]
        [RelayCommand]
        private async Task GerenciarSolicitacoes() {
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
        private async Task ListarTimesInscritos() {
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