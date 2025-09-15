using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ArenaVirtualAPI.Models {
    [Table("UsuarioCampeonatoFavoritos")]
    [Index(nameof(UsuarioClientAppId), nameof(CampeonatoClientAppId), IsUnique = true)]
    public class UsuarioCampeonatoFavorito : ISyncable {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public Guid ClientAppId { get; set; }

        // Novos campos para os IDs de servidor (chaves estrangeiras)
        public int UsuarioId { get; set; }
        public int CampeonatoId { get; set; }

        // Mantém os campos de GUID para sincronização
        public Guid UsuarioClientAppId { get; set; }
        public Guid CampeonatoClientAppId { get; set; }

        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }
}