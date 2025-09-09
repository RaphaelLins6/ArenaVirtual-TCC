using ArenaVirtualAPI.DTOs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ArenaVirtualAPI.Services {
    public class BackendSyncService {
        private readonly ILogger<BackendSyncService> _logger;
        private readonly IBackendSyncServiceFactory _syncServiceFactory;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly List<string> _entityTypes = new() {
            "Usuario", "Campeonato", "Time", "Convite", "UsuarioCampeonatoFavorito"
        };

        public BackendSyncService(ILogger<BackendSyncService> logger, IBackendSyncServiceFactory syncServiceFactory) {
            _logger = logger;
            _syncServiceFactory = syncServiceFactory;
            _jsonSerializerOptions = new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true,
            };
        }

        public async Task<Dictionary<Guid, int>> ProcessAndMapItemsAsync(JsonElement data, string modelTypeName) {
            try {
                var dtoType = Type.GetType($"ArenaVirtualAPI.DTOs.{modelTypeName}SyncDto");
                if (dtoType == null) {
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

                dynamic result = genericMethod.Invoke(_syncServiceFactory, new object[] { items, modelTypeName });
                return await result;
            } catch (Exception ex) {
                _logger.LogError($"[BackendSyncService] Erro ao processar upload de {modelTypeName}: {ex.Message}");
                throw;
            }
        }

        public async Task ProcessAllUploadsAsync(AllUploadsDto data) {
            _logger.LogInformation("[BackendSyncService] Iniciando processamento de todos os uploads.");

            // A ordem é importante para resolver as dependências
            if (data.Usuarios != null) {
                await _syncServiceFactory.ProcessAndMapItemsAsync<UsuarioSyncDto>(data.Usuarios, "Usuario");
            }

            if (data.Campeonatos != null) {
                await _syncServiceFactory.ProcessAndMapItemsAsync<CampeonatoSyncDto>(data.Campeonatos, "Campeonato");
            }

            if (data.Times != null) {
                await _syncServiceFactory.ProcessAndMapItemsAsync<TimeSyncDto>(data.Times, "Time");
            }

            if (data.Convites != null) {
                await _syncServiceFactory.ProcessAndMapItemsAsync<ConviteSyncDto>(data.Convites, "Convite");
            }

            if (data.UsuarioCampeonatoFavoritos != null) {
                await _syncServiceFactory.ProcessAndMapItemsAsync<UsuarioCampeonatoFavoritoSyncDto>(data.UsuarioCampeonatoFavoritos, "UsuarioCampeonatoFavorito");
            }

            _logger.LogInformation("[BackendSyncService] Processamento de uploads concluído.");
        }

        // --- INÍCIO DO CÓDIGO CORRIGIDO ---
        public async Task<UpdatesDTO> GetUpdatesAsync(DateTime lastSyncTime) {
            var updates = new UpdatesDTO();
            foreach (var entityType in _entityTypes) {
                try {
                    IEnumerable<ISyncableDto> updatedItems = entityType switch {
                        "Usuario" => await _syncServiceFactory.GetUpdatesAsync<UsuarioSyncDto>(entityType, lastSyncTime),
                        "Campeonato" => await _syncServiceFactory.GetUpdatesAsync<CampeonatoSyncDto>(entityType, lastSyncTime),
                        "Time" => await _syncServiceFactory.GetUpdatesAsync<TimeSyncDto>(entityType, lastSyncTime),
                        "Convite" => await _syncServiceFactory.GetUpdatesAsync<ConviteSyncDto>(entityType, lastSyncTime),
                        "UsuarioCampeonatoFavorito" => await _syncServiceFactory.GetUpdatesAsync<UsuarioCampeonatoFavoritoSyncDto>(entityType, lastSyncTime),
                        _ => null
                    };

                    if (updatedItems != null && updatedItems.Any()) {
                        updates.UpdatedItems.Add(entityType, JsonSerializer.SerializeToElement(updatedItems));
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