using ArenaVirtual.Models;
using ArenaVirtual.Models.ViewModels.Shared;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ArenaVirtual.ViewModels.Arbitro {

    // 🛑 IMPORTANTE: Necessita que SessaoService esteja disponível via DI ou como Singleton
    public partial class DashboardArbitroViewModel : ObservableObject {

        private readonly DatabaseService _databaseService;
        private readonly SessaoService _sessaoService; // 🛑 Adicionar SessaoService

        // 🛑 REMOVER: O ID será obtido dinamicamente.
        // private readonly int _arbitroIdLogado = 1; 
        private Guid _arbitroIdLogado; // O ID real do Árbitro no banco local

        [ObservableProperty]
        private bool _estaOcupado;

        public ObservableCollection<JogoDetalheViewModel> MinhasPartidas { get; } = new();

        // 🛑 CORREÇÃO: Receber SessaoService no construtor
        public DashboardArbitroViewModel(DatabaseService databaseService, SessaoService sessaoService) {
            _databaseService = databaseService;
            _sessaoService = sessaoService;

            // O carregamento inicial deve ser feito após a obtenção do ID
            // LoadPartidasCommand.Execute(null); 
        }

        // 🛑 NOVO: Método auxiliar para obter o ID do árbitro logado
        private async Task<bool> ObterArbitroIdLogadoAsync() {
            var arbitro = await _sessaoService.GetArbitroAtualAsync();

            if (arbitro != null) {
                // ✅ CORREÇÃO: Use o ID de sincronização (Guid)
                // Você deve ter uma propriedade ClientAppId ou similar no seu modelo Arbitro.
                _arbitroIdLogado = arbitro.ClientAppId; // <<< MUDANÇA AQUI (DE arbitro.Id para arbitro.ClientAppId)

                Debug.WriteLine($"[DashboardArbitroViewModel] Árbitro logado ID: {_arbitroIdLogado}");
                return true;
            } else {
                // ✅ CORREÇÃO: Use Guid.Empty como valor padrão para Guid
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
                // 🛑 CORREÇÃO: Obter o ID dinamicamente
                if (!await ObterArbitroIdLogadoAsync() || _arbitroIdLogado == Guid.Empty) {
                    Debug.WriteLine("[DashboardArbitroViewModel] Não foi possível carregar as partidas: Árbitro não logado.");
                    return;
                }

                MinhasPartidas.Clear();

                // 1. Obter todos os jogos onde o ArbitroId é o ID logado
                var jogosDoArbitro = await _databaseService.GetTable<Jogo>()
                                                          .Where(j => j.ArbitroId == _arbitroIdLogado) // Usa o ID dinâmico
                                                          .OrderByDescending(j => j.DataHora)
                                                          .ToListAsync();

                // 2. Resolver as dependências (Times e Campeonatos)
                foreach (var jogo in jogosDoArbitro) {
                    var timeA = await _databaseService.GetTimeByIdAsync(jogo.TimeAId);
                    var timeB = await _databaseService.GetTimeByIdAsync(jogo.TimeBId);
                    // Como GetTable<T> retorna AsyncTableQuery<T>, o .FirstOrDefaultAsync() é direto.
                    var campeonato = await _databaseService.GetTable<Campeonato>().Where(c => c.Id == jogo.CampeonatoId).FirstOrDefaultAsync();

                    // Adicionar ao ViewModel de Detalhe
                    if (timeA != null && timeB != null && campeonato != null) {
                        var detalhe = new JogoDetalheViewModel(
                            jogo,
                            timeA.Nome,
                            timeB.Nome,
                            campeonato.Nome
                        );
                        MinhasPartidas.Add(detalhe);
                    }
                }

            } catch (Exception ex) {
                Debug.WriteLine($"Erro ao carregar partidas do árbitro: {ex.Message}");
            } finally {
                EstaOcupado = false;
            }
        }

        [RelayCommand]
        private async Task NavigateToInscricoesAsync() {
            await Shell.Current.GoToAsync("CampeonatoInscricao");
        }

        [RelayCommand]
        private async Task PartidaSelecionadaAsync(JogoDetalheViewModel partidaDetalhe) {
            if (partidaDetalhe == null || !partidaDetalhe.PodeLancarEstatisticas) return;

            // Navega para a tela de lançamento, passando o ID do jogo
            await Shell.Current.GoToAsync($"LancamentoEstatisticaPage?JogoId={partidaDetalhe.Jogo.Id}");
        }
    }
}