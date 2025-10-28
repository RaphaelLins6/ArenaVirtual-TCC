using System;
using System.ComponentModel.DataAnnotations;

namespace ArenaVirtualAPI.DTOs {
    // DTO para retornar dados
    public class RodadaDeJogosReadDto {
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }
        public string NomeRodada { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
    }

    // DTO para criar/atualizar dados via API (sem IDs de sincronização)
    public class RodadaDeJogosCreateUpdateDto {
        [Required]
        [StringLength(100)]
        public string NomeRodada { get; set; } = string.Empty;
    }
}