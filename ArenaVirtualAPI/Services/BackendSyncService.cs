using ArenaVirtualAPI.Data;
using ArenaVirtualAPI.DTOs;
using ArenaVirtualAPI.Models;
using ArenaVirtualAPI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

public class BackendSyncService {
    private readonly ILogger<BackendSyncService> _logger;
    private readonly IServiceProvider _serviceProvider;

    private readonly string[] _uploadOrder = new string[] {
        "Usuario",
        "Campeonato",
        "Time",
        "Partida",
        "AvaliacaoArbitro",
        "CampanhaPatrocinio",
        "Estatistica",
        "Jogo",
        "PropostaPatrocinio",
        "UsuarioCampeonatoFavorito",
        "Convite"
    };

    private readonly string[] _downloadOrder = new string[] {
        "Convite",
        "UsuarioCampeonatoFavorito",
        "PropostaPatrocinio",
        "Jogo",
        "Estatistica",
        "CampanhaPatrocinio",
        "AvaliacaoArbitro",
        "Partida",
        "Time",
        "Campeonato",
        "Usuario"
    };

    public BackendSyncService(ILogger<BackendSyncService> logger, IServiceProvider serviceProvider) {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task<Dictionary<string, Dictionary<Guid, int>>> ProcessAllUploadsAsync(AllUploadsDto data) {
        _logger.LogInformation("[BackendSyncService] Iniciando processamento de todos os uploads.");
        var idMappings = new Dictionary<string, Dictionary<Guid, int>>();

        foreach (var entityType in _uploadOrder) {
            try {
                var prop = typeof(AllUploadsDto).GetProperty(entityType + "s");
                if (prop == null) continue;
                var items = prop.GetValue(data);
                if (items == null || ((System.Collections.ICollection)items).Count == 0) continue;

                var modelType = GetModelType(entityType);
                var dtoType = GetDtoType(entityType);

                if (modelType == null || dtoType == null) {
                    _logger.LogWarning($"[BackendSyncService] Tipos de modelo ou DTO não encontrados para a entidade {entityType}.");
                    continue;
                }

                var serviceType = typeof(IBackendService<,>).MakeGenericType(modelType, dtoType);
                var service = _serviceProvider.GetService(serviceType);

                if (service == null) {
                    _logger.LogError($"[BackendSyncService] Serviço não encontrado para o tipo {serviceType.Name}.");
                    continue;
                }

                var processMethod = service.GetType().GetMethod("ProcessAndMapItemsAsync");
                if (processMethod == null) {
                    _logger.LogError($"[BackendSyncService] Método ProcessAndMapItemsAsync não encontrado no serviço para {entityType}.");
                    continue;
                }

                var mappingTask = (Task)processMethod.Invoke(service, new object[] { items, idMappings });
                await mappingTask;
                var mapping = (Dictionary<Guid, int>)((dynamic)mappingTask).Result;

                if (mapping.Count > 0) {
                    idMappings[entityType] = mapping;
                }
            } catch (Exception ex) {
                _logger.LogError($"[BackendSyncService] Erro ao processar upload de {entityType}: {ex.Message}");
                _logger.LogError(ex.StackTrace);
                throw;
            }
        }
        _logger.LogInformation("[BackendSyncService] Processamento de uploads concluído.");
        return idMappings;
    }

    public async Task<AllUpdatesDto> GetUpdatesAsync(DateTime lastSyncTime) {
        _logger.LogInformation($"[BackendSyncService] Iniciando busca por atualizações desde: {lastSyncTime.ToUniversalTime()}");
        var updates = new AllUpdatesDto();

        foreach (var entityType in _downloadOrder) {
            try {
                var modelType = GetModelType(entityType);
                var dtoType = GetDtoType(entityType);
                if (modelType == null || dtoType == null) {
                    _logger.LogWarning($"[BackendSyncService] Tipos de modelo ou DTO não encontrados para a entidade {entityType}.");
                    continue;
                }
                var serviceType = typeof(IBackendService<,>).MakeGenericType(modelType, dtoType);
                var service = _serviceProvider.GetService(serviceType);
                if (service == null) {
                    _logger.LogError($"[BackendSyncService] Serviço não encontrado para o tipo {serviceType.Name}.");
                    continue;
                }

                var getUpdatesMethod = service.GetType().GetMethod("GetUpdatedSinceAsync");
                if (getUpdatesMethod == null) {
                    _logger.LogError($"[BackendSyncService] Método GetUpdatedSinceAsync não encontrado no serviço para {entityType}.");
                    continue;
                }
                var updatesTask = (Task)getUpdatesMethod.Invoke(service, new object[] { lastSyncTime });
                await updatesTask;

                var updatedItems = ((dynamic)updatesTask).Result;

                var prop = typeof(AllUpdatesDto).GetProperty(entityType + "s");
                if (prop != null) {
                    prop.SetValue(updates, updatedItems);
                }
            } catch (Exception ex) {
                _logger.LogError($"[BackendSyncService] Erro ao buscar atualizações para {entityType}: {ex.Message}");
                _logger.LogError(ex.StackTrace);
                throw;
            }
        }
        _logger.LogInformation("[BackendSyncService] Busca por atualizações concluída.");
        return updates;
    }

    private Type? GetModelType(string entityName) {
        return Type.GetType($"ArenaVirtualAPI.Models.{entityName}");
    }

    private Type? GetDtoType(string entityName) {
        return Type.GetType($"ArenaVirtualAPI.DTOs.{entityName}SyncDto");
    }
}