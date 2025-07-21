using ArenaVirtual.Models;
using ArenaVirtual.Services;

namespace ArenaVirtual.Services
{
    public class CampeonatoService(DatabaseService databaseService)
    {
        private readonly DatabaseService _databaseService = databaseService;

        public async Task<List<Campeonato>> ObterTodosAsync() => await _databaseService.ListarCampeonatosAsync();
    }
}
