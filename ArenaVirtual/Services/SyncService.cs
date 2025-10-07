using ArenaVirtual.DTOs;
using ArenaVirtual.Models;
using ArenaVirtual.Services;
using System.Collections;
using System.Diagnostics;
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
        typeof(Jogo),
        typeof(Convite),
        typeof(UsuarioCampeonatoFavorito)
    };

    private readonly Dictionary<string, Type> _downloadDtoMap = new Dictionary<string, Type>
    {
        { "Usuario", typeof(UsuarioDownloadDto) },
        { "Campeonato", typeof(CampeonatoDownloadDto) },
        { "Time", typeof(TimeDownloadDto) },
        { "Convite", typeof(ConviteDownloadDto) },
        { "UsuarioCampeonatoFavorito", typeof(UsuarioCampeonatoFavoritoDownloadDto) },
        { "Jogo", typeof(JogoDownloadDto) }
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
            await UploadChangesAsync(progress);
            await DownloadChangesAsync(progress);
            progress?.Report("Sincronização concluída.");
        } catch (Exception ex) {
            // O erro de GUID do JSON será capturado aqui, se persistir,
            // mas agora é tratado pelo ApiService e UploadChangesAsync.
            Debug.WriteLine($"[SyncService] Erro na sincronização: {ex.Message}");
            Debug.WriteLine($"[SyncService] StackTrace: {ex.StackTrace}");
            progress?.Report($"Erro: {ex.Message}");
        } finally {
            _isSyncing = false;
        }
    }

    private async Task UploadChangesAsync(IProgress<string> progress) {
        progress?.Report("Enviando dados para o servidor...");
        var allUploads = new AllUploadsDto();
        var allItems = new Dictionary<Type, IList>();

        foreach (var type in _uploadOrder) {
            var unsyncedItems = await GetUnsyncedItemsWithTypeAsync(type);
            if (unsyncedItems.Count > 0) {
                allItems[type] = unsyncedItems;
            }
        }

        if (!allItems.Any()) {
            progress?.Report("Nenhum dado para enviar.");
            return;
        }

        foreach (var type in _uploadOrder) {
            if (allItems.TryGetValue(type, out var itemsToUpload)) {
                var syncDtos = CreateSyncDtos(itemsToUpload, type);
                var prop = typeof(AllUploadsDto).GetProperty($"{type.Name}s");
                if (prop != null) {
                    prop.SetValue(allUploads, syncDtos);
                }
            }
        }

        progress?.Report("Enviando todos os dados para o servidor...");
        // O tipo de retorno de PostDataAsync foi alterado para Dictionary<string, Dictionary<string, int>>
        var idMappings = await _apiService.PostDataAsync("AllUploads", allUploads);

        if (idMappings == null) return;

        foreach (var type in _uploadOrder) {
            if (idMappings.TryGetValue(type.Name, out var mappingRaw)) {
                // CORRIGIDO: Verifica se o tipo é o novo Dictionary<string, int>
                if (mappingRaw is IDictionary<string, int> mapping && allItems.TryGetValue(type, out var itemsToUpdate)) {
                    foreach (var item in itemsToUpdate.Cast<ISyncable>()) {
                        // CORRIGIDO: Usa a string do GUID como chave para buscar o ServerId
                        if (mapping.TryGetValue(item.ClientAppId.ToString(), out int serverId)) {
                            // A lógica de conversão complexa foi removida, pois 'serverId' já é um int.
                            if (serverId > 0) {
                                await _databaseService.UpdateIdAndMarkAsSyncedAsync((dynamic)item, serverId);
                            }
                        }
                    }
                }
            }
        }
    }

    private IList CreateSyncDtos(IList items, Type itemType) {
        var dtoType = Type.GetType($"ArenaVirtual.DTOs.{itemType.Name}SyncDto");
        if (dtoType == null) {
            Debug.WriteLine($"[SyncService] DTO de Sincronização não encontrado para o tipo {itemType.Name}.");
            return new List<object>();
        }

        var listType = typeof(List<>).MakeGenericType(dtoType);
        var syncDtos = (IList)Activator.CreateInstance(listType);

        foreach (var item in items) {
            if (item is UsuarioCampeonatoFavorito favorito && (favorito.UsuarioClientAppId == Guid.Empty || favorito.CampeonatoClientAppId == Guid.Empty)) {
                continue;
            }

            var dto = Activator.CreateInstance(dtoType);

            foreach (var prop in itemType.GetProperties()) {
                var dtoProp = dtoType.GetProperty(prop.Name);

                if (dtoProp != null && dtoProp.CanWrite) {
                    var originalValue = prop.GetValue(item);
                    var targetType = dtoProp.PropertyType;
                    var nonNullableTargetType = Nullable.GetUnderlyingType(targetType) ?? targetType;

                    if (originalValue == null) {
                        // Atribui null diretamente.
                        dtoProp.SetValue(dto, null);
                        continue;
                    }

                    // --- INÍCIO DA CORREÇÃO PARA PLACARES 'X' e 'Y' ---
                    if (prop.PropertyType == typeof(string) && (nonNullableTargetType == typeof(int) || nonNullableTargetType == typeof(double))) {

                        // Se o valor de origem é String e o destino é numérico, tentamos a conversão.
                        // Se falhar (e.g., por ser 'X' ou 'Y'), garantimos o valor '0'.
                        if (!double.TryParse(originalValue.ToString(), out double placarNumerico)) {
                            // Se não for um número válido (como 'X' ou 'Y'), assume 0 para sincronizar.
                            originalValue = 0;
                        } else {
                            originalValue = placarNumerico;
                        }
                    }
                    // --- FIM DA CORREÇÃO ---


                    try {
                        // 1. Se os tipos são idênticos ou compatíveis, copia diretamente.
                        if (prop.PropertyType == targetType || (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))) {
                            dtoProp.SetValue(dto, originalValue);
                        }
                        // 2. Tenta usar Convert.ChangeType para forçar a conversão (agora com PlacarA/B já corrigidos para 0 ou o número real).
                        else {
                            var convertedValue = Convert.ChangeType(originalValue, nonNullableTargetType);
                            dtoProp.SetValue(dto, convertedValue);
                        }
                    } catch (Exception ex) {
                        // Logamos o erro exato de conversão.
                        Debug.WriteLine($"[!!! ERRO REFLECTION !!!] Falha ao copiar propriedade '{prop.Name}' de '{itemType.Name}' para DTO '{dtoType.Name}'.");
                        Debug.WriteLine($" -> Tipos: Origem: {prop.PropertyType.Name} | Destino: {targetType.Name}. Valor: '{originalValue}'");
                        Debug.WriteLine($" -> Erro: {ex.Message}");
                        // Continuamos o loop para tentar a próxima propriedade
                    }
                }
            }
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

        foreach (var entry in _downloadDtoMap) {
            var typeName = entry.Key;
            var dtoType = entry.Value;

            if (updates.UpdatedItems.TryGetValue(typeName, out var jsonElement)) {
                try {
                    var listType = typeof(List<>).MakeGenericType(dtoType);
                    var dtoList = (IList)JsonSerializer.Deserialize(jsonElement.GetRawText(), listType, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (dtoList != null && dtoList.Count > 0) {
                        progress?.Report($"Processando {dtoList.Count} atualizações para {typeName}...");

                        var saveMethodName = $"SaveDownloaded{typeName}sAsync";
                        var saveMethod = typeof(DatabaseService).GetMethod(saveMethodName);

                        if (saveMethod != null) {
                            await (Task)saveMethod.Invoke(_databaseService, new object[] { dtoList });
                        } else {
                            Debug.WriteLine($"[SyncService] Método de salvamento '{saveMethodName}' não encontrado no DatabaseService.");
                        }
                    }
                } catch (JsonException jsonEx) {
                    // Este é o bloco que captura erros de Guid no download (se houver)
                    Debug.WriteLine($"[SyncService] Erro de JSON ao processar {typeName}: {jsonEx.Message}");
                    Debug.WriteLine($"[SyncService] JSON para {typeName}: {jsonElement.GetRawText()}");
                }
            }
        }

        Preferences.Set("LastSyncTime", DateTime.UtcNow);
        progress?.Report("Sincronização de download concluída.");
    }

    private async Task<IList> GetUnsyncedItemsWithTypeAsync(Type type) {
        var getMethod = typeof(DatabaseService).GetMethod("GetUnsyncedItemsAsync").MakeGenericMethod(type);
        var task = (Task)getMethod.Invoke(_databaseService, null);
        await task;
        return (IList)((dynamic)task).Result;
    }

    public async Task SyncData() {
        await SyncAsync(new Progress<string>());
    }
}