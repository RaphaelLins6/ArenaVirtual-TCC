using ArenaVirtual.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ArenaVirtual.Services;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace ArenaVirtual.ViewModels.Atleta {

    // REMOVIDO: IQueryAttributable
    public partial class PartidasViewModel : ObservableObject {

        [ObservableProperty]
        private ObservableCollection<Jogo> partidasDoTime = new();

        [ObservableProperty]
        private Time? _timeDoAtleta;

        [ObservableProperty]
        private string _nomeTime = string.Empty;

        private readonly DatabaseService _databaseService;
        private readonly IJogoService _jogoService;

        public PartidasViewModel(DatabaseService databaseService, IJogoService jogoService) {
            _databaseService = databaseService;
            _jogoService = jogoService;
        }

        // NOVO MÉTODO: Chamado pela MeusTimesPage para inicializar a aba
        public async Task InitializeAsync(int timeId) {
            Debug.WriteLine("----------------------------------------------------------------------------------------------------");
            Debug.WriteLine($"[PartidasViewModel] ** INÍCIO ** InitializeAsync chamado com TimeId: {timeId}");

            if (timeId > 0 && (TimeDoAtleta == null || TimeDoAtleta.Id != timeId)) {
                // Carrega o objeto Time e popula TimeDoAtleta
                await LoadTimeDoAtletaAsync(timeId);

                if (TimeDoAtleta != null) {
                    // Executa a primeira carga de jogos
                    Debug.WriteLine("[PartidasViewModel] Primeira carga de jogos via InitializeAsync.");
                    await LoadTodasPartidasDoTimeAsync(TimeDoAtleta);
                }
            } else if (timeId <= 0) {
                Debug.WriteLine("[PartidasViewModel] ERRO FATAL: TimeId inválido (<= 0) ou não recebido.");
            } else {
                Debug.WriteLine("[PartidasViewModel] Time já carregado. Pulando inicialização redundante.");
            }
            Debug.WriteLine("----------------------------------------------------------------------------------------------------");
        }

        // O método ApplyQueryAttributes FOI REMOVIDO para evitar conflitos.

        private async Task LoadTimeDoAtletaAsync(int timeId) {
            var time = await _databaseService.GetTimeByIdAsync(timeId);

            if (time != null) {
                TimeDoAtleta = time;
                NomeTime = time.Nome;
                Debug.WriteLine($"[PartidasViewModel] Time do Atleta carregado com sucesso: {time.Nome} (ID: {time.Id})");
            } else {
                NomeTime = "Time não encontrado";
                TimeDoAtleta = null;
                Debug.WriteLine($"[PartidasViewModel] ERRO: Time com ID {timeId} não encontrado no banco de dados.");
            }
        }

        // MÉTODO: Carrega partidas de TODOS os campeonatos do time (agora chamado por InitializeAsync e OnAppearing da PartidasPage)
        public async Task LoadTodasPartidasDoTimeAsync(Time timeDoAtleta) {
            if (timeDoAtleta == null) return;

            PartidasDoTime.Clear();
            var todasPartidas = new List<Jogo>();

            // 1. Obter todos os IDs de campeonatos onde este time foi ACEITO
            var campeonatosIds = await _databaseService.ObterIdsCampeonatosDoTimeAceitoAsync(timeDoAtleta.ClientAppId);

            Debug.WriteLine($"[PartidasViewModel] Passo 1: Encontrados {campeonatosIds.Count} IDs de campeonatos aceitos para o Time.");

            if (!campeonatosIds.Any()) {
                Debug.WriteLine($"[PartidasViewModel] ALERTA: O time {timeDoAtleta.Nome} não tem convites ACEITOS na tabela Convite. Finalizando busca de jogos.");
                return;
            }

            // 2. Iterar sobre cada campeonato e gerar/filtrar os jogos
            foreach (var campId in campeonatosIds) {
                Debug.WriteLine($"[PartidasViewModel] -- Iniciando busca para o Campeonato ID interno: {campId}");

                var campeonato = await _databaseService.GetTable<Campeonato>()
                  .Where(c => c.Id == campId).FirstOrDefaultAsync();

                if (campeonato == null) {
                    Debug.WriteLine($"[PartidasViewModel] ERRO: Campeonato com ID {campId} não encontrado. Pulando.");
                    continue;
                }

                Debug.WriteLine($"[PartidasViewModel] Carregando dados para o Campeonato: {campeonato.Nome}");

                // 2.1. Obter a lista de times inscritos neste campeonato
                var timesInscritos = await _databaseService.ObterTimesAceitosAsync(campeonato.Id);

                Debug.WriteLine($"[PartidasViewModel] {timesInscritos.Count} times encontrados no campeonato {campeonato.Nome}.");

                if (timesInscritos.Count < 2) {
                    Debug.WriteLine($"[PartidasViewModel] ALERTA: Poucos times ({timesInscritos.Count}) para gerar jogos no {campeonato.Nome}. Pulando.");
                    continue;
                }

                // 2.2. Gerar todos os jogos do campeonato
                var todosOsJogosPorRodada = await _jogoService.GerarTabelaJogosAsync(campeonato, timesInscritos.ToList());

                // 2.3. Filtrar os jogos do time
                var jogosDoTimeNesteCamp = _jogoService.FiltrarPartidasDoTime(todosOsJogosPorRodada, timeDoAtleta.Id);

                Debug.WriteLine($"[PartidasViewModel] Encontrados {jogosDoTimeNesteCamp.Count} jogos específicos do time no campeonato {campeonato.Nome}.");

                // 2.4. Adicionar o Título do Campeonato ao Card
                foreach (var jogo in jogosDoTimeNesteCamp) {
                    jogo.NomeCampeonato = campeonato.Nome;
                }

                todasPartidas.AddRange(jogosDoTimeNesteCamp);
            }

            // 3. Ordenar todas as partidas (por data é o mais comum)
            var partidasOrdenadas = todasPartidas.OrderBy(j => j.DataHora).ToList();

            // 4. Popular a coleção
            PartidasDoTime.Clear();
            foreach (var jogo in partidasOrdenadas) {
                PartidasDoTime.Add(jogo);
            }

            Debug.WriteLine($"[PartidasViewModel] ** FIM ** Total de {PartidasDoTime.Count} partidas consolidadas adicionadas à lista PartidasDoTime.");
        }
    }
}