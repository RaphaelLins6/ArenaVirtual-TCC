using SQLite;
using System;

namespace ArenaVirtual.Models {
    [Table("UsuarioCampeonatoFavoritos")]
    public class UsuarioCampeonatoFavorito : ISyncable {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // Chave de sincronização
        public Guid ClientAppId { get; set; } = Guid.NewGuid();

        // Propriedade IdServidor adicionada para a sincronização
        public int? IdServidor { get; set; }

        // Referências a outras entidades usando a chave universal
        [Indexed]
        public Guid UsuarioClientAppId { get; set; }
        [Indexed]
        public Guid CampeonatoClientAppId { get; set; }

        public bool IsSynced { get; set; } = false;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public UsuarioCampeonatoFavorito() { }
    }
}