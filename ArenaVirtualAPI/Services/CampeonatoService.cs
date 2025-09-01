using ArenaVirtualAPI.Data;
using ArenaVirtualAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace ArenaVirtualAPI.Services {
    public class CampeonatoService : IBackendService<Campeonato> {
        private readonly AppDbContext _context;

        public CampeonatoService(AppDbContext context) {
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
                // Se o Id é 0, é uma nova entrada. Caso contrário, é uma atualização.
                if (item.Id == 0) {
                    item.UpdatedAt = DateTime.UtcNow;
                    _context.Campeonatos.Add(item);
                } else {
                    item.UpdatedAt = DateTime.UtcNow;
                    _context.Campeonatos.Update(item);
                }
            }
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