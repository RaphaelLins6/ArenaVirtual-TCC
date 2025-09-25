using SQLite;
using System;
using System.Text.Json.Serialization;

namespace ArenaVirtual.Models {
    public enum StatusInscricao {
        Pendente,
        Aceita,
        Recusada
    }

    [Table("SolicitacoesCampeonato")]
    public class SolicitacaoCampeonato : ISyncable {
        [PrimaryKey, AutoIncrement]
        [JsonIgnore]
        public int Id { get; set; }

        [NotNull, Unique]
        public Guid ClientAppId { get; set; } = Guid.NewGuid();

        public Guid TimeClientAppId { get; set; }

        public Guid CampeonatoClientAppId { get; set; }

        public StatusInscricao Status { get; set; }

        public DateTime DataSolicitacao { get; set; }

        // Propriedades da interface ISyncable
        public bool IsSynced { get; set; } = false;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }
}