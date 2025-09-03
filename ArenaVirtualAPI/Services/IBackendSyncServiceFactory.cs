using ArenaVirtualAPI.DTOs;
using System.Text.Json;

namespace ArenaVirtualAPI.Services {
    public interface IBackendSyncServiceFactory {
        Task ProcessUploadAsync(JsonElement data, string entityType);
        Task<IEnumerable<T>> GetUpdatesAsync<T>(string entityType, DateTime lastSyncTime) where T : ISyncableDto;
    }
}