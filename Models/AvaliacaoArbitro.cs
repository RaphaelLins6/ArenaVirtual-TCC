using SQLite;

namespace ArenaVirtual.Models {
    public class AvaliacaoArbitro : ISyncable {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int ArbitroId { get; set; }
        public int JogoId { get; set; }
        public string Comentarios { get; set; } = string.Empty;
        public int Nota { get; set; }
        public bool IsSynced { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
