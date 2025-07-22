using ArenaVirtual.Models;
using ArenaVirtual.Services;

namespace ArenaVirtual.Services
{
    public class CampeonatoService(DatabaseService databaseService)
    {
        private readonly DatabaseService _databaseService = databaseService;

        public async Task<List<Campeonato>> ObterTodosAsync() =>
            await _databaseService.ListarCampeonatosAsync();

        public async Task<Campeonato?> ObterPorIdAsync(int id) =>
            (await _databaseService.ListarCampeonatosAsync()).FirstOrDefault(c => c.Id == id);

        public async Task<int> AdicionarAsync(Campeonato campeonato) =>
            await _databaseService.InserirCampeonatoAsync(campeonato);

        public async Task<int> AtualizarAsync(Campeonato campeonato) =>
            await _databaseService.AtualizarCampeonatoAsync(campeonato);

        public async Task<int> RemoverAsync(Campeonato campeonato) =>
            await _databaseService.DeletarCampeonatoAsync(campeonato);
    }
}
