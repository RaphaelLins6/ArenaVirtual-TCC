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

        public async Task InitializeAsync(int timeId) {
            //Debug.WriteLine("----------------------------------------------------------------------------------------------------");
            //Debug.WriteLine($"[PartidasViewModel] ** INÍCIO ** InitializeAsync chamado com TimeId: {timeId}");

            if (timeId > 0 && (TimeDoAtleta == null || TimeDoAtleta.Id != timeId)) {
                await LoadTimeDoAtletaAsync(timeId);

                if (TimeDoAtleta != null) {
                    //Debug.WriteLine("[PartidasViewModel] Primeira carga de jogos via InitializeAsync.");
                    await LoadTodasPartidasDoTimeAsync(TimeDoAtleta);
                }
            } else if (timeId <= 0) {
                //Debug.WriteLine("[PartidasViewModel] ERRO FATAL: TimeId inválido (<= 0) ou não recebido.");
            } else {
                //Debug.WriteLine("[PartidasViewModel] Time já carregado. Pulando inicialização redundante.");
            }
            //Debug.WriteLine("----------------------------------------------------------------------------------------------------");
        }

        private async Task LoadTimeDoAtletaAsync(int timeId) {
            var time = await _databaseService.GetTimeByIdAsync(timeId);

            if (time != null) {
                TimeDoAtleta = time;
                NomeTime = time.Nome;
                //Debug.WriteLine($"[PartidasViewModel] Time do Atleta carregado com sucesso: {time.Nome} (ID: {time.Id})");
            } else {
                NomeTime = "Time não encontrado";
                TimeDoAtleta = null;
                //Debug.WriteLine($"[PartidasViewModel] ERRO: Time com ID {timeId} não encontrado no banco de dados.");
            }
        }

        public async Task LoadTodasPartidasDoTimeAsync(Time timeDoAtleta) {
            if (timeDoAtleta == null) return;

            PartidasDoTime.Clear();
            var todasPartidas = new List<Jogo>();

            var campeonatosIds = await _databaseService.ObterIdsCampeonatosDoTimeAceitoAsync(timeDoAtleta.ClientAppId);

            //Debug.WriteLine($"[PartidasViewModel] Passo 1: Encontrados {campeonatosIds.Count} IDs de campeonatos aceitos para o Time.");

            if (!campeonatosIds.Any()) {
                //Debug.WriteLine($"[PartidasViewModel] ALERTA: O time {timeDoAtleta.Nome} não tem convites ACEITOS na tabela Convite. Finalizando busca de jogos.");
                return;
            }

            foreach (var campId in campeonatosIds) {
                //Debug.WriteLine($"[PartidasViewModel] -- Iniciando busca para o Campeonato ID interno: {campId}");

                var campeonato = await _databaseService.GetTable<Campeonato>()
                  .Where(c => c.Id == campId).FirstOrDefaultAsync();

                if (campeonato == null) {
                    //Debug.WriteLine($"[PartidasViewModel] ERRO: Campeonato com ID {campId} não encontrado. Pulando.");
                    continue;
                }

                //Debug.WriteLine($"[PartidasViewModel] Carregando dados para o Campeonato: {campeonato.Nome}");

                var timesInscritos = await _databaseService.ObterTimesAceitosAsync(campeonato.Id);

                //Debug.WriteLine($"[PartidasViewModel] {timesInscritos.Count} times encontrados no campeonato {campeonato.Nome}.");

                if (timesInscritos.Count < 2) {
                    //Debug.WriteLine($"[PartidasViewModel] ALERTA: Poucos times ({timesInscritos.Count}) para gerar jogos no {campeonato.Nome}. Pulando.");
                    continue;
                }

                var todosOsJogosPorRodada = await _jogoService.GerarTabelaJogosAsync(campeonato, timesInscritos.ToList());

                var jogosDoTimeNesteCamp = _jogoService.FiltrarPartidasDoTime(todosOsJogosPorRodada, timeDoAtleta.Id);

                //Debug.WriteLine($"[PartidasViewModel] Encontrados {jogosDoTimeNesteCamp.Count} jogos específicos do time no campeonato {campeonato.Nome}.");

                foreach (var jogo in jogosDoTimeNesteCamp) {
                    jogo.NomeCampeonato = campeonato.Nome;
                }

                todasPartidas.AddRange(jogosDoTimeNesteCamp);
            }

            var partidasOrdenadas = todasPartidas.OrderBy(j => j.DataHora).ToList();

            PartidasDoTime.Clear();
            foreach (var jogo in partidasOrdenadas) {
                PartidasDoTime.Add(jogo);
            }

            //Debug.WriteLine($"[PartidasViewModel] ** FIM ** Total de {PartidasDoTime.Count} partidas consolidadas adicionadas à lista PartidasDoTime.");
        }
    }
}