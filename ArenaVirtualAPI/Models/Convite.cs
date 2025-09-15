using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArenaVirtualAPI.Models {
    public class Convite : ISyncable {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public Guid ClientAppId { get; set; }

        // Quem enviou o convite/solicitação
        public int IdSolicitanteServidor { get; set; }

        // Time para qual foi enviado
        public int TimeId { get; set; }

        // Email convidado (quando for convite do capitão)
        public string? ConvidadoEmail { get; set; }

        public DateTime DataEnvio { get; set; }

        public StatusConvite Status { get; set; }

        public bool IsSynced { get; set; }

        public DateTime UpdatedAt { get; set; }

        // 🔑 Identificação no app (linkar com TimeClientAppId)
        [ForeignKey("TimeId")]
        public virtual Time Time { get; set; }

        [ForeignKey("IdSolicitanteServidor")]
        public virtual Usuario Solicitante { get; set; }
    }

    public enum StatusConvite {
        Pendente,
        Aceito,
        Recusado
    }
}
