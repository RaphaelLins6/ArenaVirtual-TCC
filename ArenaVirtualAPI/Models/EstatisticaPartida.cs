using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArenaVirtualAPI.Models // Namespace corrigido para API
{
    // Removido 'ObservableObject' - esta é uma Entidade EF Core, não um ViewModel
    public class EstatisticaPartida : ISyncable {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Propriedade de sincronização (Guid?)
        public Guid ClientAppId { get; set; }

        // --- Chaves Estrangeiras e Navegação ---

        // Relacionamento com Usuário (o jogador)
        public int UsuarioId { get; set; }
        [ForeignKey("UsuarioId")]
        public virtual Usuario? Usuario { get; set; }

        // Relacionamento com Jogo (a partida)
        public int JogoId { get; set; }
        [ForeignKey("JogoId")]
        public virtual Jogo? Jogo { get; set; }

        // Relacionamento com Time
        public int TimeId { get; set; }
        [ForeignKey("TimeId")]
        public virtual Time? Time { get; set; }

        // --- Estatísticas (Manter como int, assumindo que não serão nulas) ---

        public int Pontos { get; set; } = 0;
        public int Rebotes { get; set; } = 0;
        public int Assistencias { get; set; } = 0;
        public int Roubos { get; set; } = 0;
        public int Bloqueios { get; set; } = 0;
        public int Faltas { get; set; } = 0;
        public int Turnovers { get; set; } = 0;

        public int Arremessos2PontosConvertidos { get; set; } = 0;
        public int Arremessos2PontosTentados { get; set; } = 0;

        public int Arremessos3PontosConvertidos { get; set; } = 0;
        public int Arremessos3PontosTentados { get; set; } = 0;

        public int LancesLivresConvertidos { get; set; } = 0;
        public int LancesLivresTentados { get; set; } = 0;

        // --- Sincronização ---
        public bool IsSynced { get; set; } = false;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public EstatisticaPartida() { }
    }
}
