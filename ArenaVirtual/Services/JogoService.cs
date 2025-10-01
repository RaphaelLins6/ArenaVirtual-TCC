using ArenaVirtual.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ArenaVirtual.Services {

    public class JogoService : IJogoService {

        private Jogo CriarNovoJogo(Time timeA, Time timeB, int rodada, string localCampeonato, DateTime dataHora) {
            return new Jogo {
                TimeA = timeA,
                TimeB = timeB,
                PlacarA = "X",
                PlacarB = "Y",
                Rodada = rodada,
                DataHora = dataHora,
                Local = localCampeonato
            };
        }

        public async Task<Dictionary<int, ObservableCollection<Jogo>>> GerarTabelaJogosAsync(Campeonato campeonato, List<Time> timesInscritos) {
            var jogosPorRodada = new Dictionary<int, ObservableCollection<Jogo>>();
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

            // CORREÇÃO: DuracaoCampeamento -> duracaoCampeonato
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
                    rodadaJogos.Add(CriarNovoJogo(timeA, timeB, r, campeonato.Local, dataHoraRodada));
                } else {
                    Debug.WriteLine($"Rodada {r}: {timeA.Nome} Folga.");
                }

                for (int i = 1; i < numJogosPorRodada; i++) {
                    Time timeX = timesRotativos[i];
                    Time timeY = timesRotativos[numRodadas - i];

                    if (timeX.Id != -1 && timeY.Id != -1) {
                        rodadaJogos.Add(CriarNovoJogo(timeX, timeY, r, campeonato.Local, dataHoraRodada));
                    }
                }

                if (rodadaJogos.Any()) {
                    jogosPorRodada.Add(r, rodadaJogos);
                }

                var ultimoTime = timesRotativos.Last();
                timesRotativos.RemoveAt(timesRotativos.Count - 1);
                timesRotativos.Insert(0, ultimoTime);
            }

            await Task.CompletedTask;
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
    }
}