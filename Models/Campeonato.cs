using SQLite;

namespace ArenaVirtual.Models {
    public class Campeonato : ISyncable {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string? Local { get; set; }
        [Ignore]
        public bool EhFavorito { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public int OrganizadorId { get; set; } 
        public string? LogoUrl { get; set; }
        public string? NomeOrganizador { get; set; }
        public string? EmailOrganizador { get; set; }
        public string? TelefoneOrganizador { get; set; }
        public int NumeroMaximoEquipes { get; set; }
        public decimal ValorTaxaInscricao { get; set; }
        public string? FormatoCampeonato { get; set; }
        public string? LocaisDosJogos { get; set; }
        public bool HaveraPremiacao { get; set; }
        public bool IsSynced { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}