// Services/ApiService.cs
using System.Text;
using System.Text.Json;
using ArenaVirtual.Models;
using System.Diagnostics;
using Microsoft.Maui.Devices;

public class ApiService {
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public ApiService() {
        _httpClient = new HttpClient();

#if DEBUG
        string debugIp = "192.168.15.13";
        string debugPort = "5067";

        // A URL base deve ser apenas o IP e a porta
        _baseUrl = $"http://{debugIp}:{debugPort}";

#else
        _baseUrl = "https://sua-url-de-producao-da-api.com";
#endif
        _httpClient.BaseAddress = new Uri(_baseUrl);
        Debug.WriteLine($"[ApiService] Base URL configurada para: {_baseUrl}");
    }

    // Método para enviar dados para a API (Upload)
    public async Task PostDataAsync<T>(List<T> items) where T : ISyncable {
        var json = JsonSerializer.Serialize(items);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try {
            // ** AQUI ESTÁ A CORREÇÃO **
            // Adicione o nome do tipo do modelo na URL para a rota correta
            var typeName = typeof(T).Name;
            var url = $"api/data/sync/{typeName}";
            Debug.WriteLine($"[ApiService] Enviando dados para: {url}");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            Debug.WriteLine($"[ApiService] Dados de {typeName} enviados com sucesso. Status: {response.StatusCode}");
        } catch (HttpRequestException ex) {
            Debug.WriteLine($"[ApiService] Erro ao enviar dados: {ex.Message}");
            throw;
        }
    }

    // Método para buscar dados atualizados da API (Download)
    public async Task<List<T>> GetUpdatesAsync<T>(DateTime lastSyncTime) where T : ISyncable {
        try {
            // ** AQUI ESTÁ A CORREÇÃO **
            // A rota de download na API é "api/data/updates", não apenas "api/data"
            var url = $"api/data/updates?lastSyncTime={lastSyncTime.ToString("o")}";
            Debug.WriteLine($"[ApiService] Buscando atualizações da URL: {url}");

            var response = await _httpClient.GetStringAsync(url);

            // Certifique-se de que a desserialização para List<T> funciona
            // O backend deve retornar apenas o tipo T para que isso funcione
            return JsonSerializer.Deserialize<List<T>>(response);
        } catch (HttpRequestException ex) {
            Debug.WriteLine($"[ApiService] Erro ao buscar dados: {ex.Message}");
            return new List<T>();
        } catch (JsonException ex) {
            Debug.WriteLine($"[ApiService] Erro de desserialização JSON: {ex.Message}");
            return new List<T>();
        }
    }
}