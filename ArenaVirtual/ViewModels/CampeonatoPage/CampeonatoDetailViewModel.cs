using ArenaVirtual.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using ArenaVirtual.Services;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Storage;
using ArenaVirtual.Popups;
using System.IO;
using ArenaVirtual.Views.CampeonatoPage;
using Microsoft.Maui.Devices;
using System.Collections.Generic; // Necessário para Dictionary
using System.Threading.Tasks; // Necessário para Task
using Microsoft.Maui.Controls; // Necessário para ImageSource, Application e MainThread

namespace ArenaVirtual.ViewModels.CampeonatoPage {
    public partial class CampeonatoDetailViewModel : ObservableObject, IQueryAttributable {
        [ObservableProperty]
        private Campeonato campeonato;

        [ObservableProperty]
        private ObservableCollection<Time> tabelaClassificacao;

        // CORREÇÃO/GARANTIA: TabelaJogos deve ser [ObservableProperty] se for alterada no LoadRodada
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

        // MÉTODO CORRIGIDO E MELHORADO: Carrega times e simula a primeira rodada
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

            // 4. Lógica de Jogos por Rodada - AGORA UTILIZA OS TIMES REAIS
            _jogosPorRodada.Clear();

            // Simulação da Rodada 1 para exibição inicial
            if (TabelaClassificacao.Count >= 2) {
                var rodada1Jogos = new ObservableCollection<Jogo>();

                // Cria pares de jogos a partir dos times carregados
                for (int i = 0; i < TabelaClassificacao.Count; i += 2) {
                    if (i + 1 < TabelaClassificacao.Count) {
                        // Emparelha o time 'i' com o time 'i+1'
                        rodada1Jogos.Add(new Jogo {
                            TimeA = TabelaClassificacao[i],
                            TimeB = TabelaClassificacao[i + 1],
                            PlacarA = "X", // Placeholder
                            PlacarB = "Y", // Placeholder
                            Rodada = 1 // Atribui a Rodada
                        });
                    } else {
                        // Se for um número ímpar de times, o último time "folga" ou joga contra um placeholder.
                        Debug.WriteLine($"[CampeonatoDetailViewModel] Time {TabelaClassificacao[i].Nome} tem folga na Rodada 1 (simulação).");
                    }
                }

                if (rodada1Jogos.Any()) {
                    _jogosPorRodada.Add(1, rodada1Jogos);
                }
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