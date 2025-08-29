using ArenaVirtualAPI.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ArenaVirtualAPI.Services {
    // A fábrica agora implementa a nova interface.
    public class BackendSyncServiceFactory : IBackendSyncServiceFactory {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Type> _entityTypes = new()
        {
            { "Usuario", typeof(Usuario) },
            { "Campeonato", typeof(Campeonato) },
            { "Time", typeof(Time) },
            { "Convite", typeof(Convite) }
        };

        public BackendSyncServiceFactory(IServiceProvider serviceProvider) {
            _serviceProvider = serviceProvider;
        }

        public async Task ProcessUploadAsync(JsonElement data, string entityType) {
            if (!_entityTypes.TryGetValue(entityType, out var entityTypeObject)) {
                throw new ArgumentException($"Tipo de entidade '{entityType}' não suportado.");
            }

            // Usa o 'switch' para lidar com a desserialização e o processamento de forma segura.
            switch (entityType) {
                case "Usuario":
                    var usuarios = JsonSerializer.Deserialize<List<Usuario>>(data.GetRawText());
                    await _serviceProvider.GetRequiredService<IBackendService<Usuario>>().ProcessItemsAsync(usuarios);
                    break;
                case "Campeonato":
                    var campeonatos = JsonSerializer.Deserialize<List<Campeonato>>(data.GetRawText());
                    await _serviceProvider.GetRequiredService<IBackendService<Campeonato>>().ProcessItemsAsync(campeonatos);
                    break;
                case "Time":
                    var times = JsonSerializer.Deserialize<List<Time>>(data.GetRawText());
                    await _serviceProvider.GetRequiredService<IBackendService<Time>>().ProcessItemsAsync(times);
                    break;
                case "Convite":
                    var convites = JsonSerializer.Deserialize<List<Convite>>(data.GetRawText());
                    await _serviceProvider.GetRequiredService<IBackendService<Convite>>().ProcessItemsAsync(convites);
                    break;
                default:
                    throw new ArgumentException($"Tipo de entidade '{entityType}' não suportado para upload.");
            }
        }

        public async Task<IEnumerable<ISyncable>> GetUpdatesAsync(string entityType, DateTime lastSyncTime) {
            switch (entityType) {
                case "Usuario":
                    return await _serviceProvider.GetRequiredService<IBackendService<Usuario>>().GetUpdatedSinceAsync(lastSyncTime);
                case "Campeonato":
                    return await _serviceProvider.GetRequiredService<IBackendService<Campeonato>>().GetUpdatedSinceAsync(lastSyncTime);
                case "Time":
                    return await _serviceProvider.GetRequiredService<IBackendService<Time>>().GetUpdatedSinceAsync(lastSyncTime);
                case "Convite":
                    return await _serviceProvider.GetRequiredService<IBackendService<Convite>>().GetUpdatedSinceAsync(lastSyncTime);
                default:
                    throw new ArgumentException($"Tipo de entidade '{entityType}' não suportado.");
            }
        }
    }
}
