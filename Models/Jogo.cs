using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaVirtual.Models {
    public class Jogo {
        public int Id { get; set; }
        public int TimeAId { get; set; }
        public int TimeBId { get; set; }
        public DateTime DataHora { get; set; }
        public int CampeonatoId { get; set; }
        public int ArbitroId { get; set; }
        public required string Local { get; set; }
    }
}
