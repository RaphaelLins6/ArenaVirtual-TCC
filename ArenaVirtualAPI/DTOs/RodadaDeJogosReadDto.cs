using System;
using System.ComponentModel.DataAnnotations;

namespace ArenaVirtualAPI.DTOs {
    public class RodadaDeJogosReadDto {
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }
        public string NomeRodada { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
    }

    public class RodadaDeJogosCreateUpdateDto {
        [Required]
        [StringLength(100)]
        public string NomeRodada { get; set; } = string.Empty;
    }
}