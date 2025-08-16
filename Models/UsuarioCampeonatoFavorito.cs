using SQLite;

namespace ArenaVirtual.Models {
    [Table("UsuarioCampeonatoFavoritos")]
    public class UsuarioCampeonatoFavorito {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int UsuarioId { get; set; }

        [Indexed]
        public int CampeonatoId { get; set; }
    }
}
