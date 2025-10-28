using System;
using System.ComponentModel.DataAnnotations;

namespace ArenaVirtualAPI.DTOs {
    // DTO para retornar dados
    public class AvaliacaoArbitroReadDto {
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }

        public int ArbitroId { get; set; }
        public int JogoId { get; set; }

        public string Comentarios { get; set; } = string.Empty;
        public int Nota { get; set; }

        public DateTime UpdatedAt { get; set; }
    }

    // DTO para criar/atualizar dados via API
    public class AvaliacaoArbitroCreateUpdateDto {
        // Necessário para o upsert via ClientAppId no POST (opcional)
        public Guid? ClientAppId { get; set; }

        [Required]
        public int ArbitroId { get; set; }
        [Required]
        public int JogoId { get; set; }

        public string Comentarios { get; set; } = string.Empty;

        [Range(1, 10)] // Assumindo uma nota de 1 a 10
        public int Nota { get; set; }
    }
}