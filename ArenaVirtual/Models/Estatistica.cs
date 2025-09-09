using SQLite;

namespace ArenaVirtual.Models {
    public class Estatistica : ISyncable {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }

        public int UsuarioId { get; set; }
        public int JogoId { get; set; }
        public int Pontos { get; set; }
        public int Rebotes { get; set; }
        public int Assistencias { get; set; }
        public bool IsSynced { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
