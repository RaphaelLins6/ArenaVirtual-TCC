using ArenaVirtualAPI.Models;
using System.Collections.Generic;

namespace ArenaVirtualAPI.Models {
    // Data Transfer Object para as atualizações de sincronização.
    public class UpdatesDTO {
        public Dictionary<string, IEnumerable<ISyncable>> UpdatedItems { get; set; } = new Dictionary<string, IEnumerable<ISyncable>>();
    }
}