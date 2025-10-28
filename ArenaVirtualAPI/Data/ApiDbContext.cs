using Microsoft.EntityFrameworkCore;
using ArenaVirtualAPI.Models;

namespace ArenaVirtualAPI.Data {
    public class ApiDbContext : DbContext {
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Campeonato> Campeonatos { get; set; }
        public DbSet<Time> Time { get; set; }
        public DbSet<Convite> Convites { get; set; }
        public DbSet<Jogo> Jogos { get; set; }
        public DbSet<UsuarioCampeonatoFavorito> UsuarioCampeonatoFavoritos { get; set; }
        public DbSet<RodadaDeJogos> RodadasDeJogos { get; set; }
        public DbSet<Inscricao> Inscricoes { get; set; }
        public DbSet<EstatisticaPartida> EstatisticasPartidas { get; set; }
        public DbSet<AvaliacaoArbitro> AvaliacoesArbitros { get; set; }
        public DbSet<CampanhaPatrocinio> CampanhasPatrocinios { get; set; }
        public DbSet<PropostaPatrocinio> PropostasPatrocinio { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Campeonato>()
                .Property(c => c.ValorTaxaInscricao)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Time>()
                .HasOne(t => t.Capitao)
                .WithOne(u => u.TimeCapitao)
                .HasForeignKey<Time>(t => t.CapitaoId)
                .OnDelete(DeleteBehavior.SetNull);

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

            // --- Jogo: Relações com Time A/B (Para quebrar o ciclo de FK) ---
            modelBuilder.Entity<Jogo>()
                .HasOne(j => j.TimeA)
                .WithMany()
                .HasForeignKey(j => j.TimeAId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Jogo>()
                .HasOne(j => j.TimeB)
                .WithMany()
                .HasForeignKey(j => j.TimeBId)
                .OnDelete(DeleteBehavior.NoAction);
            // -----------------------------------------------------------------

            modelBuilder.Entity<UsuarioCampeonatoFavorito>()
                .HasIndex(ucf => new { ucf.UsuarioId, ucf.CampeonatoId })
                .IsUnique();

            // 1. RodadaDeJogos (1:N com Jogo)
            modelBuilder.Entity<RodadaDeJogos>()
                .HasMany(r => r.Jogos)
                .WithOne(j => j.RodadaDeJogos)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // 3. EstatisticaPartida
            modelBuilder.Entity<EstatisticaPartida>()
                .HasOne(e => e.Usuario)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            // EstatisticaPartida -> Jogo
            modelBuilder.Entity<EstatisticaPartida>()
                .HasOne(e => e.Jogo)
                .WithMany(j => j.Estatisticas)
                .HasForeignKey(e => e.JogoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EstatisticaPartida>()
                .HasOne(e => e.Time)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            // 4. AvaliacaoArbitro 
            modelBuilder.Entity<AvaliacaoArbitro>()
                .HasOne(a => a.Arbitro)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            // AvaliacaoArbitro -> Jogo: CORREÇÃO DO NOME DA PROPRIEDADE
            modelBuilder.Entity<AvaliacaoArbitro>()
                .HasOne(a => a.Jogo)
                .WithMany(j => j.AvaliacoesArbitro) // <--- CORRIGIDO: Agora usa 'AvaliacoesArbitro' (singular)
                .HasForeignKey(a => a.JogoId)
                .OnDelete(DeleteBehavior.Cascade);

            // 5. CampanhaPatrocinio
            modelBuilder.Entity<CampanhaPatrocinio>()
                .Property(c => c.ValorProposta)
                .HasPrecision(18, 2);

            modelBuilder.Entity<CampanhaPatrocinio>()
                .HasOne(c => c.Patrocinador)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CampanhaPatrocinio>()
                .HasOne(c => c.Campeonato)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            // 6. PropostaPatrocinio
            modelBuilder.Entity<PropostaPatrocinio>()
                .Property(p => p.ValorMonetario)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PropostaPatrocinio>()
                .HasOne(p => p.Patrocinador)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PropostaPatrocinio>()
                .HasOne(p => p.Campeonato)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }

    }
}