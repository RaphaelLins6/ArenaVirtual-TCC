using ArenaVirtualAPI.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace ArenaVirtualAPI.Services {
    // O serviço principal de sincronização, agora mais limpo.
    public class BackendSyncService {
        private readonly ILogger<BackendSyncService> _logger;
        private readonly IBackendSyncServiceFactory _syncServiceFactory;
        private readonly List<string> _entityTypes = new() { "Usuario", "Campeonato", "Time", "Convite" };

        // O construtor recebe a interface da fábrica.
        public BackendSyncService(ILogger<BackendSyncService> logger, IBackendSyncServiceFactory syncServiceFactory) {
            _logger = logger;
            _syncServiceFactory = syncServiceFactory;
        }

        // Processa o upload de dados, delegando à fábrica.
        public async Task ProcessUploadAsync(JsonElement data, string modelTypeName) {
            await _syncServiceFactory.ProcessUploadAsync(data, modelTypeName);
        }

        // Obtém as atualizações, delegando à fábrica.
        public async Task<UpdatesDTO> GetUpdatesAsync(DateTime lastSyncTime) {
            var updates = new UpdatesDTO();

            foreach (var entityType in _entityTypes) {
                try {
                    var updatedItems = await _syncServiceFactory.GetUpdatesAsync(entityType, lastSyncTime);
                    updates.UpdatedItems.Add(entityType, updatedItems);
                } catch (Exception ex) {
                    _logger.LogError($"Erro ao obter atualizações para o tipo {entityType}: {ex.Message}");
                    throw; // Rethrow para que o controller capture o erro 500
                }
            }
            return updates;
        }
    }
}