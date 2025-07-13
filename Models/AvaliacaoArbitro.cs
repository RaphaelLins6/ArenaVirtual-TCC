using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaVirtual.Models {
    public class AvaliacaoArbitro {
        public int Id { get; set; }
        public int ArbitroId { get; set; }
        public int JogoId { get; set; }
        public string Comentarios { get; set; } = string.Empty;
        public int Nota { get; set; } 
    }
}
