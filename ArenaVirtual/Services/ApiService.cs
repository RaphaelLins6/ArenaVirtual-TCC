using System.Collections;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace ArenaVirtual.Services {
    public class ApiService {
        private readonly HttpClient _httpClient;

        // NOVO: A URL base é fixada para o endereço público do Azure
        private const string AzureBaseUrl = "https://arenavirtualapi-cvghgbcgdqfbdhha.canadacentral-01.azurewebsites.net/";

        public ApiService() {
            Console.WriteLine($"[ApiService] Ambiente Nuvem detectado → usando Azure: {AzureBaseUrl}");

            var handler = new HttpClientHandler();

            // O Azure usa HTTPS, mas se houver algum problema de certificado no MAUI durante o DEBUG
            // você pode precisar habilitar o ServerCertificateCustomValidationCallback
            // #if DEBUG
            // handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            // #endif

            _httpClient = new HttpClient(handler) {
                BaseAddress = new Uri(AzureBaseUrl)
            };

            Console.WriteLine($"[ApiService] Base URL configurada: {_httpClient.BaseAddress}");
        }

        // CORRIGIDO: Altera o tipo de retorno da chave GUID para string para evitar erros de desserialização.
        public async Task<Dictionary<string, Dictionary<string, int>>> PostDataAsync<T>(string typeName, T data) {
            try {
                // A URL agora é fixa e aponta para o endpoint geral de sincronização,
                // que recebe o AllUploadsDto.
                var response = await _httpClient.PostAsJsonAsync("api/data/sync", data);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[ApiService] JSON recebido de {typeName}: {responseContent}");

                // CORRIGIDO: Desserializa para Dictionary<string, Dictionary<string, int>>
                return JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, int>>>(responseContent);
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