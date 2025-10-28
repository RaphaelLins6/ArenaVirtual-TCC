using System;
using System.ComponentModel.DataAnnotations;

namespace ArenaVirtualAPI.DTOs {
    // DTO para retornar dados
    public class InscricaoReadDto {
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }

        public int? TimeId { get; set; }
        public Guid TimeClientAppId { get; set; }

        public int? CampeonatoId { get; set; }
        public Guid CampeonatoClientAppId { get; set; }

        public string? Status { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // DTO para criar/atualizar dados via API (o ClientAppId é opcional no POST)
    public class InscricaoCreateUpdateDto {
        // Necessário para o upsert via ClientAppId no POST (opcional)
        public Guid? ClientAppId { get; set; }

        // Se o Controller/Cliente já resolveu o ID local
        public int? TimeId { get; set; }
        [Required]
        public Guid TimeClientAppId { get; set; }

        public int? CampeonatoId { get; set; }
        [Required]
        public Guid CampeonatoClientAppId { get; set; }

        public string? Status { get; set; }
    }
}