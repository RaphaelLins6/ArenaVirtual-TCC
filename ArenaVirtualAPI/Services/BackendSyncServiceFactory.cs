using ArenaVirtualAPI.DTOs;
using ArenaVirtualAPI.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ArenaVirtualAPI.Services {
    public class BackendSyncServiceFactory : IBackendSyncServiceFactory {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackendSyncServiceFactory> _logger;

        private readonly Dictionary<string, (Type DtoType, Type EntityType)> _typeMappings = new() {
            { "Usuario", (typeof(UsuarioSyncDto), typeof(Usuario)) },
            { "Campeonato", (typeof(CampeonatoSyncDto), typeof(Campeonato)) },
            { "Time", (typeof(TimeSyncDto), typeof(Time)) },
            { "Convite", (typeof(ConviteSyncDto), typeof(Convite)) },
            { "UsuarioCampeonatoFavorito", (typeof(UsuarioCampeonatoFavoritoSyncDto), typeof(UsuarioCampeonatoFavorito)) }
        };

        public BackendSyncServiceFactory(IServiceProvider serviceProvider, ILogger<BackendSyncServiceFactory> logger) {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task<Dictionary<Guid, int>> ProcessAndMapItemsAsync<T>(IEnumerable<T> items, string entityType) where T : ISyncableDto {
            if (items == null || !items.Any()) {
                return new Dictionary<Guid, int>();
            }

            if (!_typeMappings.TryGetValue(entityType, out var types)) {
                throw new ArgumentException($"Tipo de entidade '{entityType}' não suportado.");
            }

            var serviceType = typeof(IBackendService<,>).MakeGenericType(types.EntityType, types.DtoType);
            dynamic service = _serviceProvider.GetRequiredService(serviceType);

            return await service.ProcessAndMapItemsAsync(items);
        }

        public async Task UpdateForeignKeysAsync<T>(IEnumerable<T> items, string entityType, Dictionary<string, Dictionary<Guid, int>> idMappings) where T : ISyncableDto {
            if (items == null || !items.Any()) {
                return;
            }

            if (!_typeMappings.TryGetValue(entityType, out var types)) {
                _logger.LogWarning($"Tipo de entidade '{entityType}' não suportado. Pulando atualização de chaves.");
                return;
            }

            var serviceType = typeof(IBackendService<,>).MakeGenericType(types.EntityType, types.DtoType);
            dynamic service = _serviceProvider.GetRequiredService(serviceType);

            await service.UpdateForeignKeysAsync(items, idMappings);
        }

        public async Task<IEnumerable<T>> GetUpdatesAsync<T>(string entityType, DateTime lastSyncTime) where T : ISyncableDto {
            if (!_typeMappings.TryGetValue(entityType, out var types)) {
                throw new ArgumentException($"Tipo de entidade '{entityType}' não suportado.");
            }

            var serviceType = typeof(IBackendService<,>).MakeGenericType(types.EntityType, types.DtoType);
            dynamic service = _serviceProvider.GetRequiredService(serviceType);

            return await service.GetUpdatedSinceAsync(lastSyncTime);
        }
    }
}