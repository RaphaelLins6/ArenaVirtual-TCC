using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaVirtual.Models {
    public class Time {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty; // Fixed: Initialized with a default value
        public int CampeonatoId { get; set; }
        public List<int> AtletasIds { get; set; } = new();
    }
}
