using SQLite;

namespace ArenaVirtual.Models {
    public class CampanhaPatrocinio : ISyncable {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }

        public string Nome { get; set; } = string.Empty; 
        public int PatrocinadorId { get; set; }
        public int CampeonatoId { get; set; }
        public decimal ValorProposta { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fim { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public bool IsSynced { get; set; }
        public DateTime UpdatedAt { get; set; }
        public CampanhaPatrocinio() { }
    }
}
