using ArenaVirtual.Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace ArenaVirtual.Services {
    public interface IJogoService {
        Task<Dictionary<int, ObservableCollection<Jogo>>> GerarTabelaJogosAsync(Campeonato campeonato, List<Time> timesInscritos);
        List<Jogo> FiltrarPartidasDoTime(Dictionary<int, ObservableCollection<Jogo>> todosOsJogosPorRodada, int timeId);
        Task<List<Jogo>> ObterJogosMataMataPorCampeonatoAsync(Guid campeonatoClientAppId);
        Task DeletarJogosDoCampeonatoAsync(Guid campeonatoClientAppId);
        Task<int> SalvarJogoAsync(Jogo jogo);
    }
}