using ArenaVirtualAPI.DTOs;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace ArenaVirtualAPI.Services {
    public interface IBackendSyncServiceFactory {
        Task<Dictionary<Guid, int>> ProcessAndMapItemsAsync<T>(
            IEnumerable<T> items,
            string entityType)
            where T : ISyncableDto;

        // Método renomeado para maior clareza
        Task UpdateForeignKeysAsync<T>(
            IEnumerable<T> items,
            string entityType,
            Dictionary<string, Dictionary<Guid, int>> idMappings)
            where T : ISyncableDto;

        Task<IEnumerable<T>> GetUpdatesAsync<T>(string entityType, DateTime lastSyncTime) where T : ISyncableDto;
    }
}