using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaVirtual.Models {
    public class PatrocinioDetalhe : ISyncable {
        public CampanhaPatrocinio Campanha { get; set; }
        public PropostaPatrocinio Proposta { get; set; }
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }
}
