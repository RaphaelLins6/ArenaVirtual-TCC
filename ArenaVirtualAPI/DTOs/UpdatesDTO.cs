using System.Text.Json;

namespace ArenaVirtualAPI.DTOs {
    public class UpdatesDTO {
        public Dictionary<string, JsonElement> UpdatedItems { get; set; } = new Dictionary<string, JsonElement>();

    }
}
