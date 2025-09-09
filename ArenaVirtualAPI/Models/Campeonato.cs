using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace ArenaVirtualAPI.Models {
    public class Campeonato : ISyncable {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }

        [MaxLength(200)]
        public string? Nome { get; set; }

        [MaxLength(200)]
        public string? Local { get; set; }

        [NotMapped]
        public bool EhFavorito { get; set; }

        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }

        // Chave estrangeira para o organizador
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
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsSynced { get; set; } = false;

        // Propriedades alinhadas com o modelo do aplicativo
        public string? Descricao { get; set; }
        public string? Modalidade { get; set; }
        public string? Regras { get; set; }
        public DateTime? DataTermino { get; set; }
        public int? NumeroEquipes { get; set; }

        // Propriedade de navegação para o Organizador
        [ForeignKey("OrganizadorId")]
        public Usuario? Organizador { get; set; }

        [JsonIgnore]
        public ICollection<Time>? Times { get; set; }
    }
}