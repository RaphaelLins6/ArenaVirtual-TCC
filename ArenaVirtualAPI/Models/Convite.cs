using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArenaVirtualAPI.Models {
    public class Convite : ISyncable {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int IdSolicitante { get; set; }
        public int IdTime { get; set; }
        public string ConvidadoEmail { get; set; } // Adicione esta linha
        public DateTime DataEnvio { get; set; }
        public StatusConvite Status { get; set; }
        public bool IsSynced { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public enum StatusConvite {
        Pendente,
        Aceito,
        Recusado
    }
}