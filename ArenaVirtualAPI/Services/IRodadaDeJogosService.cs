using ArenaVirtualAPI.DTOs;
using ArenaVirtualAPI.Models;

namespace ArenaVirtualAPI.Services {
    public interface IRodadaDeJogosService {

        // Os métodos usam os tipos concretos
        Task<IEnumerable<RodadaDeJogos>> GetAllAsync();
        Task<RodadaDeJogos?> GetByIdAsync(int id);
        Task AddAsync(RodadaDeJogos item);
        Task UpdateAsync(RodadaDeJogos item);

        // Métodos de sincronização
        Task<IEnumerable<RodadaDeJogosSyncDto>> GetUpdatedSinceAsync(DateTime lastSyncTime);
        Task<Dictionary<Guid, int>> ProcessAndMapItemsAsync(IEnumerable<RodadaDeJogosSyncDto> items);
        Task UpdateForeignKeysAsync(IEnumerable<RodadaDeJogosSyncDto> items, Dictionary<string, Dictionary<Guid, int>> idMappings);
    }
}