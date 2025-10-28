using System;
using System.ComponentModel.DataAnnotations;

namespace ArenaVirtualAPI.DTOs {
    // DTO para retornar dados
    public class CampanhaPatrocinioReadDto {
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }

        public string Nome { get; set; } = string.Empty;
        public string? ImagemPatrocinador { get; set; }

        public int PatrocinadorId { get; set; }
        public int CampeonatoId { get; set; }

        public decimal ValorProposta { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fim { get; set; }
        public string Descricao { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; }
    }

    // DTO para criar/atualizar dados via API
    public class CampanhaPatrocinioCreateUpdateDto {
        // Necessário para o upsert via ClientAppId no POST (opcional)
        public Guid? ClientAppId { get; set; }

        [Required]
        public string Nome { get; set; } = string.Empty;
        public string? ImagemPatrocinador { get; set; }

        [Required]
        public int PatrocinadorId { get; set; }
        [Required]
        public int CampeonatoId { get; set; }

        [Range(0.01, (double)decimal.MaxValue)]
        public decimal ValorProposta { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fim { get; set; }
        public string Descricao { get; set; } = string.Empty;
    }
}