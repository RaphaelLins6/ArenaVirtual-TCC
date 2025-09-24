using ArenaVirtual.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using ArenaVirtual.Services;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Storage;
using ArenaVirtual.Popups; // Adicione esta linha para usar o popup
using System.IO; // Adicionar para usar Path

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

        private readonly Dictionary<int, ObservableCollection<Jogo>> _jogosPorRodada = new();

        private readonly IAlertService _alertService;
        private readonly DatabaseService _databaseService;
        private readonly SyncService _syncService;

        // Construtor com injeção de dependência para os serviços
        public CampeonatoDetailViewModel(IAlertService alertService, DatabaseService databaseService, SyncService syncService) {
            TabelaClassificacao = new ObservableCollection<Time>();
            TabelaJogos = new ObservableCollection<Jogo>();
            _alertService = alertService;
            _databaseService = databaseService;
            _syncService = syncService;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query) {
            Debug.WriteLine("[CampeonatoDetailViewModel] ApplyQueryAttributes chamado.");
            if (query.ContainsKey("Campeonato")) {
                var campeonatoRecebido = query["Campeonato"] as Campeonato;
                LoadCampeonato(campeonatoRecebido);
            }
        }

        public void LoadCampeonato(Campeonato campeonato) {
            Debug.WriteLine("[CampeonatoDetailViewModel] LoadCampeonato chamado.");
            if (campeonato == null) {
                Debug.WriteLine("[CampeonatoDetailViewModel] Campeonato é nulo, retornando.");
                return;
            }

            Campeonato = campeonato;

            var usuarioAtual = SessaoService.Instancia.GetUsuarioAtual();
            IsOrganizador = (campeonato.OrganizadorId == usuarioAtual?.Id);

            Debug.WriteLine($"[CampeonatoDetailViewModel] ID do Campeonato: {campeonato.Id}");
            Debug.WriteLine($"[CampeonatoDetailViewModel] ID do Organizador do Campeonato: {campeonato.OrganizadorId}");
            Debug.WriteLine($"[CampeonatoDetailViewModel] ID do Usuário Logado (do serviço de sessão): {usuarioAtual?.Id}");
            Debug.WriteLine($"[CampeonatoDetailViewModel] A condição 'IsOrganizador' é: {IsOrganizador}");
            Debug.WriteLine($"[CampeonatoDetailViewModel] Campeonato carregado: {Campeonato?.Nome}");
            Debug.WriteLine($"[CampeonatoDetailViewModel] É organizador? {IsOrganizador}");

            LoadSimulatedData();

            RodadaAtual = _jogosPorRodada.Keys.Any() ? _jogosPorRodada.Keys.Min() : 0;
            if (RodadaAtual > 0) {
                LoadRodada(RodadaAtual);
            }

            Debug.WriteLine($"[CampeonatoDetailViewModel] Campeonato.BannerUrl: '{Campeonato.BannerUrl}'");
            if (!string.IsNullOrEmpty(Campeonato.BannerUrl)) {
                // Tenta carregar a imagem a partir de um arquivo local.
                // O `File.Exists` é a forma mais robusta de verificar.
                if (File.Exists(Campeonato.BannerUrl)) {
                    BannerSource = ImageSource.FromFile(Campeonato.BannerUrl);
                    Debug.WriteLine("[CampeonatoDetailViewModel] Banner carregado de um arquivo local.");
                }
                // Se não for um arquivo local válido, tenta carregar como uma URL.
                else if (Uri.IsWellFormedUriString(Campeonato.BannerUrl, UriKind.Absolute)) {
                    BannerSource = ImageSource.FromUri(new Uri(Campeonato.BannerUrl));
                    Debug.WriteLine("[CampeonatoDetailViewModel] Banner carregado de uma URL.");
                } else {
                    // Se o caminho não for nem um arquivo local nem uma URL, usa o padrão.
                    BannerSource = ImageSource.FromFile("default_banner.png");
                    Debug.WriteLine("[CampeonatoDetailViewModel] Caminho do banner inválido. Usando imagem padrão.");
                }
            } else {
                // Se não houver URL ou caminho, usa a imagem padrão.
                BannerSource = ImageSource.FromFile("default_banner.png");
                Debug.WriteLine("[CampeonatoDetailViewModel] Nenhum BannerUrl encontrado. Usando imagem padrão.");
            }
        }

        [RelayCommand]
        private async Task AlterarBanner() {
            Debug.WriteLine("[CampeonatoDetailViewModel] Botão 'Alterar Banner' clicado.");
            var popup = new AlterarBannerPopup(Campeonato, _alertService, _databaseService, _syncService);

            // Assine o evento para atualizar a imagem quando o popup fechar
            popup.BannerAtualizado += (s, newBannerPath) => {
                Debug.WriteLine($"[CampeonatoDetailViewModel] Evento BannerAtualizado recebido com caminho: '{newBannerPath}'");
                MainThread.BeginInvokeOnMainThread(() => {
                    if (string.IsNullOrEmpty(newBannerPath)) {
                        Debug.WriteLine("[CampeonatoDetailViewModel] Caminho do novo banner é nulo ou vazio.");
                        return;
                    }
                    if (File.Exists(newBannerPath)) {
                        Debug.WriteLine("[CampeonatoDetailViewModel] Arquivo de banner existe. Atualizando BannerSource.");
                        BannerSource = ImageSource.FromFile(newBannerPath);
                    } else {
                        Debug.WriteLine("[CampeonatoDetailViewModel] Arquivo de banner NÃO encontrado no caminho especificado.");
                    }
                });
            };

            await Application.Current.MainPage.Navigation.PushModalAsync(popup);
        }

        private void LoadSimulatedData() {
            var time1 = new Time { Posicao = 1, Nome = "Time A", LogoUrl = "https://example.com/logo_a.png", Vitorias = 5, Derrotas = 1, Empates = 0, PontuacaoTotal = 15 };
            var time2 = new Time { Posicao = 2, Nome = "Time B", LogoUrl = "https://example.com/logo_b.png", Vitorias = 4, Derrotas = 2, Empates = 0, PontuacaoTotal = 12 };
            var time3 = new Time { Posicao = 3, Nome = "Time C", LogoUrl = "https://example.com/logo_c.png", Vitorias = 3, Derrotas = 3, Empates = 0, PontuacaoTotal = 9 };

            TabelaClassificacao.Clear();
            TabelaClassificacao.Add(time1);
            TabelaClassificacao.Add(time2);
            TabelaClassificacao.Add(time3);

            _jogosPorRodada.Clear();
            _jogosPorRodada[1] = new ObservableCollection<Jogo>
            {
                new Jogo { TimeA = time1, TimeB = time2, PlacarA = "2", PlacarB = "1" },
                new Jogo { TimeA = time3, TimeB = time2, PlacarA = "0", PlacarB = "3" }
            };
            _jogosPorRodada[2] = new ObservableCollection<Jogo>
            {
                new Jogo { TimeA = time1, TimeB = time3, PlacarA = "5", PlacarB = "2" }
            };
        }

        [RelayCommand]
        private void MudarRodadaAnterior() {
            if (RodadaAtual > 1) {
                RodadaAtual--;
                LoadRodada(RodadaAtual);
            }
        }

        [RelayCommand]
        private void MudarRodadaProxima() {
            if (RodadaAtual < _jogosPorRodada.Count) {
                RodadaAtual++;
                LoadRodada(RodadaAtual);
            }
        }

        private void LoadRodada(int rodada) {
            if (_jogosPorRodada.ContainsKey(rodada)) {
                TabelaJogos = _jogosPorRodada[rodada];
            } else {
                TabelaJogos.Clear();
            }
        }

        [RelayCommand]
        private async Task GerenciarSolicitacoes() {
            Debug.WriteLine($"[CampeonatoDetailViewModel] Botão 'Gerenciar Solicitações' clicado para o campeonato: {Campeonato?.Nome}");
            await Application.Current.MainPage.DisplayAlert("Ação", "Você clicou em Gerenciar Solicitações. Implemente a navegação.", "OK");
        }
    }
}