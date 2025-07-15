using SQLite;

namespace ArenaVirtual.Models {
    public class AvaliacaoArbitro {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int ArbitroId { get; set; }
        public int JogoId { get; set; }
        public string Comentarios { get; set; } = string.Empty;
        public int Nota { get; set; } 
    }
}
