using ArenaVirtualAPI.Data;
using ArenaVirtualAPI.DTOs;
using ArenaVirtualAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ArenaVirtualAPI.Services {
    public class RodadaDeJogosService : IRodadaDeJogosService {
        private readonly ApiDbContext _context;
        private readonly ILogger<RodadaDeJogosService> _logger;

        public RodadaDeJogosService(ApiDbContext context, ILogger<RodadaDeJogosService> logger) {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<RodadaDeJogos>> GetAllAsync() {
            return await _context.RodadasDeJogos.ToListAsync();
        }

        public async Task<Dictionary<Guid, int>> ProcessAndMapItemsAsync(IEnumerable<RodadaDeJogosSyncDto> dtos) {
            var idMapping = new Dictionary<Guid, int>();
            foreach (var dto in dtos) {
                var existingItem = await _context.RodadasDeJogos.FirstOrDefaultAsync(r => r.ClientAppId == dto.ClientAppId);

                if (existingItem == null) {
                    var newItem = new RodadaDeJogos {
                        ClientAppId = dto.ClientAppId,
                        NomeRodada = dto.NomeRodada,
                        IsSynced = true,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.RodadasDeJogos.Add(newItem);
                    _logger.LogInformation($"[RodadaDeJogosService] Criado novo RodadaDeJogos com ClientAppId: {newItem.ClientAppId}");
                } else {
                    existingItem.NomeRodada = dto.NomeRodada;
                    existingItem.IsSynced = true;
                    existingItem.UpdatedAt = DateTime.UtcNow;

                    _context.Entry(existingItem).State = EntityState.Modified;
                    _logger.LogInformation($"[RodadaDeJogosService] Atualizado RodadaDeJogos com ClientAppId: {existingItem.ClientAppId}");
                }
            }

            await _context.SaveChangesAsync();

            foreach (var entry in _context.ChangeTracker.Entries<RodadaDeJogos>()) {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.State == EntityState.Unchanged) {
                    idMapping[entry.Entity.ClientAppId] = entry.Entity.Id;
                }
            }

            return idMapping;
        }

        public Task UpdateForeignKeysAsync(IEnumerable<RodadaDeJogosSyncDto> items, Dictionary<string, Dictionary<Guid, int>> idMappings) {
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<RodadaDeJogosSyncDto>> GetUpdatedSinceAsync(DateTime lastSyncTime) {
            return await _context.RodadasDeJogos
                .Where(r => r.UpdatedAt > lastSyncTime)
                .Select(r => new RodadaDeJogosSyncDto {
                    ClientAppId = r.ClientAppId,
                    Id = r.Id,
                    NomeRodada = r.NomeRodada,
                    UpdatedAt = r.UpdatedAt,
                    IsSynced = r.IsSynced
                })
                .ToListAsync();
        }

        public async Task<RodadaDeJogos?> GetByIdAsync(int id) {
            return await _context.RodadasDeJogos.FindAsync(id);
        }

        public async Task AddAsync(RodadaDeJogos item) {
            item.IsSynced = true;
            item.UpdatedAt = DateTime.UtcNow;
            _context.RodadasDeJogos.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(RodadaDeJogos item) {
            _context.Entry(item).State = EntityState.Modified;
            item.IsSynced = true;
            item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}