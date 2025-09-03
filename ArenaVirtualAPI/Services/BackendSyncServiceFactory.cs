using ArenaVirtualAPI.Models;
using ArenaVirtualAPI.DTOs;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections;
using System.Reflection;

namespace ArenaVirtualAPI.Services {
    public class BackendSyncServiceFactory : IBackendSyncServiceFactory {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, (Type DtoType, Type EntityType)> _typeMappings = new()
        {
            { "Usuario", (typeof(UsuarioSyncDto), typeof(Usuario)) },
            { "Campeonato", (typeof(CampeonatoSyncDto), typeof(Campeonato)) },
            { "Time", (typeof(TimeSyncDto), typeof(Time)) },
            { "Convite", (typeof(ConviteSyncDto), typeof(Convite)) },
            { "UsuarioCampeonatoFavorito", (typeof(UsuarioCampeonatoFavoritoSyncDto), typeof(UsuarioCampeonatoFavorito)) }
        };

        public BackendSyncServiceFactory(IServiceProvider serviceProvider) {
            _serviceProvider = serviceProvider;
        }

        public async Task ProcessUploadAsync(JsonElement data, string entityType) {
            if (!_typeMappings.TryGetValue(entityType, out var types)) {
                throw new ArgumentException($"Tipo de entidade '{entityType}' não suportado.");
            }

            var listType = typeof(List<>).MakeGenericType(types.DtoType);
            var items = (IList)JsonSerializer.Deserialize(data.GetRawText(), listType, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (items == null || items.Count == 0) {
                return;
            }

            var serviceType = typeof(IBackendService<,>).MakeGenericType(types.EntityType, types.DtoType);
            var service = _serviceProvider.GetRequiredService(serviceType);

            var processMethod = serviceType.GetMethod("ProcessItemsAsync");
            if (processMethod == null) {
                throw new InvalidOperationException($"Método 'ProcessItemsAsync' não encontrado no serviço '{serviceType}'.");
            }
            await (Task)processMethod.Invoke(service, new object[] { items });
        }

        public async Task<IEnumerable<ISyncable>> GetUpdatesAsync(string entityType, DateTime lastSyncTime) {
            if (!_typeMappings.TryGetValue(entityType, out var types)) {
                throw new ArgumentException($"Tipo de entidade '{entityType}' não suportado.");
            }

            var serviceType = typeof(IBackendService<,>).MakeGenericType(types.EntityType, types.DtoType);
            dynamic service = _serviceProvider.GetRequiredService(serviceType);

            return await service.GetUpdatedSinceAsync(lastSyncTime);
        }
    }
}