using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArenaVirtualAPI.Models // Namespace corrigido para API
{
    public class CampanhaPatrocinio : ISyncable {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Corrigido para Guid? para implementar ISyncable corrigido
        public Guid ClientAppId { get; set; }

        public string? ImagemPatrocinador { get; set; }
        public string Nome { get; set; } = string.Empty;

        // Chave estrangeira para o Patrocinador (que é um Usuário)
        public int PatrocinadorId { get; set; }
        [ForeignKey("PatrocinadorId")]
        public virtual Usuario? Patrocinador { get; set; }

        // Chave estrangeira para o Campeonato
        public int CampeonatoId { get; set; }
        [ForeignKey("CampeonatoId")]
        public virtual Campeonato? Campeonato { get; set; }

        // Usando Column para garantir o tipo no SQL Server
        [Column(TypeName = "decimal(18, 2)")]
        public decimal ValorProposta { get; set; }

        public DateTime Inicio { get; set; }
        public DateTime Fim { get; set; }
        public string Descricao { get; set; } = string.Empty;

        // Propriedades de sincronização
        public bool IsSynced { get; set; }
        public DateTime UpdatedAt { get; set; }

        public CampanhaPatrocinio() { }
    }
}