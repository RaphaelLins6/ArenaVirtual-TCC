using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaVirtual.Models {
    public class Campeonato {
            public int Id { get; set; }
            public required string Nome { get; set; }
            public DateTime DataInicio { get; set; }
            public DateTime DataFim { get; set; }
            public int OrganizadorId { get; set; }
        }

    }
