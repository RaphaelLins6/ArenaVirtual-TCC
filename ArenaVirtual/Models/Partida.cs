using SQLite;

namespace ArenaVirtual.Models {
    public class Partida : ISyncable {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int TimeAId { get; set; }
        public int TimeBId { get; set; }
        public int CampeonatoId { get; set; }
        public DateTime DataHora { get; set; }
        public string? Local { get; set; }
        public bool IsSynced { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}