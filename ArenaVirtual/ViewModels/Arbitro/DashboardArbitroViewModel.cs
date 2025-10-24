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
        public ObservableCollection<JogoDetalheViewModel> JogosParaLancamento { get; } = new();
        public ObservableCollection<JogoDetalheViewModel> JogosLancados { get; } = new();

        [ObservableProperty]
        private bool _isJogosLancadosVisivel;
        [ObservableProperty]
        private bool _isJogosParaLancamentoVisivel;

        public DashboardArbitroViewModel(DatabaseService databaseService, SessaoService sessaoService) {
            _databaseService = databaseService;
            _sessaoService = sessaoService;
        }

        private async Task<bool> ObterArbitroIdLogadoAsync() {
            var arbitro = await _sessaoService.GetArbitroAtualAsync();

            if (arbitro != null) {
                _arbitroIdLogado = arbitro.ClientAppId;

                //Debug.WriteLine($"[DashboardArbitroViewModel] Árbitro logado ID: {_arbitroIdLogado}");
                return true;
            } else {
                _arbitroIdLogado = Guid.Empty;
                //Debug.WriteLine("[DashboardArbitroViewModel] Nenhum árbitro logado encontrado.");
                return false;
            }
        }

        [RelayCommand]
        public async Task LoadPartidasAsync() {
            if (EstaOcupado) return;
            EstaOcupado = true;

            try {
                if (!await ObterArbitroIdLogadoAsync() || _arbitroIdLogado == Guid.Empty) {
                    //Debug.WriteLine("[DashboardArbitroViewModel] Não foi possível carregar as partidas: Árbitro não logado.");
                    return;
                }

                JogosParaLancamento.Clear();
                JogosLancados.Clear();

                var jogosDoArbitro = await _databaseService.ObterJogosPorArbitroAsync(_arbitroIdLogado);

                if (jogosDoArbitro != null) {
                    await HidratarJogos(jogosDoArbitro);
                }

                if (jogosDoArbitro != null) {
                    foreach (var jogo in jogosDoArbitro.OrderBy(j => j.DataHora)) { // Ordenar por data
                        if (jogo.TimeA != null && jogo.TimeB != null && jogo.Campeonato != null) {
                            var detalhe = new JogoDetalheViewModel(
                                jogo,
                                jogo.TimeA.Nome,
                                jogo.TimeB.Nome,
                                jogo.Campeonato.Nome
                            );

                            if (jogo.Status == JogoStatus.Finalizado) {
                                JogosLancados.Add(detalhe);
                            } else {
                                JogosParaLancamento.Add(detalhe);
                            }
                        } else {
                            //Debug.WriteLine($"[DEBUG 3] Jogo descartado (ID: {jogo.Id}): Falta dados (TimeA/B/Campeonato).");
                        }
                    }
                }

                IsJogosParaLancamentoVisivel = JogosParaLancamento.Any();
                IsJogosLancadosVisivel = true;

                //Debug.WriteLine($"[DEBUG 4] Para Lançamento: {JogosParaLancamento.Count}, Lançados: {JogosLancados.Count}");

            } catch (Exception ex) {
                //Debug.WriteLine($"Erro ao carregar partidas do árbitro: {ex.Message}");
            } finally {
                EstaOcupado = false;
            }
        }

        private async Task HidratarJogos(List<Jogo> jogos) {
            var timeIds = jogos.SelectMany(j => new[] { j.TimeAId, j.TimeBId }).Where(id => id > 0).Distinct().ToList();
            var campeonatoClientAppIds = jogos.Select(j => j.CampeonatoClientAppId).Distinct().ToList();

            var listaTimes = await _databaseService.GetTable<Time>()
                                                 .Where(t => timeIds.Contains(t.Id))
                                                 .ToListAsync();
            var times = listaTimes.ToDictionary(t => t.Id, t => t);

            var listaCampeonatos = await _databaseService.GetTable<Campeonato>()
                                                         .Where(c => campeonatoClientAppIds.Contains(c.ClientAppId))
                                                         .ToListAsync();
            var campeonatos = listaCampeonatos.ToDictionary(c => c.ClientAppId, c => c);

            foreach (var jogo in jogos) {
                // Times
                if (times.TryGetValue(jogo.TimeAId, out var timeA)) jogo.TimeA = timeA;
                if (times.TryGetValue(jogo.TimeBId, out var timeB)) jogo.TimeB = timeB;

                // Campeonato
                if (campeonatos.TryGetValue(jogo.CampeonatoClientAppId, out var campeonato)) jogo.Campeonato = campeonato;

                if (jogo.TimeAId == -1) jogo.TimeA = new Time { Nome = "Folga", LogoUrl = "url_padrao_folga" }; 
                if (jogo.TimeBId == -1) jogo.TimeB = new Time { Nome = "Folga", LogoUrl = "url_padrao_folga" }; 
            }
        }

        [RelayCommand]
        private async Task NavigateToInscricoesAsync() {
            await Shell.Current.GoToAsync("CampeonatoInscricao");
        }

        [RelayCommand]
        private async Task PartidaSelecionadaAsync(JogoDetalheViewModel partidaDetalhe) {

            //Debug.WriteLine("[COMMAND LOG] PartidaSelecionadaAsync: Comando acionado.");

            if (partidaDetalhe == null) {
                //Debug.WriteLine("[COMMAND LOG] PartidaDetalhe é NULO.");
                return;
            }

            if (!partidaDetalhe.PodeLancarEstatisticas) {
                //Debug.WriteLine($"[COMMAND LOG] Jogo ID {partidaDetalhe.Jogo.Id}: Lançamento de estatísticas não habilitado. Saindo.");
                return;
            }

            var navigationParameters = new Dictionary<string, object>
            {
                { "JogoId", partidaDetalhe.Jogo.Id }
            };

            //Debug.WriteLine($"[COMMAND LOG] Tentando navegar para Jogo ID: {partidaDetalhe.Jogo.Id} usando Dictionary.");

            try {
                await Shell.Current.GoToAsync("LancamentoEstatisticaPage", navigationParameters);
                //Debug.WriteLine("[COMMAND LOG] Navegação solicitada com sucesso (via Dictionary).");
            } catch (Exception ex) {
               //Debug.WriteLine($"[COMMAND LOG ERROR] FALHA NA NAVEGAÇÃO: {ex.Message}");
                await Shell.Current.DisplayAlert("Erro de Navegação", "Não foi possível abrir a tela de estatísticas. Verifique a rota no AppShell.", "OK");
            }
        }
    }
}