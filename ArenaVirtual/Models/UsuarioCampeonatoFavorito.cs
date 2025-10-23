using SQLite;
using System;

namespace ArenaVirtual.Models {
    [Table("UsuarioCampeonatoFavoritos")]
    public class UsuarioCampeonatoFavorito : ISyncable {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public Guid ClientAppId { get; set; } = Guid.NewGuid();

        public int? IdServidor { get; set; }

        [Indexed]
        public Guid UsuarioClientAppId { get; set; }
        [Indexed]
        public Guid CampeonatoClientAppId { get; set; }

        public bool IsSynced { get; set; } = false;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public UsuarioCampeonatoFavorito() { }
    }
}