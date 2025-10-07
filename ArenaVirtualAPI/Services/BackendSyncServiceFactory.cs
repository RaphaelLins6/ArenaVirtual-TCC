using ArenaVirtualAPI.DTOs;
using ArenaVirtualAPI.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace ArenaVirtualAPI.Services {
    public class BackendSyncServiceFactory : IBackendSyncServiceFactory {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackendSyncServiceFactory> _logger;

        private readonly Dictionary<string, (Type DtoType, Type EntityType)> _typeMappings = new()
        {
            { "Usuario", (typeof(UsuarioSyncDto), typeof(Usuario)) },
            { "Campeonato", (typeof(CampeonatoSyncDto), typeof(Campeonato)) },
            { "Time", (typeof(TimeSyncDto), typeof(Time)) },
            { "Jogo", (typeof(JogoSyncDto), typeof(Jogo)) },
            { "Convite", (typeof(ConviteSyncDto), typeof(Convite)) },
            { "UsuarioCampeonatoFavorito", (typeof(UsuarioCampeonatoFavoritoSyncDto), typeof(UsuarioCampeonatoFavorito)) }
        };

        public BackendSyncServiceFactory(IServiceProvider serviceProvider, ILogger<BackendSyncServiceFactory> logger) {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        // CORREÇÃO: O método implementa a interface com <T>, mas usa reflexão para chamar o método correto.
        public async Task<Dictionary<Guid, int>> ProcessAndMapItemsAsync<T>(IEnumerable<T> items, string entityType) where T : ISyncableDto {
            if (items == null || !items.Any()) {
                return new Dictionary<Guid, int>();
            }

            if (!_typeMappings.TryGetValue(entityType, out var types)) {
                throw new ArgumentException($"Tipo de entidade '{entityType}' não suportado.");
            }

            // Obtém o serviço específico (ex: UsuarioService)
            object service = _serviceProvider.GetRequiredService(typeof(IBackendService<,>).MakeGenericType(types.EntityType, types.DtoType));

            // Encontra o método "ProcessAndMapItemsAsync" no serviço
            var method = service.GetType().GetMethod("ProcessAndMapItemsAsync");
            if (method == null) {
                throw new InvalidOperationException($"Método 'ProcessAndMapItemsAsync' não encontrado no serviço para a entidade '{entityType}'.");
            }

            // Invoca o método usando reflexão. O binder da reflexão consegue lidar com a conversão de tipos da lista.
            var task = (Task<Dictionary<Guid, int>>)method.Invoke(service, new object[] { items });
            return await task;
        }

        // CORREÇÃO: Mesma lógica de reflexão aplicada aqui.
        public async Task UpdateForeignKeysAsync<T>(IEnumerable<T> items, string entityType, Dictionary<string, Dictionary<Guid, int>> idMappings) where T : ISyncableDto {
            if (items == null || !items.Any()) {
                return;
            }

            if (!_typeMappings.TryGetValue(entityType, out var types)) {
                _logger.LogWarning($"Tipo de entidade '{entityType}' não suportado. Pulando atualização de chaves.");
                return;
            }

            object service = _serviceProvider.GetRequiredService(typeof(IBackendService<,>).MakeGenericType(types.EntityType, types.DtoType));
            var method = service.GetType().GetMethod("UpdateForeignKeysAsync");
            if (method == null) {
                throw new InvalidOperationException($"Método 'UpdateForeignKeysAsync' não encontrado no serviço para a entidade '{entityType}'.");
            }

            var task = (Task)method.Invoke(service, new object[] { items, idMappings });
            await task;
        }

        public async Task<IEnumerable<T>> GetUpdatesAsync<T>(string entityType, DateTime lastSyncTime) where T : ISyncableDto {
            if (!_typeMappings.TryGetValue(entityType, out var types)) {
                throw new ArgumentException($"Tipo de entidade '{entityType}' não suportado.");
            }

            var serviceType = typeof(IBackendService<,>).MakeGenericType(types.EntityType, types.DtoType);
            dynamic service = _serviceProvider.GetRequiredService(serviceType);

            // A chamada com 'dynamic' funciona aqui porque o tipo de retorno já é genérico e não há ambiguidade.
            var result = await service.GetUpdatedSinceAsync(lastSyncTime);

            // O resultado já é do tipo correto (IEnumerable<TDto>), que pode ser convertido para IEnumerable<T>
            return (IEnumerable<T>)result;
        }
    }
}
