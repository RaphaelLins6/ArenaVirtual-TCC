using ArenaVirtualAPI.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace ArenaVirtualAPI.Services {
    // A interface para a nossa fábrica de serviços de sincronização.
    public interface IBackendSyncServiceFactory {
        Task ProcessUploadAsync(JsonElement data, string entityType);
        Task<IEnumerable<ISyncable>> GetUpdatesAsync(string entityType, DateTime lastSyncTime);
    }
}