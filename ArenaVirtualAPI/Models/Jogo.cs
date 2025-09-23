// Arquivo: ArenaVirtualAPI/Models/Jogo.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArenaVirtualAPI.Models {
    public class Jogo : ISyncable {
        [Key]
        public int Id { get; set; }

        public Guid ClientAppId { get; set; }

        public int TimeAId { get; set; }
        public int TimeBId { get; set; }
        public int CampeonatoId { get; set; }
        public int ArbitroId { get; set; }

        [Required, MaxLength(255)]
        public string Local { get; set; } = string.Empty;

        public DateTime DataHora { get; set; }
        public int PlacarA { get; set; }
        public int PlacarB { get; set; }

        // Propriedades de navegação do Entity Framework Core
        [ForeignKey("TimeAId")]
        public Time? TimeA { get; set; }

        [ForeignKey("TimeBId")]
        public Time? TimeB { get; set; }

        [ForeignKey("CampeonatoId")]
        public Campeonato? Campeonato { get; set; }

        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }
}