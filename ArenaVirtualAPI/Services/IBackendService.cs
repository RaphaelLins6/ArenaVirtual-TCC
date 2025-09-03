using ArenaVirtualAPI.Models;
using ArenaVirtualAPI.DTOs; // Adicione esta referência para o namespace de DTOs

namespace ArenaVirtualAPI.Services {
    public interface IBackendService<TModel, TDto>
        where TModel : class, ISyncable
        where TDto : ISyncableDto {
        Task<TModel?> GetByIdAsync(int id);
        Task AddAsync(TModel item);
        Task UpdateAsync(TModel item);
        Task<IEnumerable<TModel>> GetUpdatedSinceAsync(DateTime lastSyncTime);
        Task ProcessItemsAsync(IEnumerable<TDto> items);
    }
}