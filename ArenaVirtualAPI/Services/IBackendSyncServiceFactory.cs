using ArenaVirtualAPI.DTOs;

namespace ArenaVirtualAPI.Services {
    public interface IBackendSyncServiceFactory {
        Task<Dictionary<Guid, int>> ProcessAndMapItemsAsync<T>(IEnumerable<T> items, string entityType) where T : ISyncableDto;

        Task<IEnumerable<T>> GetUpdatesAsync<T>(string entityType, DateTime lastSyncTime) where T : ISyncableDto;
    }
}