using SQLite;

namespace ArenaVirtual.Models {
    public class Convite {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int IdSolicitante { get; set; } // ID do usuário que enviou o convite

        [Indexed]
        public int IdTime { get; set; } // ID do time para o qual o convite foi enviado

        public DateTime DataEnvio { get; set; } // Data e hora do envio

        // Enum para o status do convite: Pendente, Aceito, Recusado
        public StatusConvite Status { get; set; }
    }

    public enum StatusConvite {
        Pendente,
        Aceito,
        Recusado
    }
}