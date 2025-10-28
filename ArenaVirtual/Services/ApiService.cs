using ArenaVirtual.Models;
using System;
using System.Collections;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ArenaVirtual.Services {
    public class ApiService {
        private readonly HttpClient _httpClient;

        private const string AzureBaseUrl = "https://arenavirtualapi-cvghgbcgdqfbdhha.canadacentral-01.azurewebsites.net/";

        public ApiService() {
            Console.WriteLine($"[ApiService] Ambiente Nuvem detectado → usando Azure: {AzureBaseUrl}");

            var handler = new HttpClientHandler();

            _httpClient = new HttpClient(handler) {
                BaseAddress = new Uri(AzureBaseUrl)
            };

            Console.WriteLine($"[ApiService] Base URL configurada: {_httpClient.BaseAddress}");
        }

        public async Task<T?> PostAsync<T>(string endpoint, object data) where T : class {
            try {
                var url = endpoint;
                var jsonContent = JsonSerializer.Serialize(data);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                if (!string.IsNullOrEmpty(SessaoService.Instancia.Token)) {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", SessaoService.Instancia.Token);
                } else {
                    _httpClient.DefaultRequestHeaders.Authorization = null;
                }

                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode) {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[ApiService] Sucesso PostAsync para {endpoint}: {jsonResponse}");
                    return JsonSerializer.Deserialize<T>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                Debug.WriteLine($"[ApiService] Falha PostAsync para {endpoint}. Status: {response.StatusCode}");
                return null;

            } catch (Exception ex) {
                Debug.WriteLine($"[ApiService] Erro Exception PostAsync para {endpoint}: {ex.Message}");
                return null;
            }
        }

        public async Task<Dictionary<string, Dictionary<string, int>>> PostDataAsync<T>(string typeName, T data) {
            try {
                var response = await _httpClient.PostAsJsonAsync("api/data/sync", data);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[ApiService] JSON recebido de {typeName}: {responseContent}");

                return JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, int>>>(
                    responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true } 
                );
            } catch (HttpRequestException ex) {
                Console.WriteLine($"[ApiService] Falha na requisição (UPLOAD {typeName}): {ex.Message}");
                throw;
            }
        }

        public async Task<UpdateResponse?> GetAllUpdatesAsync(DateTime lastSyncTime) {
            try {
                var response = await _httpClient.GetAsync($"api/data/updates?lastSyncTime={lastSyncTime:o}");
                response.EnsureSuccessStatusCode();
                var jsonResponse = await response.Content.ReadAsStringAsync();

                Debug.WriteLine($"[ApiService] JSON de atualizações recebido: {jsonResponse}");

                return JsonSerializer.Deserialize<UpdateResponse>(
                    jsonResponse,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
            } catch (HttpRequestException ex) {
                Console.WriteLine($"[ApiService] Falha na requisição (DOWNLOAD): {ex.Message}");
                throw;
            }
        }

        private int GetItemCount<T>(T data) {
            if (data is ICollection collection) {
                return collection.Count;
            }
            return 1;
        }
    }

    public class UpdateResponse {
        public Dictionary<string, JsonElement> UpdatedItems { get; set; } = new();
    }
}
