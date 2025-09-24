using ArenaVirtual.DTOs;
using ArenaVirtual.Models;
using ArenaVirtual.Services;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

public class SyncService {
    private readonly DatabaseService _databaseService;
    private readonly ApiService _apiService;
    private bool _isSyncing = false;
    private Timer? _syncTimer;

    private readonly Type[] _uploadOrder = new Type[] {
        typeof(Usuario),
        typeof(Campeonato),
        typeof(Time),
        typeof(Partida),
        typeof(AvaliacaoArbitro),
        typeof(CampanhaPatrocinio),
        typeof(Estatistica),
        typeof(Jogo),
        typeof(PropostaPatrocinio),
        typeof(UsuarioCampeonatoFavorito),
        typeof(Convite)
    };

    public SyncService(DatabaseService databaseService, ApiService apiService) {
        _databaseService = databaseService;
        _apiService = apiService;
    }

    public void ScheduleSync() {
        _syncTimer?.Dispose();
        _syncTimer = new Timer(async (e) => {
            _syncTimer?.Dispose();
            await SyncAsync(new Progress<string>());
        }, null, 2000, Timeout.Infinite);
    }

    public async Task SyncAsync(IProgress<string> progress) {
        if (_isSyncing) return;
        _isSyncing = true;
        try {
            progress?.Report("Iniciando sincronização...");
            progress?.Report("Enviando dados para o servidor...");
            await UploadChangesAsync(progress);
            progress?.Report("Recebendo dados do servidor...");
            await DownloadChangesAsync(progress);
            progress?.Report("Sincronização concluída.");
        } catch (Exception ex) {
            Debug.WriteLine($"[SyncService] Erro na sincronização: {ex.Message}");
            progress?.Report($"Erro: {ex.Message}");
        } finally {
            _isSyncing = false;
        }
    }

    private async Task UploadChangesAsync(IProgress<string> progress) {
        var allUploads = new AllUploadsDto();
        var allItems = new Dictionary<Type, IList>();
        var requiredDependencies = new HashSet<Guid>();

        // 1. Coletar todos os itens não sincronizados e suas dependências
        foreach (var type in _uploadOrder) {
            var unsyncedItems = await GetUnsyncedItemsWithTypeAsync(type);

            if (unsyncedItems.Count > 0) {
                allItems[type] = unsyncedItems;

                // Lógica para adicionar dependências de usuário
                if (type == typeof(Campeonato)) {
                    foreach (dynamic item in unsyncedItems) {
                        requiredDependencies.Add(item.OrganizadorClientAppId);
                    }
                }
                if (type == typeof(Time)) {
                    foreach (dynamic item in unsyncedItems) {
                        if (item.CapitaoClientAppId != null) {
                            requiredDependencies.Add(item.CapitaoClientAppId);
                        }
                    }
                }
            }
        }

        // 2. Coletar as dependências que não foram incluídas
        if (requiredDependencies.Count > 0) {
            var userItems = await _databaseService.GetItemsByClientAppIdsAsync<Usuario>(requiredDependencies);
            if (userItems.Any()) {
                if (allItems.ContainsKey(typeof(Usuario))) {
                    foreach (var user in userItems) {
                        if (!((List<Usuario>)allItems[typeof(Usuario)]).Any(u => u.ClientAppId == user.ClientAppId)) {
                            allItems[typeof(Usuario)].Add(user);
                        }
                    }
                } else {
                    allItems[typeof(Usuario)] = userItems;
                }
            }
        }

        if (!allItems.Any()) {
            progress?.Report("Nenhum dado para enviar.");
            return;
        }

        // 3. Criar e popular os DTOs para envio, respeitando a ordem de upload
        foreach (var type in _uploadOrder) {
            if (allItems.TryGetValue(type, out var itemsToUpload)) {
                var syncDtos = CreateSyncDtos(itemsToUpload, type);
                var prop = typeof(AllUploadsDto).GetProperty($"{type.Name}s");
                prop?.SetValue(allUploads, syncDtos);
            }
        }

        // 4. Enviar a requisição para o servidor
        progress?.Report("Enviando todos os dados para o servidor...");
        var idMappings = await _apiService.PostDataAsync("AllUploads", allUploads);

        // 5. Atualizar os IDs locais (agora com validação e tratamento de erros)
        foreach (var type in _uploadOrder) {
            if (idMappings.TryGetValue(type.Name, out var mappingRaw)) {
                // AQUI: Usando 'as' para uma conversão segura para o tipo que esperamos
                var mapping = mappingRaw as IDictionary<Guid, object>;

                if (mapping != null && allItems.TryGetValue(type, out var itemsToUpdate)) {
                    foreach (var item in itemsToUpdate.Cast<ISyncable>()) {
                        if (mapping.TryGetValue(item.ClientAppId, out object rawValue)) {
                            int serverId = 0;

                            // A lógica de verificação foi reescrita para evitar o erro de compilação
                            if (rawValue is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Number) {
                                serverId = jsonElement.GetInt32();
                            } else if (rawValue is int intValue) {
                                serverId = intValue;
                            } else {
                                Debug.WriteLine($"[SyncService] Valor inesperado no mapeamento da entidade {type.Name}, ClientAppId={item.ClientAppId}: {rawValue}");
                                continue;
                            }

                            if (serverId == 0) {
                                Debug.WriteLine($"[SyncService] Ignorando mapeamento inválido (ID=0) para {type.Name}, ClientAppId={item.ClientAppId}");
                                continue;
                            }

                            await _databaseService.UpdateIdAndMarkAsSyncedAsync((dynamic)item, serverId);
                        }
                    }
                } else {
                    Debug.WriteLine($"[SyncService] Mapeamento para {type.Name} não é um IDictionary<Guid, object>. Tipo recebido: {mappingRaw?.GetType().Name ?? "null"}");
                }
            }
        }
    }

    private IList CreateSyncDtos(IList items, Type itemType) {
        var dtoType = Type.GetType($"ArenaVirtual.DTOs.{itemType.Name}SyncDto");
        if (dtoType == null) {
            Debug.WriteLine($"[SyncService] DTO não encontrado para o tipo {itemType.Name}.");
            return new List<object>();
        }

        var listType = typeof(List<>).MakeGenericType(dtoType);
        var syncDtos = (IList)Activator.CreateInstance(listType);

        foreach (var item in items) {
            var dto = Activator.CreateInstance(dtoType);

            // Apenas copia as propriedades do modelo para o DTO.
            // O mapeamento de FKs agora é responsabilidade do backend.
            foreach (var prop in itemType.GetProperties()) {
                var dtoProp = dtoType.GetProperty(prop.Name);
                if (dtoProp != null && dtoProp.CanWrite) {
                    var value = prop.GetValue(item);
                    dtoProp.SetValue(dto, value);
                }
            }

            // Tratamento especial para o 'CapitaoClientAppId' no 'Time'
            if (itemType == typeof(Time)) {
                var timeItem = (Time)item;
                dtoType.GetProperty("CapitaoClientAppId")?.SetValue(dto, timeItem.CapitaoClientAppId);
            }

            dtoType.GetProperty("IsSynced")?.SetValue(dto, true);
            syncDtos.Add(dto);
        }
        return syncDtos;
    }

    private async Task DownloadChangesAsync(IProgress<string> progress) {
        var lastSyncTime = Preferences.Get("LastSyncTime", DateTime.MinValue);
        progress?.Report("Buscando todas as atualizações no servidor...");

        var updates = await _apiService.GetAllUpdatesAsync(lastSyncTime);
        if (updates == null || updates.UpdatedItems == null) {
            progress?.Report("Nenhuma atualização encontrada no servidor.");
            return;
        }

        foreach (var type in _uploadOrder) {
            var typeName = type.Name;
            if (updates.UpdatedItems.TryGetValue(typeName, out var jsonElement)) {
                var rawJson = jsonElement.GetRawText();
                var listType = typeof(List<>).MakeGenericType(type);
                var items = JsonSerializer.Deserialize(rawJson, listType, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (items is IList collection && collection.Count > 0) {
                    progress?.Report($"Atualizando {typeName}...");
                    var saveMethod = typeof(DatabaseService).GetMethod("SaveDownloadedItemsAsync", BindingFlags.Public | BindingFlags.Instance);
                    if (saveMethod == null) {
                        Debug.WriteLine($"[SyncService] Método SaveDownloadedItemsAsync não encontrado para o tipo {type.Name}.");
                        continue;
                    }

                    var genericSaveMethod = saveMethod.MakeGenericMethod(type);
                    await (Task)genericSaveMethod.Invoke(_databaseService, new object[] { items });
                }
            }
        }
        Preferences.Set("LastSyncTime", DateTime.UtcNow);
        progress?.Report("Sincronização de download concluída.");
    }

    private async Task<IList> GetUnsyncedItemsWithTypeAsync(Type type) {
        var getMethod = typeof(DatabaseService).GetMethod("GetUnsyncedItemsAsync", BindingFlags.Public | BindingFlags.Instance);
        if (getMethod == null) {
            Debug.WriteLine($"[SyncService] Método GetUnsyncedItemsAsync não encontrado para o tipo {type.Name}.");
            return new List<object>();
        }

        var genericGetMethod = getMethod.MakeGenericMethod(type);
        var unsyncedItemsTask = (Task)genericGetMethod.Invoke(_databaseService, null);
        await unsyncedItemsTask;
        return (IList)((dynamic)unsyncedItemsTask).Result;
    }
}