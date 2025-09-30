using System.ComponentModel.DataAnnotations; 
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ArenaVirtualAPI.Models {
    public enum StatusInscricao {
        Pendente,
        Aceita,
        Recusada
    }

    [Table("SolicitacoesCampeonato")]
    public class SolicitacaoCampeonato : ISyncable {

        [Key]
        [JsonIgnore]
        public int Id { get; set; }

        public Guid ClientAppId { get; set; } = Guid.NewGuid();

        public Guid TimeClientAppId { get; set; }

        public Guid CampeonatoClientAppId { get; set; }

        public StatusInscricao Status { get; set; }

        public DateTime DataSolicitacao { get; set; }

        public bool IsSynced { get; set; } = false;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }
}