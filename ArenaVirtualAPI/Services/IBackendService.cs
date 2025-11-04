using ArenaVirtualAPI.DTOs;
using ArenaVirtualAPI.Models;

namespace ArenaVirtualAPI.Services {
    public interface IBackendService<TModel, TDto>
        where TModel : class, ISyncable
        where TDto : ISyncableDto {
        Task<TModel?> GetByIdAsync(int id);
        Task AddAsync(TModel item);
        Task UpdateAsync(TModel item);
        Task<IEnumerable<TDto>> GetUpdatedSinceAsync(DateTime lastSyncTime);
        Task<Dictionary<Guid, int>> ProcessAndMapItemsAsync(IEnumerable<TDto> items);
        Task UpdateForeignKeysAsync(IEnumerable<TDto> items, Dictionary<string, Dictionary<Guid, int>> idMappings);
    }
}