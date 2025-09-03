using ArenaVirtualAPI.DTOs;
using System.Collections;
using System.Text.Json;
using System.Collections.Generic;

namespace ArenaVirtualAPI.Services {
    public class BackendSyncService {
        private readonly ILogger<BackendSyncService> _logger;
        private readonly IBackendSyncServiceFactory _syncServiceFactory;

        private readonly List<string> _entityTypes = new() {
            "Usuario",
            "Campeonato",
            "Time",
            "Convite",
            "UsuarioCampeonatoFavorito"
        };

        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public BackendSyncService(ILogger<BackendSyncService> logger, IBackendSyncServiceFactory syncServiceFactory) {
            _logger = logger;
            _syncServiceFactory = syncServiceFactory;
            _jsonSerializerOptions = new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true,
            };
        }

        // CORREÇÃO: Passa a JsonElement diretamente para a fábrica
        public async Task ProcessUploadAsync(JsonElement data, string modelTypeName) {
            try {
                _logger.LogInformation($"[BackendSyncService] Recebidos dados para processar em {modelTypeName}");

                // Chama a fábrica com a JsonElement e o nome do tipo
                await _syncServiceFactory.ProcessUploadAsync(data, modelTypeName);
            } catch (Exception ex) {
                _logger.LogError($"[BackendSyncService] Erro ao processar upload de {modelTypeName}: {ex.Message}");
                throw;
            }
        }

        public async Task<UpdatesDTO> GetUpdatesAsync(DateTime lastSyncTime) {
            var updates = new UpdatesDTO();
            foreach (var entityType in _entityTypes) {
                try {
                    // Obtém o método genérico `GetUpdatesAsync` da fábrica.
                    var getUpdatesMethod = typeof(IBackendSyncServiceFactory).GetMethod("GetUpdatesAsync");
                    if (getUpdatesMethod == null) {
                        _logger.LogError("[BackendSyncService] Método GetUpdatesAsync não encontrado na fábrica.");
                        continue;
                    }

                    var dtoType = Type.GetType($"ArenaVirtualAPI.DTOs.{entityType}SyncDto");
                    if (dtoType == null) {
                        _logger.LogWarning($"[BackendSyncService] DTO de sincronização para {entityType} não encontrado.");
                        continue;
                    }

                    var genericMethod = getUpdatesMethod.MakeGenericMethod(dtoType);
                    var updatedItems = await (Task<IEnumerable<ISyncableDto>>)genericMethod.Invoke(_syncServiceFactory, new object[] { entityType, lastSyncTime });

                    if (updatedItems != null && updatedItems.Any()) {
                        var jsonElement = JsonSerializer.SerializeToElement(updatedItems);
                        updates.UpdatedItems.Add(entityType, jsonElement);
                    }
                } catch (Exception ex) {
                    _logger.LogError($"Erro ao obter atualizações para o tipo {entityType}: {ex.Message}");
                }
            }
            return updates;
        }
    }
}