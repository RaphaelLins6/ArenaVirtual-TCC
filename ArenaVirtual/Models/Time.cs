using SQLite;
using System.ComponentModel.DataAnnotations.Schema; 

namespace ArenaVirtual.Models {
    public class Time : ISyncable{
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull, MaxLength(100)] 
        public string Nome { get; set; } = string.Empty; 

        [MaxLength(255)] 
        public string? LogoUrl { get; set; }

        public int CampeonatoId { get; set; } 

        [MaxLength(500)]
        public string? Descricao { get; set; }

        [NotNull]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string? Regiao { get; set; }

        public int PontuacaoTotal { get; set; } = 0;

        public int Vitorias { get; set; } = 0;
        public int Derrotas { get; set; } = 0;
        public int Empates { get; set; } = 0;

        [ForeignKey("CapitaoId")]
        public int? CapitaoId { get; set; }

        public bool IsSynced { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}