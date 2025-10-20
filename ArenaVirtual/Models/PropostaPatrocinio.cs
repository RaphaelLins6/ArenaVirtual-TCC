using SQLite;

namespace ArenaVirtual.Models {
    public class PropostaPatrocinio : ISyncable {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }
        public int PatrocinadorId { get; set; }
        public int CampeonatoId { get; set; }
        public string NomePatrocinador { get; set; }
        public string ImagemPatrocinador { get; set; } // O caminho/URL do banner
        public string LinkPatrocinador { get; set; }
        public decimal ValorMonetario { get; set; } = 0.00M;
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public string Mensagem { get; set; } = string.Empty; // Default value added
        public bool Aprovada { get; set; }
        public bool IsSynced { get; set; }
        public DateTime UpdatedAt { get; set; }
        public PropostaPatrocinio() { }
    }
}