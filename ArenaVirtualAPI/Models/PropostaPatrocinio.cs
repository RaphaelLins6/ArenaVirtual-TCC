using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// Incluir ArenaVirtualAPI.Models para se alinhar ao seu projeto
namespace ArenaVirtualAPI.Models {
    // ISyncable assume-se que foi definida em outro lugar
    public class PropostaPatrocinio : ISyncable {
        // Substituindo [PrimaryKey, AutoIncrement] do SQLite por [Key] e DatabaseGenerated
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Deve ser anulável (Guid?) se a proposta puder existir sem sincronização imediata
        public Guid ClientAppId { get; set; }

        // Propriedades do Patrocinador (Patrocinador é um Usuário com Perfil adequado)
        // Chave estrangeira para o Usuário (Patrocinador)
        public int PatrocinadorId { get; set; }
        [ForeignKey("PatrocinadorId")]
        public virtual Usuario Patrocinador { get; set; }

        // Propriedades do Campeonato
        // Chave estrangeira para a entidade Campeonato (assume-se que ela existe)
        public int CampeonatoId { get; set; }
        [ForeignKey("CampeonatoId")]
        public virtual Campeonato Campeonato { get; set; }

        public string NomePatrocinador { get; set; } = string.Empty;
        public string ImagemPatrocinador { get; set; } = string.Empty;
        public string LinkPatrocinador { get; set; } = string.Empty;

        // Decimal é um tipo de valor não-anulável. Se no banco de dados for NULL, 
        // deve ser decimal?. Manter como decimal se ValorMonetario for obrigatório.
        [Column(TypeName = "decimal(18, 2)")] // Garante o tipo no SQL Server
        public decimal ValorMonetario { get; set; } = 0.00M;

        // DateTime é um tipo de valor não-anulável. Se puder ser NULL, use DateTime?. 
        // Manter como DateTime se for obrigatório (como datas de início/fim).
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }

        public string Mensagem { get; set; } = string.Empty;

        public bool Aprovada { get; set; } = false; // Manter como bool (não-anulável)

        // Propriedades de sincronização
        public bool IsSynced { get; set; } = false;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public PropostaPatrocinio() { }
    }
}
