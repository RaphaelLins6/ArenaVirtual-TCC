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

            var baseUrls = new[] {
                "https://192.168.15.8:7117/",
                "http://192.168.15.8:5067/"
            };

            _httpClient = CreateHttpClient(handler, baseUrls);
#else
            Console.WriteLine("[ApiService] Ambiente Desktop detectado → usando localhost");

            var handler = new HttpClientHandler();
            var baseUrls = new[] {
                "https://localhost:7117/",
                "http://localhost:5067/"
            };

            _httpClient = CreateHttpClient(handler, baseUrls);
#endif

            Console.WriteLine($"[ApiService] Base URL configurada: {_httpClient.BaseAddress}");
        }

        private HttpClient CreateHttpClient(HttpClientHandler handler, string[] baseUrls) {
            foreach (var url in baseUrls) {
                try {
                    var client = new HttpClient(handler) {
                        BaseAddress = new Uri(url)
                    };

                    var response = client.GetAsync("swagger/index.html").Result;
                    if (response.IsSuccessStatusCode) {
                        Console.WriteLine($"[ApiService] Usando endpoint: {url}");
                        return client;
                    }
                } catch {
                    Console.WriteLine($"[ApiService] Falhou ao conectar em {url}, tentando próximo...");
                }
            }

            throw new Exception("[ApiService] Nenhum endpoint disponível!");
        }

        // AGORA RETORNA UM DICIONÁRIO
        public async Task<Dictionary<Guid, int>> PostDataAsync<T>(string typeName, T data) {
            try {
                // Adiciona log do JSON que será enviado
                var jsonContent = JsonSerializer.Serialize(data);
                Console.WriteLine($"[ApiService] Enviando {GetItemCount(data)} itens de {typeName} → api/data/sync/{typeName}");
                Debug.WriteLine($"[ApiService] JSON enviado para {typeName}: {jsonContent}");

                var response = await _httpClient.PostAsJsonAsync($"api/data/sync/{typeName}", data);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[ApiService] JSON recebido de {typeName}: {responseContent}");

                return JsonSerializer.Deserialize<Dictionary<Guid, int>>(responseContent);

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
            if (data is System.Collections.ICollection collection) {
                return collection.Count;
            }
            return 1;
        }
    }

    public class UpdateResponse {
        public Dictionary<string, JsonElement> UpdatedItems { get; set; } = new();
    }
}