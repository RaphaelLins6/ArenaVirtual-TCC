using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArenaVirtualAPI.Models // Namespace corrigido para API
{
    public class AvaliacaoArbitro : ISyncable {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Corrigido para Guid? para implementar ISyncable corrigido
        public Guid ClientAppId { get; set; }

        // Chave estrangeira para o Arbitro (que é um Usuário)
        public int ArbitroId { get; set; }
        [ForeignKey("ArbitroId")]
        public virtual Usuario? Arbitro { get; set; }

        // Chave estrangeira para o Jogo
        public int JogoId { get; set; }
        [ForeignKey("JogoId")]
        public virtual Jogo? Jogo { get; set; }

        public string Comentarios { get; set; } = string.Empty;
        public int Nota { get; set; }

        // Propriedades de sincronização
        public bool IsSynced { get; set; }
        public DateTime UpdatedAt { get; set; }

        public AvaliacaoArbitro() { }
    }
}