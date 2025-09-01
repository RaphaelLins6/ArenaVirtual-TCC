using ArenaVirtualAPI.Data;
using ArenaVirtualAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArenaVirtualAPI.Services {
    public class ConviteService : IBackendService<Convite> {
        private readonly AppDbContext _context;

        public ConviteService(AppDbContext context) {
            _context = context;
        }

        public async Task<Convite?> GetByIdAsync(int id) {
            return await _context.Convites.FindAsync(id);
        }

        public async Task AddAsync(Convite convite) {
            convite.IsSynced = true;
            convite.UpdatedAt = DateTime.UtcNow;
            _context.Convites.Add(convite);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Convite convite) {
            _context.Entry(convite).State = EntityState.Modified;
            convite.IsSynced = true;
            convite.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task ProcessItemsAsync(IEnumerable<Convite> items) {
            foreach (var convite in items) {
                // Se o Id for 0, é um novo convite. Caso contrário, é uma atualização.
                if (convite.Id == 0) {
                    convite.UpdatedAt = DateTime.UtcNow;
                    _context.Convites.Add(convite);
                } else {
                    // Anexa o item e marca-o como modificado.
                    _context.Convites.Update(convite);
                    convite.UpdatedAt = DateTime.UtcNow; // Garante que o timestamp é definido no backend
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ISyncable>> GetUpdatedSinceAsync(DateTime lastSyncTime) {
            return await _context.Convites
                                 .Where(c => c.UpdatedAt > lastSyncTime)
                                 .ToListAsync();
        }
    }
}