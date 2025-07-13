using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaVirtual.Models {
    public class PropostaPatrocinio {
        public int Id { get; set; }
        public int PatrocinadorId { get; set; }
        public int CampeonatoId { get; set; }
        public string Mensagem { get; set; } = string.Empty; // Default value added
        public bool Aprovada { get; set; }
    }
}
