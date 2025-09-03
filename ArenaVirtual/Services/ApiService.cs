using System.Net.Http.Json;
using System.Text.Json;

namespace ArenaVirtual.Services {
    public class ApiService {
        private readonly HttpClient _httpClient;

        private const string BaseUrl = "https://localhost:7117/";

        public ApiService() {
            _httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
            Console.WriteLine($"[ApiService] Base URL configurada: {_httpClient.BaseAddress}");
        }

        public async Task PostDataAsync<T>(string typeName, T data) {
            try {
                Console.WriteLine($"[ApiService] Enviando {GetItemCount(data)} itens de {typeName} → api/data/sync/{typeName}");
                var response = await _httpClient.PostAsJsonAsync($"api/data/sync/{typeName}", data);
                response.EnsureSuccessStatusCode();
            } catch (HttpRequestException ex) {
                Console.WriteLine($"[ApiService] Falha na requisição (UPLOAD {typeName}): {ex.Message}");
                throw;
            }
        }

        public async Task<UpdateResponse> GetAllUpdatesAsync(DateTime lastSyncTime) {
            try {
                var response = await _httpClient.GetAsync($"api/data/updates?lastSyncTime={lastSyncTime:o}");
                response.EnsureSuccessStatusCode();
                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<UpdateResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            } catch (HttpRequestException ex) {
                Console.WriteLine($"[ApiService] Falha na requisição (DOWNLOAD): {ex.Message}");
                throw;
            }
        }

        private int GetItemCount<T>(T data) {
            if (data is System.Collections.ICollection collection) {
                return collection.Count;
            }
            return 1;
        }
    }

    public class UpdateResponse {
        public Dictionary<string, JsonElement> UpdatedItems { get; set; } = new Dictionary<string, JsonElement>();
    }
}
