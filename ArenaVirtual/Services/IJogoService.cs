using ArenaVirtual.Models;
using System.Collections.ObjectModel;

namespace ArenaVirtual.Services {
    public interface IJogoService {
        Task<Dictionary<int, ObservableCollection<Jogo>>> GerarTabelaJogosAsync(Campeonato campeonato, List<Time> timesInscritos);
        List<Jogo> FiltrarPartidasDoTime(Dictionary<int, ObservableCollection<Jogo>> todosOsJogosPorRodada, int timeId);
    }
}