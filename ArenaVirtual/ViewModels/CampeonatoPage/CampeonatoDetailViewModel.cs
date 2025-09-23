using ArenaVirtual.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using ArenaVirtual.Services;
using System.Collections.ObjectModel;

namespace ArenaVirtual.ViewModels.CampeonatoPage {
    // Implemente a interface IQueryAttributable para receber dados de navegação
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

        private readonly Dictionary<int, ObservableCollection<Jogo>> _jogosPorRodada = new();

        public CampeonatoDetailViewModel() {
            TabelaClassificacao = new ObservableCollection<Time>();
            TabelaJogos = new ObservableCollection<Jogo>();
        }

        // Método de navegação para receber o campeonato via parâmetro
        public void ApplyQueryAttributes(IDictionary<string, object> query) {
            if (query.ContainsKey("Campeonato")) {
                var campeonatoRecebido = query["Campeonato"] as Campeonato;
                LoadCampeonato(campeonatoRecebido);
            }
        }

        // Dentro do método `LoadCampeonato`
        public void LoadCampeonato(Campeonato campeonato) {
            if (campeonato == null) return;

            Campeonato = campeonato;

            // --- CORREÇÃO AQUI ---
            // Obtenha o ID do usuário logado a partir do seu serviço de sessão.
            var idDoUsuarioLogado = SessaoService.Instancia.GetUsuarioAtual().Id;

            // Adicione estas linhas para depuração
            Debug.WriteLine($"ID do Campeonato: {campeonato.Id}");
            Debug.WriteLine($"ID do Organizador do Campeonato: {campeonato.OrganizadorId}");
            Debug.WriteLine($"ID do Usuário Logado (do serviço de sessão): {idDoUsuarioLogado}");
            Debug.WriteLine($"A condição 'IsOrganizador' é: {campeonato.OrganizadorId == idDoUsuarioLogado}");

            IsOrganizador = (campeonato.OrganizadorId == idDoUsuarioLogado);
            // -------------------

            Debug.WriteLine($"[CampeonatoDetailViewModel] Campeonato carregado: {Campeonato?.Nome}");
            Debug.WriteLine($"[CampeonatoDetailViewModel] É organizador? {IsOrganizador}");

            LoadSimulatedData();

            RodadaAtual = _jogosPorRodada.Keys.Any() ? _jogosPorRodada.Keys.Min() : 0;
            if (RodadaAtual > 0) {
                LoadRodada(RodadaAtual);
            }
        }

        // ... o restante do seu código (LoadSimulatedData, RelayCommands) está correto e não precisa de alteração.
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

        [RelayCommand]
        private async Task AlterarBanner() {
            Debug.WriteLine($"[CampeonatoDetailViewModel] Botão 'Alterar Banner' clicado para o campeonato: {Campeonato?.Nome}");
            await Application.Current.MainPage.DisplayAlert("Ação", "Você clicou em Alterar Banner. Implemente a lógica de seleção de imagem.", "OK");
        }
    }
}