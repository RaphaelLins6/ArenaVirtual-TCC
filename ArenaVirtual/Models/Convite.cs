using SQLite;

namespace ArenaVirtual.Models {
    public class Convite : ISyncable {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int IdSolicitante { get; set; } 

        [Indexed]
        public int IdTime { get; set; } 

        public DateTime DataEnvio { get; set; } 

        public StatusConvite Status { get; set; }

        public bool IsSynced { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public enum StatusConvite {
        Pendente,
        Aceito,
        Recusado
    }
}