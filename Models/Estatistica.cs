using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaVirtual.Models {
    public class Estatistica {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int JogoId { get; set; }
        public int Pontos { get; set; }
        public int Rebotes { get; set; }
        public int Assistencias { get; set; }
    }
}
