using System.Text.Json;

namespace ArenaVirtualAPI.Dtos {
    // DTO para receber os dados de atualização do servidor
    public class UpdatesDTO {
        public Dictionary<string, JsonElement> UpdatedItems { get; set; }
            = new Dictionary<string, JsonElement>();
    }

    // Classe genérica para os itens sincronizáveis
    public class SyncItem {
        public int Id { get; set; }
        public DateTime UpdatedAt { get; set; }
        // adicione os campos que precisa para cada tipo de item ou use objetos diferentes se preferir
    }
}
