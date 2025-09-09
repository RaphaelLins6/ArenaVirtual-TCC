using ArenaVirtual.DTOs;
using ArenaVirtual.Models;
using ArenaVirtual.Services;
using Microsoft.Maui.Storage;
using SQLite;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

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
        var idMapping = new Dictionary<Type, Dictionary<Guid, int>>();

        foreach (var type in _uploadOrder) {
            progress?.Report($"Enviando dados de {type.Name}...");
            var getMethod = typeof(DatabaseService).GetMethod("GetUnsyncedItemsAsync", BindingFlags.Public | BindingFlags.Instance);
            if (getMethod == null) {
                Debug.WriteLine($"[SyncService] Método GetUnsyncedItemsAsync não encontrado para o tipo {type.Name}.");
                continue;
            }

            var genericGetMethod = getMethod.MakeGenericMethod(type);
            var unsyncedItemsTask = (Task)genericGetMethod.Invoke(_databaseService, null);
            await unsyncedItemsTask;
            var unsyncedItems = (IList)((dynamic)unsyncedItemsTask).Result;

            if (unsyncedItems.Count > 0) {
                var syncDtos = await CreateSyncDtos(unsyncedItems, type, idMapping);
                var postMethod = typeof(ApiService).GetMethod("PostDataAsync", BindingFlags.Public | BindingFlags.Instance);

                if (postMethod == null) {
                    Debug.WriteLine($"[SyncService] Método PostDataAsync não encontrado para o tipo {type.Name}.");
                    continue;
                }

                var genericPostMethod = postMethod.MakeGenericMethod(syncDtos.GetType());
                var postTask = (Task<Dictionary<Guid, int>>)genericPostMethod.Invoke(_apiService, new object[] { type.Name, syncDtos });
                var currentIdMapping = await postTask;
                idMapping[type] = currentIdMapping;

                foreach (var item in unsyncedItems.Cast<ISyncable>()) {
                    if (currentIdMapping.TryGetValue(item.ClientAppId, out int serverId)) {
                        await _databaseService.UpdateIdAndMarkAsSyncedAsync((dynamic)item, serverId);
                    }
                }
            }
        }
    }

    private async Task<IList> CreateSyncDtos(IList items, Type itemType, Dictionary<Type, Dictionary<Guid, int>> idMapping) {
        var dtoType = Type.GetType($"ArenaVirtual.DTOs.{itemType.Name}SyncDto");
        if (dtoType == null) {
            Debug.WriteLine($"[SyncService] DTO não encontrado para o tipo {itemType.Name}.");
            return new List<object>();
        }
        var listType = typeof(List<>).MakeGenericType(dtoType);
        var syncDtos = (IList)Activator.CreateInstance(listType);

        foreach (var item in items) {
            var dto = Activator.CreateInstance(dtoType);

            foreach (var prop in itemType.GetProperties()) {
                var dtoProp = dtoType.GetProperty(prop.Name);
                if (dtoProp != null && dtoProp.CanWrite) {
                    var value = prop.GetValue(item);
                    dtoProp.SetValue(dto, value);
                }
            }

            foreach (var mappingType in idMapping.Keys) {
                var fkPropName = $"{mappingType.Name}Id";
                var fkClientAppIdPropName = $"{mappingType.Name}ClientAppId";

                var fkProp = dtoType.GetProperty(fkPropName);
                var fkClientAppIdProp = itemType.GetProperty(fkClientAppIdPropName);

                if (fkProp != null && fkClientAppIdProp != null) {
                    var clientAppIdValue = fkClientAppIdProp.GetValue(item);
                    if (clientAppIdValue is Guid clientAppId && idMapping[mappingType].TryGetValue(clientAppId, out int serverId)) {
                        fkProp.SetValue(dto, serverId);
                    }
                }
            }

            if (itemType == typeof(Time)) {
                var timeItem = (Time)item;
                var userMapping = idMapping.GetValueOrDefault(typeof(Usuario));
                if (userMapping != null && timeItem.CapitaoId.HasValue) {
                    var usuarioLocalId = await _databaseService.GetUsuarioClientAppIdById(timeItem.CapitaoId.Value);
                    if (usuarioLocalId.HasValue && userMapping.TryGetValue(usuarioLocalId.Value, out int serverId)) {
                        dtoType.GetProperty("CapitaoId")?.SetValue(dto, serverId);
                    }
                }
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
}