using ArenaVirtualAPI.Models;
using ArenaVirtualAPI.DTOs;

namespace ArenaVirtualAPI.Services {
    public interface IBackendService<TModel, TDto>
        where TModel : class, ISyncable
        where TDto : ISyncableDto {
        Task<TModel?> GetByIdAsync(int id);
        Task AddAsync(TModel item);
        Task UpdateAsync(TModel item);
        Task<IEnumerable<TDto>> GetUpdatedSinceAsync(DateTime lastSyncTime);
        // Remova a linha abaixo para resolver o erro:
        // Task<Dictionary<Guid, int>> ProcessAndMapItemsAsync(IEnumerable<TDto> items);

        // Mantenha apenas a sobrecarga que será usada no BackendSyncService
        Task<Dictionary<Guid, int>> ProcessAndMapItemsAsync(IEnumerable<TDto> items, Dictionary<string, Dictionary<Guid, int>> idMappings);
    }
}