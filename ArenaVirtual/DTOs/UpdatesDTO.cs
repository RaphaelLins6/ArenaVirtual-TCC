using System.Collections.Generic;
using System.Text.Json;

namespace ArenaVirtual.DTOs {
    
    public class UpdatesDTO {
        public Dictionary<string, JsonElement> UpdatedItems { get; set; } = new Dictionary<string, JsonElement>();
    }
}