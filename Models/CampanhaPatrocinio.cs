using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaVirtual.Models {
    public class CampanhaPatrocinio {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty; // Fixed: Initialized with a default value
        public int PatrocinadorId { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fim { get; set; }
        public string Descricao { get; set; } = string.Empty; // Fixed: Initialized with a default value
    }
}
