using SQLite;

namespace ArenaVirtual.Models {
    public class Time {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string? Nome { get; set; }
        public int CampeonatoId { get; set; } // FK para Campeonato
    }
}