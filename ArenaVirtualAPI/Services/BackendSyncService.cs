using ArenaVirtualAPI.Dtos;
using System.Text.Json;

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
                    // O método da fábrica retorna uma lista de objetos (IEnumerable<ISyncable>).
                    var updatedItems = await _syncServiceFactory.GetUpdatesAsync(entityType, lastSyncTime);

                    // Verifique se a lista de itens não está vazia antes de serializar
                    if (updatedItems != null && updatedItems.Any()) {
                        // Serializa a lista de objetos para um JsonElement
                        var jsonElement = JsonSerializer.SerializeToElement(updatedItems);

                        // Adiciona o JsonElement ao dicionário
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