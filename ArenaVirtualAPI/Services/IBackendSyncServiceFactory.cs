// IBackendSyncServiceFactory.cs (Corrigido)

using ArenaVirtualAPI.DTOs;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace ArenaVirtualAPI.Services {
    public interface IBackendSyncServiceFactory {
        // Agora, a interface exige o dicionário de mapeamentos
        Task<Dictionary<Guid, int>> ProcessAndMapItemsAsync<T>(
            IEnumerable<T> items,
            string entityType,
            Dictionary<string, Dictionary<Guid, int>> idMappings)
            where T : ISyncableDto;

        Task<IEnumerable<T>> GetUpdatesAsync<T>(string entityType, DateTime lastSyncTime) where T : ISyncableDto;
    }
}