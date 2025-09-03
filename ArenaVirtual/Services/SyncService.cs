using ArenaVirtual.Services;
using ArenaVirtual.Models;
using System.Text.Json;
using System.Collections;
using ArenaVirtual.DTOs;

public class SyncService {
    private readonly DatabaseService _databaseService;
    private readonly ApiService _apiService;
    private bool _isSyncing = false;

    private readonly Type[] _syncableTypes = new Type[] {
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
            progress?.Report("Enviando dados para o servidor...");
            await UploadChangesAsync(progress);
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
            var unsyncedItemsTask = (Task)genericGetMethod.Invoke(_databaseService, null);
            await unsyncedItemsTask;
            var unsyncedItems = (IList)((dynamic)unsyncedItemsTask).Result;

            if (unsyncedItems.Count > 0) {
                object syncDtos;
                Type dtoType;

                if (type == typeof(Usuario)) {
                    var items = unsyncedItems.Cast<Usuario>().ToList();
                    syncDtos = items.Select(item => new UsuarioSyncDto {
                        Id = item.Id,
                        Nome = item.Nome,
                        Email = item.Email,
                        Perfil = item.Perfil,
                        ImagemPath = item.ImagemPath,
                        Localizacao = item.Localizacao,
                        Telefone = item.Telefone,
                        LinkRedeSocial = item.LinkRedeSocial,
                        DataNascimento = item.DataNascimento,
                        Genero = item.Genero,
                        NomeEmpresa = item.NomeEmpresa,
                        CNPJ = item.CNPJ,
                        Peso = item.Peso,
                        Altura = item.Altura,
                        FaixaOrcamentoPatrocinio = item.FaixaOrcamentoPatrocinio,
                        TimeId = item.TimeId,
                        UpdatedAt = item.UpdatedAt,
                        IsSynced = true
                    }).ToList();
                    dtoType = typeof(List<UsuarioSyncDto>);
                } else if (type == typeof(Campeonato)) {
                    var items = unsyncedItems.Cast<Campeonato>().ToList();
                    syncDtos = items.Select(item => new CampeonatoSyncDto {
                        Id = item.Id,
                        Nome = item.Nome,
                        Local = item.Local,
                        DataInicio = item.DataInicio,
                        DataFim = item.DataFim,
                        OrganizadorId = item.OrganizadorId,
                        LogoUrl = item.LogoUrl,
                        NomeOrganizador = item.NomeOrganizador,
                        EmailOrganizador = item.EmailOrganizador,
                        TelefoneOrganizador = item.TelefoneOrganizador,
                        NumeroMaximoEquipes = item.NumeroMaximoEquipes,
                        ValorTaxaInscricao = item.ValorTaxaInscricao,
                        FormatoCampeonato = item.FormatoCampeonato,
                        LocaisDosJogos = item.LocaisDosJogos,
                        HaveraPremiacao = item.HaveraPremiacao,
                        UpdatedAt = item.UpdatedAt,
                        Descricao = item.Descricao,
                        Modalidade = item.Modalidade,
                        Regras = item.Regras,
                        DataTermino = item.DataTermino,
                        NumeroEquipes = item.NumeroEquipes,
                        IsSynced = true
                    }).ToList();
                    dtoType = typeof(List<CampeonatoSyncDto>);
                } else if (type == typeof(Time)) {
                    var items = unsyncedItems.Cast<Time>().ToList();
                    syncDtos = items.Select(item => new TimeSyncDto {
                        Id = item.Id,
                        Nome = item.Nome,
                        LogoUrl = item.LogoUrl,
                        CampeonatoId = item.CampeonatoId,
                        Descricao = item.Descricao,
                        DataCriacao = item.DataCriacao,
                        Regiao = item.Regiao,
                        PontuacaoTotal = item.PontuacaoTotal,
                        Vitorias = item.Vitorias,
                        Derrotas = item.Derrotas,
                        Empates = item.Empates,
                        CapitaoId = item.CapitaoId,
                        UpdatedAt = item.UpdatedAt,
                        IsSynced = true
                    }).ToList();
                    dtoType = typeof(List<TimeSyncDto>);
                } else if (type == typeof(Convite)) {
                    var items = unsyncedItems.Cast<Convite>().ToList();
                    syncDtos = items.Select(item => new ConviteSyncDto {
                        Id = item.Id,
                        IdSolicitante = item.IdSolicitante,
                        TimeId = item.TimeId,
                        DataEnvio = item.DataEnvio,
                        StatusConvite = item.Status,
                        ConvidadoEmail = item.ConvidadoEmail,
                        UpdatedAt = item.UpdatedAt,
                        IsSynced = true
                    }).ToList();
                    dtoType = typeof(List<ConviteSyncDto>);
                } else {
                    syncDtos = unsyncedItems;
                    dtoType = unsyncedItems.GetType();
                }

                var postMethod = typeof(ApiService).GetMethod("PostDataAsync");
                if (postMethod == null) continue;
                var genericPostMethod = postMethod.MakeGenericMethod(dtoType);

                await (Task)genericPostMethod.Invoke(_apiService, new object[] { type.Name, syncDtos });

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

        var updates = await _apiService.GetAllUpdatesAsync(lastSyncTime);

        foreach (var type in _syncableTypes) {
            var typeName = type.Name;

            if (updates.UpdatedItems.TryGetValue(typeName, out var jsonElement)) {
                var rawJson = jsonElement.GetRawText();
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

        Preferences.Set("LastSyncTime", DateTime.UtcNow);
        progress?.Report("Sincronização de download concluída.");
    }
}
