using ArenaVirtual.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArenaVirtual.Services {

    public class JogoService : IJogoService {

        private readonly DatabaseService _databaseService;

        public JogoService(DatabaseService databaseService) {
            _databaseService = databaseService;
        }

        // --- MÉTODOS AUXILIARES DE DATABASE E MODELO ---
        private async Task<Time> ObterTimePorIdAsync(int timeId) {
            if (timeId <= 0) return null; 

            var time = await _databaseService.ObterTimePorIdAsync(timeId);

            return time;
        }

        private async Task HidratarJogosComTimes(List<Jogo> jogos) {
            var timeIds = jogos
                .SelectMany(j => new[] { j.TimeAId, j.TimeBId })
                .Where(id => id > 0) 
                .Distinct()
                .ToList();

            var times = new Dictionary<int, Time>();
            foreach (var id in timeIds) {
                var time = await ObterTimePorIdAsync(id);
                if (time != null) {
                    times.Add(id, time);
                }
            }

            foreach (var jogo in jogos) {
                if (times.TryGetValue(jogo.TimeAId, out var timeA)) {
                    jogo.TimeA = timeA;
                }
                if (times.TryGetValue(jogo.TimeBId, out var timeB)) {
                    jogo.TimeB = timeB;
                }
                if (jogo.TimeAId == -1) jogo.TimeA = new Time { Nome = "Folga" };
                if (jogo.TimeBId == -1) jogo.TimeB = new Time { Nome = "Folga" };
            }
        }

        private Dictionary<int, ObservableCollection<Jogo>> MapJogosToRodadas(List<Jogo> jogos) {
            var jogosPorRodada = new Dictionary<int, ObservableCollection<Jogo>>();

            var gruposRodadas = jogos
                .OrderBy(j => j.Rodada)
                .ThenBy(j => j.DataHora)
                .GroupBy(j => j.Rodada);

            foreach (var grupo in gruposRodadas) {
                jogosPorRodada.Add(grupo.Key, new ObservableCollection<Jogo>(grupo));
            }

            return jogosPorRodada;
        }

        private Jogo CriarNovoJogo(Campeonato campeonato, Time timeA, Time timeB, int rodada, DateTime dataHora) {
            return new Jogo {
                TimeAId = timeA.Id,
                TimeBId = timeB.Id,
                CampeonatoClientAppId = campeonato.ClientAppId,

                TimeA = timeA,
                TimeB = timeB,

                PlacarA = "X", 
                PlacarB = "Y", 
                Rodada = rodada,
                DataHora = dataHora,
                Local = campeonato.Local
            };
        }

        // --- MÉTODOS PÚBLICOS ---

        public async Task<Dictionary<int, ObservableCollection<Jogo>>> GerarTabelaJogosAsync(Campeonato campeonato, List<Time> timesInscritos) {

            var jogosExistentes = await _databaseService.ObterJogosPorCampeonatoAsync(campeonato.ClientAppId);

            if (jogosExistentes.Any()) {
                //Debug.WriteLine($"[JOGO SERVICE] Carregando {jogosExistentes.Count} jogos existentes para o campeonato.");

                await HidratarJogosComTimes(jogosExistentes);

                return MapJogosToRodadas(jogosExistentes);
            }

            // --- Lógica de Geração ---

            //Debug.WriteLine($"[JOGO SERVICE] Nenhum jogo encontrado. Gerando nova tabela.");

            var jogosPorRodada = new Dictionary<int, ObservableCollection<Jogo>>();
            var jogosParaSalvar = new List<Jogo>();
            var times = timesInscritos.ToList();
            int n = times.Count;

            if (n < 2) return jogosPorRodada;

            if (n % 2 != 0) {
                times.Add(new Time { Id = -1, Nome = "Folga", LogoUrl = "" });
                n++;
            }

            int numRodadas = n - 1;
            int numJogosPorRodada = n / 2;

            DateTime dataHoraBase = campeonato.DataInicio.Date.AddHours(18);
            TimeSpan duracaoCampeonato = campeonato.DataFim.Date - campeonato.DataInicio.Date;

            int totalDias = (int)duracaoCampeonato.TotalDays;

            double intervaloDiasPorRodada = 0;

            if (totalDias > 0 && numRodadas > 1) {
                intervaloDiasPorRodada = (double)totalDias / (numRodadas - 1);
            }

            var timesRotativos = times.Skip(1).ToList();

            for (int r = 1; r <= numRodadas; r++) {
                var rodadaJogos = new ObservableCollection<Jogo>();

                DateTime dataHoraRodada = dataHoraBase.AddDays((r - 1) * intervaloDiasPorRodada);
                if (dataHoraRodada.Date > campeonato.DataFim.Date) {
                    dataHoraRodada = campeonato.DataFim.Date.AddHours(dataHoraBase.Hour);
                }

                Time timeA = times[0];
                Time timeB = timesRotativos[0];

                if (timeB.Id != -1) {
                    var novoJogo = CriarNovoJogo(campeonato, timeA, timeB, r, dataHoraRodada);
                    rodadaJogos.Add(novoJogo);
                    jogosParaSalvar.Add(novoJogo);
                } else {
                    //Debug.WriteLine($"Rodada {r}: {timeA.Nome} Folga."); 
                }

                for (int i = 1; i < numJogosPorRodada; i++) {
                    Time timeX = timesRotativos[i];
                    Time timeY = timesRotativos[numRodadas - i];

                    if (timeX.Id != -1 && timeY.Id != -1) {
                        var novoJogo = CriarNovoJogo(campeonato, timeX, timeY, r, dataHoraRodada);
                        rodadaJogos.Add(novoJogo);
                        jogosParaSalvar.Add(novoJogo);
                    }
                }

                if (rodadaJogos.Any()) {
                    jogosPorRodada.Add(r, rodadaJogos);
                }

                var ultimoTime = timesRotativos.Last();
                timesRotativos.RemoveAt(timesRotativos.Count - 1);
                timesRotativos.Insert(0, ultimoTime);
            }

            if (jogosParaSalvar.Any()) {
                //Debug.WriteLine($"[JOGO SERVICE] Salvando {jogosParaSalvar.Count} jogos gerados no DB.");
                await _databaseService.InsertAllAsync(jogosParaSalvar);
            }

            return jogosPorRodada;
        }

        public List<Jogo> FiltrarPartidasDoTime(Dictionary<int, ObservableCollection<Jogo>> todosOsJogosPorRodada, int timeId) {
            var partidasDoTime = new List<Jogo>();

            foreach (var rodada in todosOsJogosPorRodada.Values) {
                var jogosFiltrados = rodada.Where(jogo =>
                    jogo.TimeA?.Id == timeId || jogo.TimeB?.Id == timeId
                );
                partidasDoTime.AddRange(jogosFiltrados);
            }

            return partidasDoTime.OrderBy(j => j.DataHora).ToList();
        }

        public async Task DeletarJogosDoCampeonatoAsync(Guid campeonatoClientAppId) {
            //Debug.WriteLine($"[JOGO SERVICE] Excluindo todos os jogos e dependências para o campeonato GUID: {campeonatoClientAppId}");
            await _databaseService.DeletarJogosECascataPorCampeonatoAsync(campeonatoClientAppId);
        }

        public async Task<List<Jogo>> ObterJogosMataMataPorCampeonatoAsync(Guid campeonatoClientAppId) {
            //Debug.WriteLine($"[JOGO SERVICE] Buscando jogos de Mata-Mata para o campeonato GUID: {campeonatoClientAppId}");
            var jogosSalvos = await _databaseService.ObterJogosPorCampeonatoAsync(campeonatoClientAppId);

            if (jogosSalvos == null || !jogosSalvos.Any()) {
                //Debug.WriteLine("[JOGO SERVICE] Nenhum jogo de Mata-Mata encontrado no DB.");
                return new List<Jogo>();
            }

            await HidratarJogosComTimes(jogosSalvos);

            //Debug.WriteLine($"[JOGO SERVICE] {jogosSalvos.Count} jogos de Mata-Mata carregados e hidratados.");

            return jogosSalvos;
        }
    }
}