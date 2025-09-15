using ArenaVirtualAPI.DTOs;
using System.Collections;
using System.Text.Json;

namespace ArenaVirtualAPI.Services {
    public class BackendSyncService {
        private readonly ILogger<BackendSyncService> _logger;
        private readonly IBackendSyncServiceFactory _syncServiceFactory;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly Dictionary<string, Type> _dtoTypes = new()
        {
            { "Usuario", typeof(UsuarioSyncDto) },
            { "Campeonato", typeof(CampeonatoSyncDto) },
            { "Time", typeof(TimeSyncDto) },
            { "Convite", typeof(ConviteSyncDto) },
            { "UsuarioCampeonatoFavorito", typeof(UsuarioCampeonatoFavoritoSyncDto) },
            // Adicione outros DTOs de sincronização aqui
        };

        private readonly List<string> _uploadOrder = new()
        {
            "Usuario",
            "Time", // Depende de Usuario (CapitaoId)
            "Campeonato", // Depende de Usuario (OrganizadorId)
            "Convite", // Depende de Usuario (IdSolicitante) e Time
            "UsuarioCampeonatoFavorito" // Depende de Usuario e Campeonato
        };

        public BackendSyncService(ILogger<BackendSyncService> logger, IBackendSyncServiceFactory syncServiceFactory) {
            _logger = logger;
            _syncServiceFactory = syncServiceFactory;
            _jsonSerializerOptions = new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true,
            };
        }

        public async Task ProcessAllUploadsAsync(AllUploadsDto data) {
            _logger.LogInformation("[BackendSyncService] Iniciando processamento de todos os uploads.");

            var allItems = new Dictionary<string, JsonElement>();
            var idMappings = new Dictionary<string, Dictionary<Guid, int>>();

            // Adicione todos os itens recebidos a um dicionário para fácil acesso
            if (data.Usuarios != null) allItems.Add("Usuario", JsonSerializer.SerializeToElement(data.Usuarios));
            if (data.Times != null) allItems.Add("Time", JsonSerializer.SerializeToElement(data.Times));
            if (data.Campeonatos != null) allItems.Add("Campeonato", JsonSerializer.SerializeToElement(data.Campeonatos));
            if (data.Convites != null) allItems.Add("Convite", JsonSerializer.SerializeToElement(data.Convites));
            if (data.UsuarioCampeonatoFavoritos != null) allItems.Add("UsuarioCampeonatoFavorito", JsonSerializer.SerializeToElement(data.UsuarioCampeonatoFavoritos));

            foreach (var entityType in _uploadOrder) {
                if (allItems.TryGetValue(entityType, out var jsonElement)) {
                    try {
                        var mapping = await ProcessAndMapItemsAsync(jsonElement, entityType, idMappings);
                        if (mapping.Count > 0) {
                            idMappings[entityType] = mapping;
                        }
                    } catch (Exception ex) {
                        _logger.LogError($"[BackendSyncService] Erro ao processar upload de {entityType}: {ex.Message}");
                        throw;
                    }
                }
            }
            _logger.LogInformation("[BackendSyncService] Processamento de uploads concluído.");
        }

        public async Task<Dictionary<Guid, int>> ProcessAndMapItemsAsync(JsonElement data, string modelTypeName, Dictionary<string, Dictionary<Guid, int>> idMappings) {
            if (!_dtoTypes.TryGetValue(modelTypeName, out var dtoType)) {
                _logger.LogWarning($"[BackendSyncService] DTO de sincronização para {modelTypeName} não encontrado.");
                return new Dictionary<Guid, int>();
            }

            var listType = typeof(List<>).MakeGenericType(dtoType);
            var items = JsonSerializer.Deserialize(data.GetRawText(), listType, _jsonSerializerOptions) as IList;

            if (items == null || items.Count == 0) {
                return new Dictionary<Guid, int>();
            }

            var method = _syncServiceFactory.GetType().GetMethod("ProcessAndMapItemsAsync");
            if (method == null) {
                throw new InvalidOperationException("Método 'ProcessAndMapItemsAsync' não encontrado na fábrica.");
            }

            var genericMethod = method.MakeGenericMethod(dtoType);

            // Adapta a chamada para passar o dicionário de mapeamentos
            // A ordem dos argumentos no Invoke deve corresponder à assinatura do método
            object[] args = (modelTypeName == "Usuario")
                ? new object[] { items }
                : new object[] { items, idMappings };

            dynamic result = genericMethod.Invoke(_syncServiceFactory, args);
            return await result;
        }

        // O método GetUpdatesAsync não foi modificado, pois o problema é no upload.
        public async Task<UpdatesDTO> GetUpdatesAsync(DateTime lastSyncTime) {
            var updates = new UpdatesDTO();
            foreach (var entityType in _uploadOrder) {
                try {
                    if (_dtoTypes.TryGetValue(entityType, out var dtoType)) {
                        var method = _syncServiceFactory.GetType().GetMethod("GetUpdatedSinceAsync");
                        var genericMethod = method.MakeGenericMethod(dtoType);

                        var task = (Task)genericMethod.Invoke(_syncServiceFactory, new object[] { lastSyncTime });
                        await task;

                        var resultProperty = task.GetType().GetProperty("Result");
                        if (resultProperty != null) {
                            var updatedItems = resultProperty.GetValue(task) as IEnumerable<ISyncableDto>;

                            if (updatedItems != null && updatedItems.Any()) {
                                updates.UpdatedItems.Add(entityType, JsonSerializer.SerializeToElement(updatedItems));
                            }
                        }
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