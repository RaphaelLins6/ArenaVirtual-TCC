using ArenaVirtualAPI.DTOs;
using System.Collections;
using System.Text.Json;

namespace ArenaVirtualAPI.Services {
    public class BackendSyncService {
        private readonly ILogger<BackendSyncService> _logger;
        private readonly IBackendSyncServiceFactory _syncServiceFactory;

        // Adicione UsuarioCampeonatoFavorito à lista
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

        public async Task ProcessUploadAsync(JsonElement data, string modelTypeName) {
            try {
                var type = Type.GetType($"ArenaVirtualAPI.Models.{modelTypeName}");
                if (type == null) {
                    _logger.LogWarning($"[BackendSyncService] Modelo {modelTypeName} não encontrado na API. Ignorando upload.");
                    return;
                }

                var dtoType = Type.GetType($"ArenaVirtualAPI.DTOs.{modelTypeName}SyncDto");
                if (dtoType == null) {
                    _logger.LogWarning($"[BackendSyncService] DTO de sincronização para {modelTypeName} não encontrado. Ignorando upload.");
                    return;
                }

                var listType = typeof(List<>).MakeGenericType(dtoType);
                var items = (ICollection)JsonSerializer.Deserialize(data.GetRawText(), listType, _jsonSerializerOptions);

                if (items == null || items.Count == 0) {
                    _logger.LogInformation($"[BackendSyncService] Nenhum item para processar em {modelTypeName}");
                    return;
                }

                _logger.LogInformation($"[BackendSyncService] Recebidos {items.Count} itens de {modelTypeName}");
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
                    var updatedItems = await _syncServiceFactory.GetUpdatesAsync(entityType, lastSyncTime);

                    if (updatedItems != null && updatedItems.Any()) {
                        var jsonElement = JsonSerializer.SerializeToElement(updatedItems);
                        // A ordem dos argumentos está correta aqui:
                        updates.UpdatedItems.Add(entityType, jsonElement);
                    }
                } catch (Exception ex) {
                    _logger.LogError($"Erro ao obter atualizações para o tipo {entityType}: {ex.Message}");
                    // Lembre-se de não dar 'throw' em um loop de processamento
                    // de itens. Isso impedirá que os outros itens sejam processados.
                }
            }
            return updates;
        }
    }
}
