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
            _logger.LogInformation("[BackendSyncService] Iniciando processamento de uploads em etapas sequenciais.");
            var idMappings = new Dictionary<string, Dictionary<Guid, int>>();

            // Fase 1: Processa e mapeia IDs para entidades primárias ou independentes
            // As entidades de `Campeonatos` e `Times` agora são processadas aqui.
            if (data.Usuarios != null) {
                idMappings["Usuario"] = await _syncServiceFactory.ProcessAndMapItemsAsync(data.Usuarios, "Usuario");
                _logger.LogInformation("[BackendSyncService] Concluído o mapeamento de IDs para Usuario.");
            }
            if (data.Times != null) {
                idMappings["Time"] = await _syncServiceFactory.ProcessAndMapItemsAsync(data.Times, "Time");
                _logger.LogInformation("[BackendSyncService] Concluído o mapeamento de IDs para Time.");
            }
            if (data.Campeonatos != null) {
                idMappings["Campeonato"] = await _syncServiceFactory.ProcessAndMapItemsAsync(data.Campeonatos, "Campeonato");
                _logger.LogInformation("[BackendSyncService] Concluído o mapeamento de IDs para Campeonato.");
            }

            // As entidades `Convites` e `UsuarioCampeonatoFavoritos` também precisam ter seu ID mapeado.
            if (data.Convites != null) {
                idMappings["Convite"] = await _syncServiceFactory.ProcessAndMapItemsAsync(data.Convites, "Convite");
                _logger.LogInformation("[BackendSyncService] Concluído o mapeamento de IDs para Convite.");
            }
            if (data.UsuarioCampeonatoFavoritos != null) {
                idMappings["UsuarioCampeonatoFavorito"] = await _syncServiceFactory.ProcessAndMapItemsAsync(data.UsuarioCampeonatoFavoritos, "UsuarioCampeonatoFavorito");
                _logger.LogInformation("[BackendSyncService] Concluído o mapeamento de IDs para UsuarioCampeonatoFavorito.");
            }

            // A chamada para `_context.SaveChangesAsync()` é removida daqui.

            _logger.LogInformation("[BackendSyncService] Entidades primárias adicionadas ao contexto. Iniciando a resolução de chaves estrangeiras.");

            // Fase 2: Atualiza as chaves estrangeiras
            if (data.Usuarios != null) {
                await _syncServiceFactory.UpdateForeignKeysAsync(data.Usuarios, "Usuario", idMappings);
            }
            if (data.Times != null) {
                await _syncServiceFactory.UpdateForeignKeysAsync(data.Times, "Time", idMappings);
            }
            if (data.Campeonatos != null) {
                await _syncServiceFactory.UpdateForeignKeysAsync(data.Campeonatos, "Campeonato", idMappings);
            }
            if (data.Convites != null) {
                await _syncServiceFactory.UpdateForeignKeysAsync(data.Convites, "Convite", idMappings);
            }
            if (data.UsuarioCampeonatoFavoritos != null) {
                await _syncServiceFactory.UpdateForeignKeysAsync(data.UsuarioCampeonatoFavoritos, "UsuarioCampeonatoFavorito", idMappings);
            }

            _logger.LogInformation("[BackendSyncService] Chaves estrangeiras resolvidas. Salvando todas as alterações.");

            // Fase 3: Salva todas as alterações pendentes no banco de dados.
            await _context.SaveChangesAsync();
            _logger.LogInformation("[BackendSyncService] Todas as alterações salvas no banco de dados.");
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