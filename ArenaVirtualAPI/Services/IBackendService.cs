using ArenaVirtualAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace ArenaVirtualAPI.Services {
    public interface IBackendService<T> : ISyncableService where T : class, ISyncable {
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T item);
        Task UpdateAsync(T item);
        Task<IEnumerable<ISyncable>> GetUpdatedSinceAsync(DateTime lastSyncTime);
        Task ProcessItemsAsync(IEnumerable<T> items);
    }
}