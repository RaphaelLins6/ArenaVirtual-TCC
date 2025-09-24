using ArenaVirtualAPI.Data;
using ArenaVirtualAPI.DTOs;
using ArenaVirtualAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;

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
            _logger.LogInformation("[BackendSyncService] Iniciando processamento de uploads em duas etapas.");
            var idMappings = new Dictionary<string, Dictionary<Guid, int>>();

            var phase1Entities = new List<(string Name, dynamic Dtos)> {
                ("Usuario", data.Usuarios),
                ("Campeonato", data.Campeonatos),
                ("Time", data.Times)
            };

            var phase2Entities = new List<(string Name, dynamic Dtos)> {
                ("Convite", data.Convites),
                ("UsuarioCampeonatoFavorito", data.UsuarioCampeonatoFavoritos)
            };

            // Etapa 1: Processamento inicial (upsert) para entidades primárias
            foreach (var entity in phase1Entities) {
                if (entity.Dtos != null) {
                    try {
                        var newMappings = await _syncServiceFactory.ProcessAndMapItemsAsync(entity.Dtos, entity.Name);
                        if (newMappings.Count > 0) {
                            idMappings[entity.Name] = newMappings;
                            _logger.LogInformation($"[BackendSyncService] Concluído o mapeamento de IDs para {entity.Name}.");
                        }
                    } catch (Exception ex) {
                        _logger.LogError(ex, $"[BackendSyncService] Erro ao processar upload de {entity.Name} na Etapa 1: {ex.Message}");
                        throw;
                    }
                }
            }

            // Salva todas as alterações da Etapa 1 em um único lote.
            await _context.SaveChangesAsync();
            _logger.LogInformation("[BackendSyncService] Entidades da Etapa 1 salvas no banco de dados.");

            // Etapa 2: Atualização de chaves estrangeiras para entidades secundárias
            foreach (var entity in phase2Entities) {
                if (entity.Dtos != null) {
                    try {
                        await _syncServiceFactory.UpdateForeignKeysAsync(entity.Dtos, entity.Name, idMappings);
                        _logger.LogInformation($"[BackendSyncService] Concluído o processamento de {entity.Name} na Etapa 2.");
                    } catch (Exception ex) {
                        _logger.LogError(ex, $"[BackendSyncService] Erro ao processar upload de {entity.Name} na Etapa 2: {ex.Message}");
                        throw;
                    }
                }
            }

            // Salva todas as alterações da Etapa 2 em um único lote.
            await _context.SaveChangesAsync();
            _logger.LogInformation("[BackendSyncService] Entidades da Etapa 2 salvas no banco de dados.");
        }

        private async Task GetUpdatesFromAllEntitiesAsync(AllUpdatesDto allUpdates, DateTime lastSyncTime) {
            _logger.LogInformation("[BackendSyncService] Iniciando busca por atualizações.");
            var entityOrder = new List<string> {
                "Usuario", "Campeonato", "Time", "Convite", "UsuarioCampeonatoFavorito"
            };

            foreach (var entityName in entityOrder) {
                try {
                    var updates = await _syncServiceFactory.GetUpdatesAsync<ISyncableDto>(entityName, lastSyncTime);
                    if (updates != null && updates.Any()) {
                        var jsonUpdates = updates.Select(u => JsonSerializer.SerializeToElement(u)).ToArray();
                        var jsonArray = JsonDocument.Parse($"[{string.Join(",", jsonUpdates.Select(e => e.ToString()))}]").RootElement;
                        allUpdates.UpdatedItems[entityName] = jsonArray;
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