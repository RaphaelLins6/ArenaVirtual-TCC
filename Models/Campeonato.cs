using SQLite;

namespace ArenaVirtual.Models {
    public class Campeonato {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string? Local { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public int OrganizadorId { get; set; } // FK para Usuario
    }
}