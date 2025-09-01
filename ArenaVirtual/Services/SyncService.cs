using ArenaVirtual.Services;
using ArenaVirtual.Models;
using System.Threading.Tasks;
using System.Text.Json; // Importação necessária
using System.Diagnostics; // Importação necessária
using System.Linq; // Importação necessária

public class SyncService {
    private readonly DatabaseService _databaseService;
    private readonly ApiService _apiService;
    private bool _isSyncing = false;

    // Lista de tipos de dados que precisam de sincronização
    private readonly Type[] _syncableTypes = new Type[]
  {
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

    public async Task SyncAsync(IProgress<string> progress) {
        if (_isSyncing) return;

        _isSyncing = true;
        try {
            progress?.Report("Iniciando sincronização...");

            // 1. Sincronização de Envio (Upload)
            progress?.Report("Enviando dados para o servidor...");
            await UploadChangesAsync(progress);

            // 2. Sincronização de Recebimento (Download)
            progress?.Report("Recebendo dados do servidor...");
            await DownloadChangesAsync(progress);

            progress?.Report("Sincronização concluída.");
        } finally {
            _isSyncing = false;
        }
    }

    private async Task UploadChangesAsync(IProgress<string> progress) {
        foreach (var type in _syncableTypes) {
            progress?.Report($"Enviando dados de {type.Name}...");
            var getMethod = typeof(DatabaseService).GetMethod("GetUnsyncedItemsAsync");
            if (getMethod == null) continue;

            var genericGetMethod = getMethod.MakeGenericMethod(type);

            dynamic unsyncedItems = await (dynamic)genericGetMethod.Invoke(_databaseService, null);

            if (unsyncedItems.Count > 0) {
                var postMethod = typeof(ApiService).GetMethod("PostDataAsync");
                if (postMethod == null) continue;
                var genericPostMethod = postMethod.MakeGenericMethod(type);
                await (Task)genericPostMethod.Invoke(_apiService, new object[] { unsyncedItems });

                var markMethod = typeof(DatabaseService).GetMethod("MarkAsSyncedAsync");
                if (markMethod == null) continue;
                var genericMarkMethod = markMethod.MakeGenericMethod(type);
                await (Task)genericMarkMethod.Invoke(_databaseService, new object[] { unsyncedItems });
            }
        }
    }

    private async Task DownloadChangesAsync(IProgress<string> progress) {
        var lastSyncTime = Preferences.Get("LastSyncTime", DateTime.MinValue);
        progress?.Report("Buscando todas as atualizações no servidor...");

        // 1. Chame o novo método para obter todos os dados de uma vez
        var updates = await _apiService.GetAllUpdatesAsync(lastSyncTime);

        // 2. Itere sobre a sua lista de tipos de modelos para desserializar corretamente
        foreach (var type in _syncableTypes) {
            var typeName = type.Name;

            // Tenta encontrar a chave no dicionário da API
            if (updates.UpdatedItems.TryGetValue(typeName, out var jsonElement)) {
                var rawJson = jsonElement.GetRawText();

                // Desserializa a lista para o tipo correto
                var listType = typeof(List<>).MakeGenericType(type);
                var items = JsonSerializer.Deserialize(rawJson, listType, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (items != null && ((System.Collections.ICollection)items).Count > 0) {
                    progress?.Report($"Atualizando {typeName}...");
                    var saveMethod = typeof(DatabaseService).GetMethod("SaveDownloadedItemsAsync");
                    if (saveMethod == null) continue;

                    var genericSaveMethod = saveMethod.MakeGenericMethod(type);
                    await (Task)genericSaveMethod.Invoke(_databaseService, new object[] { items });
                }
            }
        }

        // 3. Atualize o timestamp da última sincronização.
        Preferences.Set("LastSyncTime", DateTime.UtcNow);
        progress?.Report("Sincronização de download concluída.");
    }
}