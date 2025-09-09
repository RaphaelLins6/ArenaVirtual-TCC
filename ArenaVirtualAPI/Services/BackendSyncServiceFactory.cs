using ArenaVirtualAPI.DTOs;
using ArenaVirtualAPI.Models;

namespace ArenaVirtualAPI.Services {
    public class BackendSyncServiceFactory : IBackendSyncServiceFactory {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, (Type DtoType, Type EntityType)> _typeMappings = new() {
            { "Usuario", (typeof(UsuarioSyncDto), typeof(Usuario)) },
            { "Campeonato", (typeof(CampeonatoSyncDto), typeof(Campeonato)) },
            { "Time", (typeof(TimeSyncDto), typeof(Time)) },
            { "Convite", (typeof(ConviteSyncDto), typeof(Convite)) },
            { "UsuarioCampeonatoFavorito", (typeof(UsuarioCampeonatoFavoritoSyncDto), typeof(UsuarioCampeonatoFavorito)) }
        };

        public BackendSyncServiceFactory(IServiceProvider serviceProvider) {
            _serviceProvider = serviceProvider;
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

            // O método na interface IBackendService já retorna Dictionary<Guid, int>,
            // então a chamada aqui está correta.
            return await service.ProcessAndMapItemsAsync(items);
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