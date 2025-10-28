using System;
using System.Text.Json.Serialization;

namespace ArenaVirtual.DTOs {
    public class LoginResponseDto {

        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("clientAppId")]
        public Guid ClientAppId { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("nome")]
        public string Nome { get; set; } = string.Empty;

        [JsonPropertyName("senhaHash")]
        public string SenhaHash { get; set; } = string.Empty;
    }
}
