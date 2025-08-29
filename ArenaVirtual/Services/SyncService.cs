using ArenaVirtual.Services;
using ArenaVirtual.Models;
using System.Threading.Tasks;
using System;
using System.Reflection;
using Microsoft.Maui.ApplicationModel;

public class SyncService {
    private readonly DatabaseService _databaseService;
    private readonly ApiService _apiService;

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
        progress.Report("Iniciando sincronização...");

        // 1. Sincronização de Envio (Upload)
        progress.Report("Enviando dados para o servidor...");
        await UploadChangesAsync(progress); // Passe o objeto de progresso aqui

        // 2. Sincronização de Recebimento (Download)
        progress.Report("Recebendo dados do servidor...");
        await DownloadChangesAsync(progress); // Passe o objeto de progresso aqui

        progress.Report("Sincronização concluída.");
    }

    private async Task UploadChangesAsync(IProgress<string> progress) { // Adicione o argumento aqui
        foreach (var type in _syncableTypes) {
            progress.Report($"Enviando dados de {type.Name}..."); // Mensagem mais específica
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

    private async Task DownloadChangesAsync(IProgress<string> progress) { // Adicione o argumento aqui
        var lastSyncTime = Preferences.Get("LastSyncTime", DateTime.MinValue);

        foreach (var type in _syncableTypes) {
            progress.Report($"Atualizando {type.Name}...");
            var getUpdatesMethod = typeof(ApiService).GetMethod("GetUpdatesAsync");
            if (getUpdatesMethod == null) continue;
            var genericGetUpdatesMethod = getUpdatesMethod.MakeGenericMethod(type);

            dynamic latestItems = await (dynamic)genericGetUpdatesMethod.Invoke(_apiService, new object[] { lastSyncTime });

            var saveMethod = typeof(DatabaseService).GetMethod("SaveDownloadedItemsAsync");
            if (saveMethod == null) continue;
            var genericSaveMethod = saveMethod.MakeGenericMethod(type);
            await (Task)genericSaveMethod.Invoke(_databaseService, new object[] { latestItems });
        }

        Preferences.Set("LastSyncTime", DateTime.UtcNow);
    }
}