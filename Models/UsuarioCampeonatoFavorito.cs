using SQLite;

namespace ArenaVirtual.Models {
    [Table("UsuarioCampeonatoFavoritos")]
    public class UsuarioCampeonatoFavorito : ISyncable {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int UsuarioId { get; set; }

        [Indexed]
        public int CampeonatoId { get; set; }
        public bool IsSynced { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
