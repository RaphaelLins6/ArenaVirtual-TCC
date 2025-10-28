using ArenaVirtualAPI.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using ArenaVirtualAPI.Models;

namespace ArenaVirtualAPI.Services {
    public interface IBackendService<TModel, TDto>
        where TModel : class, ISyncable
        where TDto : ISyncableDto {
        Task<TModel?> GetByIdAsync(int id);
        Task AddAsync(TModel item);
        Task UpdateAsync(TModel item);
        Task<IEnumerable<TDto>> GetUpdatedSinceAsync(DateTime lastSyncTime);

        // Método para upsert na primeira fase, retornando o mapeamento de IDs
        Task<Dictionary<Guid, int>> ProcessAndMapItemsAsync(IEnumerable<TDto> items);

        // Novo método para atualização de chaves estrangeiras na segunda fase
        Task UpdateForeignKeysAsync(IEnumerable<TDto> items, Dictionary<string, Dictionary<Guid, int>> idMappings);
    }
}