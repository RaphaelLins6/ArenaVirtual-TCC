using SQLite;

namespace ArenaVirtual.Models {
    public class Jogo : ISyncable {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int TimeAId { get; set; }
        public int TimeBId { get; set; }
        public DateTime DataHora { get; set; }
        public int CampeonatoId { get; set; }
        public int ArbitroId { get; set; }
        public string Local { get; set; } = string.Empty;
        public bool IsSynced { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
