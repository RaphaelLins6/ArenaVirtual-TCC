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
        public DbSet<Jogo> Jogos { get; set; }
        public DbSet<UsuarioCampeonatoFavorito> UsuarioCampeonatoFavoritos { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Campeonato>()
                .Property(c => c.ValorTaxaInscricao)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Time>()
                .HasOne(t => t.Capitao)
                .WithOne(u => u.TimeCapitao)
                .HasForeignKey<Time>(t => t.CapitaoId)
                .OnDelete(DeleteBehavior.SetNull); // Permite que um Time exista sem Capitão

            // Configura o relacionamento N:1 entre Usuario e Time (Membros)
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Time)
                .WithMany(t => t.Membros)
                .HasForeignKey(u => u.TimeId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configura o relacionamento N:1 entre Time e Campeonato
            modelBuilder.Entity<Time>()
                .HasOne(t => t.Campeonato)
                .WithMany(c => c.Times)
                .HasForeignKey(t => t.CampeonatoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UsuarioCampeonatoFavorito>()
                .HasIndex(ucf => new { ucf.UsuarioId, ucf.CampeonatoId })
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }

    }
}
