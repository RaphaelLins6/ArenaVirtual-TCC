using System;

namespace ArenaVirtualAPI.DTOs {
    // DTO para retornar o detalhe de patrocínio agregado para o cliente
    public class PatrocinioDetalheReadDto {
        // --- Informações da Proposta ---
        public int PropostaId { get; set; }
        public Guid PropostaClientAppId { get; set; }
        public DateTime PropostaUpdatedAt { get; set; }

        public string NomePatrocinador { get; set; } = string.Empty;
        public string ImagemPatrocinador { get; set; } = string.Empty;

        public decimal ValorMonetario { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public bool Aprovada { get; set; }

        // Chaves de sincronização das FKs
        public Guid PatrocinadorClientAppId { get; set; }
        public Guid CampeonatoClientAppId { get; set; }

        // --- Informações Agregadas da Campanha (podem ser nulas) ---
        public Guid? CampanhaClientAppId { get; set; }
        public string? CampanhaNome { get; set; }
        public decimal? CampanhaValorProposta { get; set; }
        public DateTime? CampanhaInicio { get; set; }
        public DateTime? CampanhaFim { get; set; }
        public string? CampanhaDescricao { get; set; }
    }
}