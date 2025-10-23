using SQLite;
using System;

namespace ArenaVirtual.Models {
    public class Inscricao : ISyncable {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public Guid ClientAppId { get; set; } = Guid.NewGuid();
        public Guid TimeClientAppId { get; set; }
        public Guid CampeonatoClientAppId { get; set; }
        public string? Status { get; set; } 
        public bool IsSynced { get; set; }
        public DateTime UpdatedAt { get; set; }
        [Ignore]
        public Time? Time { get; set; } 
    }
}