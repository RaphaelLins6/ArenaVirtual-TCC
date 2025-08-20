// Services/ApiService.cs
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json; // Importe este namespace
using System.Text;
using System.Text.Json;
using ArenaVirtual.Models;
using System.Diagnostics;

public class ApiService {
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly JsonSerializerOptions _jsonSerializerOptions; // Adicione a propriedade

    public ApiService() {
        _httpClient = new HttpClient();
        _jsonSerializerOptions = new JsonSerializerOptions {
            // O padrão já é o ISO 8601, mas podemos ser explícitos se necessário
            WriteIndented = true // Apenas para debug
        };

#if DEBUG
        string debugIp = "192.168.15.13";
        string debugPort = "5067";
        _baseUrl = $"http://{debugIp}:{debugPort}";
#else
        _baseUrl = "https://sua-url-de-producao-da-api.com";
#endif
        _httpClient.BaseAddress = new Uri(_baseUrl);
        Debug.WriteLine($"[ApiService] Base URL configurada para: {_baseUrl}");
    }

    // Método para enviar dados para a API (Upload)
    public async Task PostDataAsync<T>(List<T> items) where T : ISyncable {
        try {
            var typeName = typeof(T).Name;
            var url = $"api/data/sync/{typeName}";
            Debug.WriteLine($"[ApiService] Enviando dados para: {url}");

            // Use PostAsJsonAsync que já serializa e define o Content-Type
            var response = await _httpClient.PostAsJsonAsync(url, items);
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
            var url = $"api/data/updates?lastSyncTime={lastSyncTime.ToString("o")}";
            Debug.WriteLine($"[ApiService] Buscando atualizações da URL: {url}");

            // Use GetFromJsonAsync para desserializar corretamente
            var items = await _httpClient.GetFromJsonAsync<List<T>>(url, _jsonSerializerOptions);
            return items;

        } catch (HttpRequestException ex) {
            Debug.WriteLine($"[ApiService] Erro ao buscar dados: {ex.Message}");
            return new List<T>();
        } catch (JsonException ex) {
            Debug.WriteLine($"[ApiService] Erro de desserialização JSON: {ex.Message}");
            return new List<T>();
        }
    }
}
