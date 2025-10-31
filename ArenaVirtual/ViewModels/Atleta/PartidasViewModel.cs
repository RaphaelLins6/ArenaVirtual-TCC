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

        [ObservableProperty]
        private ObservableCollection<Jogo> proximasPartidas = new();

        [ObservableProperty]
        private ObservableCollection<Jogo> partidasAnteriores = new();

        [ObservableProperty]
        private bool isPartidasAnterioresExpanded = false;

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

        [CommunityToolkit.Mvvm.Input.RelayCommand]
        private void TogglePartidasAnteriores() {
            IsPartidasAnterioresExpanded = !IsPartidasAnterioresExpanded;
        }

        public async Task LoadTodasPartidasDoTimeAsync(Time timeDoAtleta) {
            if (timeDoAtleta == null) return;

            // Limpeza das coleções de exibição (IMPORTANTE)
            PartidasDoTime.Clear(); // Mantendo se ainda for usada na View, mas é recomendado usar as novas.
            ProximasPartidas.Clear(); // <--- NOVO
            PartidasAnteriores.Clear(); // <--- NOVO

            var todasPartidas = new List<Jogo>();

            var campeonatosIds = await _databaseService.ObterIdsCampeonatosDoTimeAceitoAsync(timeDoAtleta.ClientAppId);

            if (!campeonatosIds.Any()) {
                return;
            }

            foreach (var campId in campeonatosIds) {
                // ... (Lógica de carregamento de campeonato e timesInscritos - MANTIDA)
                var campeonato = await _databaseService.GetTable<Campeonato>()
                    .Where(c => c.Id == campId).FirstOrDefaultAsync();

                if (campeonato == null) continue;

                var timesInscritos = await _databaseService.ObterTimesAceitosAsync(campeonato.Id);

                if (timesInscritos.Count < 2) continue;

                var todosOsJogosPorRodada = await _jogoService.GerarTabelaJogosAsync(campeonato, timesInscritos.ToList());

                var jogosDoTimeNesteCamp = _jogoService.FiltrarPartidasDoTime(todosOsJogosPorRodada, timeDoAtleta.Id);

                foreach (var jogo in jogosDoTimeNesteCamp) {
                    jogo.NomeCampeonato = campeonato.Nome;
                }

                todasPartidas.AddRange(jogosDoTimeNesteCamp);
            }

            // ----------------------------------------------------------------------------------
            // NOVO BLOCO DE FILTRAGEM E SEPARAÇÃO
            // ----------------------------------------------------------------------------------
            var horaAtual = DateTime.Now;

            var proximas = new List<Jogo>();
            var anteriores = new List<Jogo>();

            // 1. Itera sobre todas as partidas para separar
            foreach (var jogo in todasPartidas) {
                // Usa DataHora para determinar se já passou
                if (jogo.DataHora > horaAtual) {
                    proximas.Add(jogo);
                } else {
                    anteriores.Add(jogo);
                }
            }

            // 2. Ordena as listas:
            // Próximas: Da mais recente para a mais distante.
            // Anteriores: Da mais recente para a mais antiga (para mostrar o histórico recente).
            var proximasOrdenadas = proximas.OrderBy(j => j.DataHora).ToList();
            var anterioresOrdenadas = anteriores.OrderByDescending(j => j.DataHora).ToList();

            // 3. Atualiza as propriedades observáveis na Main Thread
            MainThread.BeginInvokeOnMainThread(() => {
                // Limpeza (já feita no início, mas garantindo)
                ProximasPartidas.Clear();
                PartidasAnteriores.Clear();

                foreach (var jogo in proximasOrdenadas) {
                    ProximasPartidas.Add(jogo);
                }

                foreach (var jogo in anterioresOrdenadas) {
                    PartidasAnteriores.Add(jogo);
                }
            });

            // Debug.WriteLine($"[PartidasViewModel] ** FIM ** Total de jogos: {todasPartidas.Count}. Próximos: {ProximasPartidas.Count}. Anteriores: {PartidasAnteriores.Count}");
        }
    }
}