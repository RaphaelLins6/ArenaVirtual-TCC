using ArenaVirtual.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArenaVirtual.Services {

    // Assumindo IJogoService e DatabaseService
    public class JogoService : IJogoService {

        private readonly DatabaseService _databaseService;

        public JogoService(DatabaseService databaseService) {
            _databaseService = databaseService;
        }

        // --- MÉTODOS AUXILIARES DE DATABASE E MODELO ---

        // NOVO método auxiliar para carregar um Time pelo ID
        private async Task<Time> ObterTimePorIdAsync(int timeId) {
            if (timeId <= 0) return null; // Ignora times de "Folga"

            // Supondo que você tem um método no DatabaseService para buscar um Time
            var time = await _databaseService.ObterTimePorIdAsync(timeId);

            return time;
        }

        // NOVO método auxiliar para preencher os objetos Time em uma lista de jogos (Hidratação)
        private async Task HidratarJogosComTimes(List<Jogo> jogos) {
            // 1. Coleta todos os IDs de times únicos necessários
            var timeIds = jogos
                .SelectMany(j => new[] { j.TimeAId, j.TimeBId })
                .Where(id => id > 0) // Filtra IDs válidos (ignora "Folga" = -1)
                .Distinct()
                .ToList();

            // 2. Carrega todos os objetos Time necessários
            var times = new Dictionary<int, Time>();
            // Nota: Para otimização real, você deve carregar todos os Times em uma única query no DatabaseService
            foreach (var id in timeIds) {
                var time = await ObterTimePorIdAsync(id);
                if (time != null) {
                    times.Add(id, time);
                }
            }

            // 3. Preenche as propriedades TimeA e TimeB de cada Jogo
            foreach (var jogo in jogos) {
                // Tenta obter o TimeA a partir do dicionário carregado
                if (times.TryGetValue(jogo.TimeAId, out var timeA)) {
                    jogo.TimeA = timeA;
                }
                // Tenta obter o TimeB a partir do dicionário carregado
                if (times.TryGetValue(jogo.TimeBId, out var timeB)) {
                    jogo.TimeB = timeB;
                }
                // Trata o caso de "Folga" se o timeId for -1 (opcional, mas claro)
                if (jogo.TimeAId == -1) jogo.TimeA = new Time { Nome = "Folga" };
                if (jogo.TimeBId == -1) jogo.TimeB = new Time { Nome = "Folga" };
            }
        }

        // Método auxiliar para converter a lista plana do DB para o dicionário da UI
        private Dictionary<int, ObservableCollection<Jogo>> MapJogosToRodadas(List<Jogo> jogos) {
            var jogosPorRodada = new Dictionary<int, ObservableCollection<Jogo>>();

            // Agrupa os jogos pela Rodada e mapeia para ObservableCollection
            var gruposRodadas = jogos
                .OrderBy(j => j.Rodada)
                .ThenBy(j => j.DataHora)
                .GroupBy(j => j.Rodada);

            foreach (var grupo in gruposRodadas) {
                jogosPorRodada.Add(grupo.Key, new ObservableCollection<Jogo>(grupo));
            }

            return jogosPorRodada;
        }

        // Método auxiliar para criar um novo objeto Jogo
        private Jogo CriarNovoJogo(Campeonato campeonato, Time timeA, Time timeB, int rodada, DateTime dataHora) {
            return new Jogo {
                // Chaves estrangeiras (IDs)
                TimeAId = timeA.Id,
                TimeBId = timeB.Id,
                CampeonatoClientAppId = campeonato.ClientAppId,

                // Propriedades de navegação (preenchidas na criação)
                TimeA = timeA,
                TimeB = timeB,

                PlacarA = "X", // Valores iniciais
                PlacarB = "Y", // Valores iniciais
                Rodada = rodada,
                DataHora = dataHora,
                Local = campeonato.Local
            };
        }

        // --- MÉTODOS PÚBLICOS ---

        public async Task<Dictionary<int, ObservableCollection<Jogo>>> GerarTabelaJogosAsync(Campeonato campeonato, List<Time> timesInscritos) {

            // 1. VERIFICAÇÃO CRÍTICA: Tenta carregar jogos existentes do banco de dados
            var jogosExistentes = await _databaseService.ObterJogosPorCampeonatoAsync(campeonato.ClientAppId);

            if (jogosExistentes.Any()) {
                Debug.WriteLine($"[JOGO SERVICE] Carregando {jogosExistentes.Count} jogos existentes para o campeonato.");

                // **CHAMADA DA CORREÇÃO: Hidratar jogos antes de retornar**
                await HidratarJogosComTimes(jogosExistentes);

                // Se jogos foram encontrados, retorna o dicionário mapeado.
                return MapJogosToRodadas(jogosExistentes);
            }

            // --- Lógica de Geração (executada APENAS se nenhum jogo foi encontrado) ---

            Debug.WriteLine($"[JOGO SERVICE] Nenhum jogo encontrado. Gerando nova tabela.");

            var jogosPorRodada = new Dictionary<int, ObservableCollection<Jogo>>();
            var jogosParaSalvar = new List<Jogo>();
            var times = timesInscritos.ToList();
            int n = times.Count;

            if (n < 2) return jogosPorRodada;

            // Adiciona a "Folga" se o número de times for ímpar
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
                    Debug.WriteLine($"Rodada {r}: {timeA.Nome} Folga.");
                    // Cria jogo de folga para manter a estrutura do dicionário se necessário para a UI
                    // var novoJogo = CriarNovoJogo(campeonato, timeA, new Time { Id = -1, Nome = "Folga" }, r, dataHoraRodada);
                    // rodadaJogos.Add(novoJogo); 
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

            // 2. PERSISTÊNCIA: Salvar NOVO campeonato.
            if (jogosParaSalvar.Any()) {
                Debug.WriteLine($"[JOGO SERVICE] Salvando {jogosParaSalvar.Count} jogos gerados no DB.");
                await _databaseService.InsertAllAsync(jogosParaSalvar);
            }

            return jogosPorRodada;
        }

        // Método adicional para filtragem
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