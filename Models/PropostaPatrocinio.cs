using SQLite;

namespace ArenaVirtual.Models {
    public class PropostaPatrocinio {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int PatrocinadorId { get; set; }
        public int CampeonatoId { get; set; }
        public string Mensagem { get; set; } = string.Empty; // Default value added
        public bool Aprovada { get; set; }
    }
}
