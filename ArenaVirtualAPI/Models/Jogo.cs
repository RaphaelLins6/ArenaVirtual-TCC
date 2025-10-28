using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArenaVirtualAPI.Models {
    public class Jogo : ISyncable {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Identificador universal para sincronização
        public Guid ClientAppId { get; set; }

        // -----------------------------------------------------------------
        // --- SOLUÇÃO PARA O ERRO: Adicionado Relacionamento com RodadaDeJogos ---
        // Chave estrangeira para a RodadaDeJogos (int)
        public int? RodadaDeJogosId { get; set; }
        [ForeignKey("RodadaDeJogosId")]
        public virtual RodadaDeJogos? RodadaDeJogos { get; set; }
        // -----------------------------------------------------------------

        // --- Relacionamentos com Times ---
        public int TimeAId { get; set; }
        public int TimeBId { get; set; }

        // Chaves universais (Guid) para sincronização, necessárias no sync DTO
        public Guid TimeAClientAppId { get; set; }
        public Guid TimeBClientAppId { get; set; }

        [ForeignKey("TimeAId")]
        public virtual Time? TimeA { get; set; }

        [ForeignKey("TimeBId")]
        public virtual Time? TimeB { get; set; }

        // --- Relacionamento com Campeonato ---
        public int CampeonatoId { get; set; }
        public Guid CampeonatoClientAppId { get; set; } // Chave universal para sincronização

        [ForeignKey("CampeonatoId")]
        public virtual Campeonato? Campeonato { get; set; }

        // --- Relacionamento com Arbitro (que é um Usuário) ---
        public int? ArbitroId { get; set; } // Pode ser nulo
        public Guid? ArbitroClientAppId { get; set; } // Chave universal para sincronização

        [ForeignKey("ArbitroId")]
        public virtual Usuario? Arbitro { get; set; }

        // --- Detalhes do Jogo (existentes no seu arquivo) ---
        [Required, MaxLength(255)]
        public string Local { get; set; } = string.Empty;

        public DateTime DataHora { get; set; }
        public int PlacarA { get; set; } = 0;
        public int PlacarB { get; set; } = 0;

        // --- Propriedades de Navegação Opcionais (Coleções 1:N) ---
        public virtual ICollection<EstatisticaPartida> Estatisticas { get; set; } = new List<EstatisticaPartida>();
        public virtual ICollection<AvaliacaoArbitro> AvaliacoesArbitro { get; set; } = new List<AvaliacaoArbitro>();


        // --- Propriedades de Sincronização ---
        public bool IsSynced { get; set; } = false;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Jogo() { }
    }
}