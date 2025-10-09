using ArenaVirtual.Models;
using ArenaVirtual.Models.ViewModels.Shared;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ArenaVirtual.ViewModels.Arbitro {

    public partial class DashboardArbitroViewModel : ObservableObject {

        private readonly DatabaseService _databaseService;
        private readonly SessaoService _sessaoService; 

        private Guid _arbitroIdLogado; 

        [ObservableProperty]
        private bool _estaOcupado;

        public ObservableCollection<JogoDetalheViewModel> MinhasPartidas { get; } = new();

        public DashboardArbitroViewModel(DatabaseService databaseService, SessaoService sessaoService) {
            _databaseService = databaseService;
            _sessaoService = sessaoService;
 
        }

        private async Task<bool> ObterArbitroIdLogadoAsync() {
            var arbitro = await _sessaoService.GetArbitroAtualAsync();

            if (arbitro != null) {
                _arbitroIdLogado = arbitro.ClientAppId; 

                Debug.WriteLine($"[DashboardArbitroViewModel] Árbitro logado ID: {_arbitroIdLogado}");
                return true;
            } else {
                _arbitroIdLogado = Guid.Empty;
                Debug.WriteLine("[DashboardArbitroViewModel] Nenhum árbitro logado encontrado.");
                return false;
            }
        }

        [RelayCommand]
        public async Task LoadPartidasAsync() {
            if (EstaOcupado) return;
            EstaOcupado = true;

            try {
                if (!await ObterArbitroIdLogadoAsync() || _arbitroIdLogado == Guid.Empty) {
                    Debug.WriteLine("[DashboardArbitroViewModel] Não foi possível carregar as partidas: Árbitro não logado.");
                    return;
                }

                Debug.WriteLine($"[DEBUG 1] Árbitro ID utilizado na busca: {_arbitroIdLogado}");

                MinhasPartidas.Clear();

                // 1. CHAMA O NOVO MÉTODO (OU O MÉTODO QUE VOCÊ JÁ TEM QUE BUSCA JOGOS POR ARBITRO ID)
                // Usando a consulta que criamos no DatabaseService
                var jogosDoArbitro = await _databaseService.ObterJogosPorArbitroAsync(_arbitroIdLogado);
                // OU, se preferir manter o código atual (mas use o método ObterJogosPorArbitroAsync se o criar):
                // var jogosDoArbitro = await _databaseService.GetTable<Jogo>()
                //                                         .Where(j => j.ArbitroId == _arbitroIdLogado) 
                //                                         .OrderBy(j => j.DataHora)
                //                                         .ToListAsync();
                Debug.WriteLine($"[DEBUG 2] Jogos encontrados no DB para o árbitro: {jogosDoArbitro.Count}");

                // 2. HIDRATAÇÃO CENTRALIZADA (Melhoria de performance)
                // Chamamos um método auxiliar que carrega times e campeonatos de forma otimizada.
                // VOCÊ NÃO ME MANDOU O SERVICE DE CAMPEONATO, ENTÃO VAMOS CRIAR UMA FUNÇÃO AUXILIAR AQUI!
                await HidratarJogos(jogosDoArbitro); // Chamada para a nova função auxiliar

                // 3. MAPEAMENTO PARA VIEW MODEL
                int jogosMapeados = 0;
                foreach (var jogo in jogosDoArbitro) {
                    if (jogo.TimeA != null && jogo.TimeB != null && jogo.Campeonato != null) {
                        var detalhe = new JogoDetalheViewModel(
                            jogo,
                            jogo.TimeA.Nome, // 2ª string
                            jogo.TimeB.Nome, // 3ª string
                            jogo.Campeonato.Nome // 4ª string
                        );
                        MinhasPartidas.Add(detalhe);
                        jogosMapeados++;
                    } else {
                        // NOVO DEBUG AQUI: O Jogo está sendo descartado!
                        Debug.WriteLine($"[DEBUG 3] Jogo descartado (ID: {jogo.Id}): TimeA: {jogo.TimeA == null}, TimeB: {jogo.TimeB == null}, Campeonato: {jogo.Campeonato == null}");
                    }
                }
                Debug.WriteLine($"[DEBUG 4] Total de partidas adicionadas à UI: {jogosMapeados}");


                // Opcional: ordenar as próximas partidas para a UI, mesmo que já ordenado na query
                var proximasPartidas = MinhasPartidas.OrderBy(p => p.DataHora).ToList();
                MinhasPartidas.Clear();
                foreach (var partida in proximasPartidas) {
                    MinhasPartidas.Add(partida);
                }

            } catch (Exception ex) {
                Debug.WriteLine($"Erro ao carregar partidas do árbitro: {ex.Message}");
            } finally {
                EstaOcupado = false;
            }
        }

        // --- NOVO MÉTODO AUXILIAR DE HIDRATAÇÃO ---
        private async Task HidratarJogos(List<Jogo> jogos) {
            // 1. CARREGAR TIMES (Reutilizando a lógica do JogoService)
            // Como o método HidratarJogosComTimes é privado no JogoService, vamos adaptá-lo
            // ou, se possível, torná-lo público no JogoService e chamá-lo aqui.
            // Assumindo que você não quer mexer no JogoService agora, vamos fazer o básico:

            // Coleta todos os IDs de Times e Campeonatos únicos.
            var timeIds = jogos.SelectMany(j => new[] { j.TimeAId, j.TimeBId }).Where(id => id > 0).Distinct().ToList();
            var campeonatoIds = jogos.Select(j => j.CampeonatoClientAppId).Distinct().ToList(); // Usando ClientAppId para Campeonatos

            // Carrega todos os objetos Time
            var times = new Dictionary<int, Time>();
            foreach (var id in timeIds) {
                var time = await _databaseService.ObterTimePorIdAsync(id); // Reutilizando seu método do DatabaseService
                if (time != null) times.Add(id, time);
            }

            // Carrega todos os objetos Campeonato (Assumindo um método de busca por ClientAppId/GUID no DatabaseService)
            var campeonatos = new Dictionary<Guid, Campeonato>();
            foreach (var id in campeonatoIds) {
                // ATENÇÃO: É NECESSÁRIO IMPLEMENTAR O ObterCampeonatoPorClientAppIdAsync no DatabaseService
                var campeonato = await _databaseService.GetTable<Campeonato>()
                                                       .Where(c => c.ClientAppId == id)
                                                       .FirstOrDefaultAsync();
                if (campeonato != null) campeonatos.Add(id, campeonato);
            }

            // Preenche as propriedades de navegação
            foreach (var jogo in jogos) {
                // Times
                if (times.TryGetValue(jogo.TimeAId, out var timeA)) jogo.TimeA = timeA;
                if (times.TryGetValue(jogo.TimeBId, out var timeB)) jogo.TimeB = timeB;

                // Campeonato
                if (campeonatos.TryGetValue(jogo.CampeonatoClientAppId, out var campeonato)) jogo.Campeonato = campeonato;

                // Trata Folga (reaproveitando a lógica do JogoService)
                if (jogo.TimeAId == -1) jogo.TimeA = new Time { Nome = "Folga" };
                if (jogo.TimeBId == -1) jogo.TimeB = new Time { Nome = "Folga" };
            }
        }

        [RelayCommand]
        private async Task NavigateToInscricoesAsync() {
            await Shell.Current.GoToAsync("CampeonatoInscricao");
        }

        [RelayCommand]
        private async Task PartidaSelecionadaAsync(JogoDetalheViewModel partidaDetalhe) {
            if (partidaDetalhe == null || !partidaDetalhe.PodeLancarEstatisticas) return;

            await Shell.Current.GoToAsync($"LancamentoEstatisticaPage?JogoId={partidaDetalhe.Jogo.Id}");
        }
    }
}