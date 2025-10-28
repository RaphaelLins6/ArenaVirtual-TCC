using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArenaVirtualAPI.Models // Namespace corrigido para API
{
    public class Inscricao : ISyncable {
        // Chave primária do EF Core
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Propriedade de sincronização da interface (Guid?)
        public Guid ClientAppId { get; set; }

        // --- Chaves estrangeiras (Ids de Sincronização e Navegação) ---

        // TimeClientAppId é a chave universal do Time no Client App
        public Guid TimeClientAppId { get; set; }
        // ID local (int) do Time
        public int? TimeId { get; set; }
        [ForeignKey("TimeId")]
        public virtual Time? Time { get; set; } // Propriedade de navegação para a entidade Time

        // CampeonatoClientAppId é a chave universal do Campeonato no Client App
        public Guid CampeonatoClientAppId { get; set; }
        // ID local (int) do Campeonato
        public int? CampeonatoId { get; set; }
        [ForeignKey("CampeonatoId")]
        public virtual Campeonato? Campeonato { get; set; } // Propriedade de navegação para o Campeonato

        // --- Outras propriedades ---

        // Status pode ser anulável
        public string? Status { get; set; }

        // Propriedades de sincronização
        public bool IsSynced { get; set; } = false;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Construtores
        public Inscricao() { }
    }
}
