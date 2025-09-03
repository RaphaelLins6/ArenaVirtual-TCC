using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArenaVirtualAPI.Models {
    public class UsuarioCampeonatoFavorito : ISyncable {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int CampeonatoId { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }

}
