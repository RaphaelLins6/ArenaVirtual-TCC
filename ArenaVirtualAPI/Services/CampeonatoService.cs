using ArenaVirtualAPI.Data;
using ArenaVirtualAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ArenaVirtualAPI.Services {
    public class CampeonatoService : IBackendService<Campeonato> {
        private readonly ApiDbContext _context;

        public CampeonatoService(ApiDbContext context) {
            _context = context;
        }

        public async Task<Campeonato?> GetByIdAsync(int id) {
            return await _context.Campeonatos.FindAsync(id);
        }

        public async Task AddAsync(Campeonato item) {
            item.UpdatedAt = DateTime.UtcNow;
            _context.Campeonatos.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Campeonato item) {
            item.UpdatedAt = DateTime.UtcNow;
            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task ProcessItemsAsync(IEnumerable<Campeonato> items) {
            foreach (var item in items) {
                if (item.Id == 0) {
                    // Nova entrada, adiciona ao contexto
                    item.UpdatedAt = DateTime.UtcNow;
                    _context.Campeonatos.Add(item);
                } else {
                    // Atualiza uma entrada existente
                    _context.Entry(item).State = EntityState.Modified;
                    item.UpdatedAt = DateTime.UtcNow;
                }
            }
            // Salva todas as alterações de uma vez
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ISyncable>> GetUpdatedSinceAsync(DateTime lastSyncTime) {
            var updatedItems = await _context.Campeonatos
                .Where(c => c.UpdatedAt > lastSyncTime)
                .ToListAsync();

            return updatedItems.Cast<ISyncable>();
        }
    }
}