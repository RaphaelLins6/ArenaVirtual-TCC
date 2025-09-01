using System.Collections.Generic;
using System.Text.Json;

namespace ArenaVirtual.Models {
    // A classe UpdatesDTO deve ser exatamente igual à classe definida na sua API.
    // Ela serve para desserializar a resposta JSON do servidor.
    public class UpdatesDTO {
        public Dictionary<string, JsonElement> UpdatedItems { get; set; } = new Dictionary<string, JsonElement>();
    }
}