// ArenaVirtualAPI/Services/BackendSyncService.cs

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ArenaVirtualAPI.Dtos;

namespace ArenaVirtualAPI.Services {
    public class BackendSyncService {
        private readonly ILogger<BackendSyncService> _logger;
        private readonly IBackendSyncServiceFactory _syncServiceFactory;
        private readonly List<string> _entityTypes = new() { "Usuario", "Campeonato", "Time", "Convite" };

        public BackendSyncService(ILogger<BackendSyncService> logger, IBackendSyncServiceFactory syncServiceFactory) {
            _logger = logger;
            _syncServiceFactory = syncServiceFactory;
        }

        public async Task ProcessUploadAsync(JsonElement data, string modelTypeName) {
            await _syncServiceFactory.ProcessUploadAsync(data, modelTypeName);
        }

        public async Task<UpdatesDTO> GetUpdatesAsync(DateTime lastSyncTime) {
            var updates = new UpdatesDTO();

            foreach (var entityType in _entityTypes) {
                try {
                    var updatedItems = await _syncServiceFactory.GetUpdatesAsync(entityType, lastSyncTime);

                    if (updatedItems != null && updatedItems.Any()) {
                        var jsonElement = JsonSerializer.SerializeToElement(updatedItems);
                        updates.UpdatedItems.Add(entityType, jsonElement);
                    }
                } catch (Exception ex) {
                    _logger.LogError($"Erro ao obter atualizações para o tipo {entityType}: {ex.Message}");
                    throw;
                }
            }
            return updates;
        }
    }
}