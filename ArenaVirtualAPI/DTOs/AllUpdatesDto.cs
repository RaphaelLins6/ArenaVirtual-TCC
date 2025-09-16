using System.Text.Json;

namespace ArenaVirtualAPI.DTOs {
    public class AllUpdatesDto {
        public Dictionary<string, JsonElement> UpdatedItems { get; set; } = new Dictionary<string, JsonElement>();

    }
}
