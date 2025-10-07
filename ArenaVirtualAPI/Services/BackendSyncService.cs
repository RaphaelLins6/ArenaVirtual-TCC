using ArenaVirtualAPI.Data;
using ArenaVirtualAPI.DTOs;
using System.Text.Json;

namespace ArenaVirtualAPI.Services {
    public class BackendSyncService {
        private readonly ApiDbContext _context;
        private readonly IBackendSyncServiceFactory _syncServiceFactory;
        private readonly ILogger<BackendSyncService> _logger;

        public BackendSyncService(ApiDbContext context, IBackendSyncServiceFactory syncServiceFactory, ILogger<BackendSyncService> logger) {
            _context = context;
            _syncServiceFactory = syncServiceFactory;
            _logger = logger;
        }

        public async Task<AllUpdatesDto> SyncDataAsync(AllUploadsDto data, DateTime lastSyncTime) {
            var allUpdates = new AllUpdatesDto();
            await ProcessAllUploadsAsync(data);
            await GetUpdatesFromAllEntitiesAsync(allUpdates, lastSyncTime);
            return allUpdates;
        }

        private async Task ProcessAllUploadsAsync(AllUploadsDto data) {
            _logger.LogInformation("[BackendSyncService] Iniciando processamento de uploads em etapas sequenciais.");
            var idMappings = new Dictionary<string, Dictionary<Guid, int>>();

            // NOVO: Define a ordem de processamento para garantir que as entidades principais existam antes das de relacionamento.
            var mainEntitiesOrder = new List<string>
            {
                "Usuario",
                "Campeonato",
                "Time",
                "Jogo",
                "Convite"
            };

            var relationshipEntitiesOrder = new List<string>
            {
                "UsuarioCampeonatoFavorito"
            };

            // --- FASE 1: Processa e mapeia IDs para entidades principais ---
            foreach (var entityName in mainEntitiesOrder) {
                // Usando reflexão para obter a lista de DTOs correspondente (ex: data.Usuarios)
                if (data.GetType().GetProperty($"{entityName}s")?.GetValue(data) is IEnumerable<ISyncableDto> dtoList && dtoList.Any()) {
                    idMappings[entityName] = await _syncServiceFactory.ProcessAndMapItemsAsync(dtoList, entityName);
                    _logger.LogInformation($"[BackendSyncService] Concluído o mapeamento de IDs para {entityName}.");
                }
            }

            _logger.LogInformation("[BackendSyncService] Entidades primárias adicionadas ao contexto. Iniciando a resolução de chaves estrangeiras.");

            // --- FASE 2: Atualiza as chaves estrangeiras das entidades principais ---
            foreach (var entityName in mainEntitiesOrder) {
                if (data.GetType().GetProperty($"{entityName}s")?.GetValue(data) is IEnumerable<ISyncableDto> dtoList && dtoList.Any()) {
                    await _syncServiceFactory.UpdateForeignKeysAsync(dtoList, entityName, idMappings);
                }
            }

            // --- FASE 3: Processamento especial para entidades de relacionamento ---
            // Agora que todos os IDs principais estão mapeados, processamos as entidades de relacionamento.
            foreach (var entityName in relationshipEntitiesOrder) {
                if (data.GetType().GetProperty($"{entityName}s")?.GetValue(data) is IEnumerable<ISyncableDto> dtoList && dtoList.Any()) {
                    _logger.LogInformation($"[BackendSyncService] Processando entidade de relacionamento: {entityName}.");
                    // Este método agora deve CRIAR a entidade e resolver suas FKs de uma só vez.
                    await _syncServiceFactory.UpdateForeignKeysAsync(dtoList, entityName, idMappings);
                }
            }

            _logger.LogInformation("[BackendSyncService] Chaves estrangeiras resolvidas. Salvando todas as alterações.");

            // --- FASE 4: Salva todas as alterações pendentes no banco de dados. ---
            await _context.SaveChangesAsync();
            _logger.LogInformation("[BackendSyncService] Todas as alterações salvas no banco de dados.");
        }

        private async Task GetUpdatesFromAllEntitiesAsync(AllUpdatesDto allUpdates, DateTime lastSyncTime) {
            _logger.LogInformation("[BackendSyncService] Iniciando busca por atualizações.");

            var entityOrder = new List<string> {
                "Usuario", "Campeonato", "Time", "Jogo", "Convite", "UsuarioCampeonatoFavorito"
            };

            foreach (var entityName in entityOrder) {
                try {
                    var updates = await _syncServiceFactory.GetUpdatesAsync<ISyncableDto>(entityName, lastSyncTime);
                    if (updates != null && updates.Any()) {
                        // Serializa a lista de DTOs para um JsonElement que representa um array JSON.
                        var jsonElement = JsonSerializer.SerializeToElement(updates);
                        allUpdates.UpdatedItems[entityName] = jsonElement;

                        _logger.LogInformation($"[BackendSyncService] Encontradas {updates.Count()} atualizações para {entityName}.");
                    } else {
                        _logger.LogInformation($"[BackendSyncService] Nenhuma atualização encontrada para {entityName}.");
                    }
                } catch (Exception ex) {
                    _logger.LogError(ex, $"[BackendSyncService] Erro ao buscar atualizações para {entityName}: {ex.Message}");
                }
            }
        }

        public async Task<AllUpdatesDto> GetUpdatesAsync(DateTime lastSyncTime) {
            var allUpdates = new AllUpdatesDto();
            await GetUpdatesFromAllEntitiesAsync(allUpdates, lastSyncTime);
            return allUpdates;
        }
    }
}
