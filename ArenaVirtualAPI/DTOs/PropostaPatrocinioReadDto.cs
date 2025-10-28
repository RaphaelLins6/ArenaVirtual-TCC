using System;
using System.ComponentModel.DataAnnotations;
using ArenaVirtualAPI.Models; // Para usar os tipos de Models (se necessário)

namespace ArenaVirtualAPI.DTOs {
    // DTO para retornar dados
    public class PropostaPatrocinioReadDto {
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }
        public int PatrocinadorId { get; set; }
        public int CampeonatoId { get; set; }
        public string NomePatrocinador { get; set; } = string.Empty;
        public string ImagemPatrocinador { get; set; } = string.Empty;
        public string LinkPatrocinador { get; set; } = string.Empty;
        public decimal ValorMonetario { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public bool Aprovada { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // DTO para criar/atualizar dados via API
    public class PropostaPatrocinioCreateUpdateDto {
        // IDs Locais são usados na API (se o Controller for a fonte de dados)
        [Required]
        public int PatrocinadorId { get; set; }
        [Required]
        public int CampeonatoId { get; set; }

        [Required]
        public string NomePatrocinador { get; set; } = string.Empty;
        public string ImagemPatrocinador { get; set; } = string.Empty;
        public string LinkPatrocinador { get; set; } = string.Empty;

        [Range(0.01, (double)decimal.MaxValue)]
        public decimal ValorMonetario { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public bool Aprovada { get; set; }
    }
}