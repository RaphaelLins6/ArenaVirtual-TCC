using ArenaVirtual.Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace ArenaVirtual.Services {
    public interface IJogoService {

        // Geração e Carregamento de Jogos de Pontos Corridos (Tabela)
        Task<Dictionary<int, ObservableCollection<Jogo>>> GerarTabelaJogosAsync(Campeonato campeonato, List<Time> timesInscritos);

        // --- Métodos de Filtro ---

        List<Jogo> FiltrarPartidasDoTime(Dictionary<int, ObservableCollection<Jogo>> todosOsJogosPorRodada, int timeId);

        // --- Métodos de Mata-Mata e Persistência (NOVOS) ---

        // Carrega a lista plana de jogos de mata-mata persistidos (para reuso e arbitragem)
        Task<List<Jogo>> ObterJogosMataMataPorCampeonatoAsync(Guid campeonatoClientAppId);

        // Deleta todos os jogos e suas dependências de um campeonato específico
        Task DeletarJogosDoCampeonatoAsync(Guid campeonatoClientAppId);
    }
}