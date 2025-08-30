using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaVirtual.Models {
    public interface ISyncable {
        int Id { get; set; }
        public bool IsSynced { get; set; }
        public DateTime UpdatedAt { get; set; } 
    }
}
