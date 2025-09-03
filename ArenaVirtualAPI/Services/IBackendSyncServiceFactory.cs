using ArenaVirtualAPI.DTOs;
using ArenaVirtualAPI.Models;
using System.Text.Json; // Adicione este namespace

namespace ArenaVirtualAPI.Services {
    public interface IBackendSyncServiceFactory {
        Task ProcessUploadAsync(JsonElement data, string entityType);
        Task<IEnumerable<ISyncable>> GetUpdatesAsync(string entityType, DateTime lastSyncTime);
    }
}