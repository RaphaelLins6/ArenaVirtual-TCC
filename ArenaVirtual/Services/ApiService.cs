using System.Collections;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace ArenaVirtual.Services {
    public class ApiService {
        private readonly HttpClient _httpClient;

        public ApiService() {
#if ANDROID
                Console.WriteLine("[ApiService] Ambiente Android detectado → usando 10.0.2.2");

                var handler = new HttpClientHandler {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };

                var baseUrl = "http://192.168.15.8:5067/";
                
                _httpClient = new HttpClient(handler) {
                    BaseAddress = new Uri(baseUrl)
                };
#else
            Console.WriteLine("[ApiService] Ambiente Desktop detectado → usando localhost");

            var handler = new HttpClientHandler();
            var baseUrl = "http://localhost:5067/";

            _httpClient = new HttpClient(handler) {
                BaseAddress = new Uri(baseUrl)
            };
#endif

            Console.WriteLine($"[ApiService] Base URL configurada: {_httpClient.BaseAddress}");
        }

        public async Task<Dictionary<string, Dictionary<Guid, int>>> PostDataAsync<T>(string typeName, T data) {
            try {
                // A URL agora é fixa e aponta para o endpoint geral de sincronização,
                // que recebe o AllUploadsDto.
                var response = await _httpClient.PostAsJsonAsync("api/data/sync", data);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[ApiService] JSON recebido de {typeName}: {responseContent}");

                // O retorno agora é um dicionário aninhado que mapeia
                // o nome da entidade para o seu próprio mapeamento de IDs.
                return JsonSerializer.Deserialize<Dictionary<string, Dictionary<Guid, int>>>(responseContent);
            } catch (HttpRequestException ex) {
                Console.WriteLine($"[ApiService] Falha na requisição (UPLOAD {typeName}): {ex.Message}");
                // A exceção deve ser tratada pelo SyncService.
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