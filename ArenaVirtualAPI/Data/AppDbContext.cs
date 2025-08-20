using Microsoft.EntityFrameworkCore;
using ArenaVirtualAPI.Models; // ajuste para onde estão suas entidades

namespace ArenaVirtualAPI.Data {
    public class AppDbContext : DbContext {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Campeonato> Campeonatos { get; set; }
        public DbSet<Time> Times { get; set; }
        //public DbSet<Partida> Partidas { get; set; }
        // adicione as outras tabelas conforme necessário
    }
}
