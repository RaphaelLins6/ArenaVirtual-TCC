using ArenaVirtualAPI.Data;
using ArenaVirtualAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArenaVirtualAPI.Services {
    // A classe de serviço para Campeonatos.
    public class CampeonatoService : IBackendService<Campeonato> {
        private readonly AppDbContext _context;

        public CampeonatoService(AppDbContext context) {
            _context = context;
        }

        // Método para obter um Campeonato por ID
        public async Task<Campeonato?> GetByIdAsync(int id) {
            return await _context.Campeonatos.FindAsync(id);
        }

        // Método para adicionar um Campeonato
        public async Task AddAsync(Campeonato item) {
            _context.Campeonatos.Add(item);
            await _context.SaveChangesAsync();
        }

        // Método para atualizar um Campeonato
        public async Task UpdateAsync(Campeonato item) {
            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        // Método para processar a lista de Campeonatos
        public async Task ProcessItemsAsync(IEnumerable<Campeonato> items) {
            foreach (var item in items) {
                var existingItem = await _context.Campeonatos.FindAsync(item.Id);
                if (existingItem == null) {
                    await AddAsync(item);
                } else {
                    _context.Entry(existingItem).CurrentValues.SetValues(item);
                    await UpdateAsync(existingItem);
                }
            }
        }

        // Método para obter itens atualizados, com a assinatura correta
        public async Task<IEnumerable<ISyncable>> GetUpdatedSinceAsync(DateTime lastSyncTime) {
            var updatedItems = await _context.Campeonatos
                .Where(c => c.UpdatedAt > lastSyncTime)
                .ToListAsync();

            return updatedItems;
        }
    }
}
