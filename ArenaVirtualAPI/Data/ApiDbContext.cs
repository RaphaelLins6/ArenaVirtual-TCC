using Microsoft.EntityFrameworkCore;
using ArenaVirtualAPI.Models; 

namespace ArenaVirtualAPI.Data {
    public class ApiDbContext : DbContext {
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Campeonato> Campeonatos { get; set; }
        public DbSet<Time> Times { get; set; }
        public DbSet<Convite> Convites { get; set; }
        public DbSet<UsuarioCampeonatoFavorito> UsuarioCampeonatoFavoritos { get; set; }

    }
}
