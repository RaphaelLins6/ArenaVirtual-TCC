using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using ArenaVirtual.Models;

public class ApiService {
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public ApiService() {
        _httpClient = new HttpClient();
        _jsonSerializerOptions = new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true // Apenas para facilitar debug
        };

#if DEBUG
        string debugIp = "192.168.15.13";
        string debugPort = "5067";
        _baseUrl = $"http://{debugIp}:{debugPort}";
#else
        _baseUrl = "https://sua-url-de-producao-da-api.com";
#endif
        _httpClient.BaseAddress = new Uri(_baseUrl);
        Debug.WriteLine($"[ApiService] Base URL configurada: {_baseUrl}");
    }

    // -----------------------------
    // Enviar dados (UPLOAD)
    // -----------------------------
    public async Task PostDataAsync<T>(List<T> items) where T : ISyncable {
        if (items == null || items.Count == 0) {
            Debug.WriteLine($"[ApiService] Nenhum item de {typeof(T).Name} para enviar.");
            return;
        }

        var typeName = typeof(T).Name;
        var url = $"api/data/sync/{typeName}";
        Debug.WriteLine($"[ApiService] Enviando {items.Count} itens de {typeName} → {url}");

        try {
            var response = await _httpClient.PostAsJsonAsync(url, items, _jsonSerializerOptions);
            response.EnsureSuccessStatusCode();
            Debug.WriteLine($"[ApiService] Upload de {typeName} concluído. Status: {response.StatusCode}");
        } catch (HttpRequestException ex) {
            Debug.WriteLine($"[ApiService] Falha na requisição (UPLOAD {typeName}): {ex.Message}");
            throw; // Propaga para SyncService lidar se precisar
        }
    }

    // -----------------------------
    // Buscar dados (DOWNLOAD)
    // -----------------------------
    public async Task<List<T>> GetUpdatesAsync<T>(DateTime lastSyncTime) where T : ISyncable {
        var typeName = typeof(T).Name;
        var url = $"api/data/updates?lastSyncTime={lastSyncTime:o}";
        Debug.WriteLine($"[ApiService] Buscando atualizações para {typeName} → {url}");

        try {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json)) {
                Debug.WriteLine($"[ApiService] Resposta vazia ao buscar {typeName}.");
                return new List<T>();
            }

            var root = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, _jsonSerializerOptions);
            if (root == null) {
                Debug.WriteLine($"[ApiService] JSON inválido ou vazio ao buscar {typeName}.");
                return new List<T>();
            }

            // Tenta localizar a chave com fallback: plural ou singular
            string keyPlural = typeName + "s";
            string keySingular = typeName;
            string selectedKey = root.ContainsKey(keyPlural) ? keyPlural :
                                 root.ContainsKey(keySingular) ? keySingular : null;

            if (selectedKey == null) {
                Debug.WriteLine($"[ApiService] Nenhum dado encontrado para {typeName} no JSON.");
                return new List<T>();
            }

            var rawArray = root[selectedKey].GetRawText();
            var items = JsonSerializer.Deserialize<List<T>>(rawArray, _jsonSerializerOptions) ?? new List<T>();

            Debug.WriteLine($"[ApiService] Recebidos {items.Count} itens de {typeName}.");
            return items;
        } catch (HttpRequestException ex) {
            Debug.WriteLine($"[ApiService] Falha na requisição (DOWNLOAD {typeName}): {ex.Message}");
            return new List<T>();
        } catch (JsonException ex) {
            Debug.WriteLine($"[ApiService] Erro ao desserializar JSON para {typeName}: {ex.Message}");
            return new List<T>();
        }
    }

    // NOVO MÉTODO: Obter todos os dados de uma só vez para o SyncService
    public async Task<UpdatesDTO> GetAllUpdatesAsync(DateTime lastSyncTime) {
        var url = $"api/data/updates?lastSyncTime={lastSyncTime:o}";
        Debug.WriteLine($"[ApiService] Buscando todas as atualizações → {url}");

        try {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json)) {
                Debug.WriteLine("[ApiService] Resposta vazia.");
                return new UpdatesDTO();
            }

            // AQUI ESTÁ O AJUSTE PRINCIPAL: desserializa diretamente para UpdatesDTO
            var updates = JsonSerializer.Deserialize<UpdatesDTO>(json, _jsonSerializerOptions);
            return updates ?? new UpdatesDTO();
        } catch (HttpRequestException ex) {
            Debug.WriteLine($"[ApiService] Falha na requisição: {ex.Message}");
            return new UpdatesDTO();
        } catch (JsonException ex) {
            Debug.WriteLine($"[ApiService] Erro ao desserializar UpdatesDTO: {ex.Message}");
            return new UpdatesDTO();
        }
    }
}