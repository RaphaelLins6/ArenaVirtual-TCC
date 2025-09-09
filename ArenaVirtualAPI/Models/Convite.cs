// API: ArenaVirtualAPI.Models.Convite
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArenaVirtualAPI.Models {
    public class Convite : ISyncable {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public Guid ClientAppId { get; set; }

        // Referência à entidade de usuário que enviou o convite
        public int IdSolicitanteServidor { get; set; }

        // Referência à entidade de time para qual o convite foi enviado
        public int TimeId { get; set; }

        public string? ConvidadoEmail { get; set; }

        public DateTime DataEnvio { get; set; }

        public StatusConvite Status { get; set; }

        public bool IsSynced { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Navegação de propriedades para facilitar o acesso
        [ForeignKey("IdSolicitanteServidor")]
        public virtual Usuario Solicitante { get; set; }

        [ForeignKey("TimeId")]
        public virtual Time Time { get; set; }
    }

    public enum StatusConvite {
        Pendente,
        Aceito,
        Recusado
    }
}