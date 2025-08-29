using ArenaVirtualAPI.Data;
using ArenaVirtualAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

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
            var existingConvite = await _context.Convites.FindAsync(convite.Id);
            if (existingConvite != null) {
                // Atualize apenas as propriedades que devem ser sincronizadas
                existingConvite.IdSolicitante = convite.IdSolicitante;
                existingConvite.IdTime = convite.IdTime;
                existingConvite.DataEnvio = convite.DataEnvio;
                existingConvite.Status = convite.Status;

                existingConvite.IsSynced = true;
                existingConvite.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        // CORREÇÃO: O tipo de retorno é IEnumerable<ISyncable>
        public async Task<IEnumerable<ISyncable>> GetUpdatedSinceAsync(DateTime lastSyncTime) {
            // A consulta retorna Convite, mas o tipo de retorno do método é o que a interface exige (ISyncable)
            return await _context.Convites
                                 .Where(c => c.UpdatedAt > lastSyncTime)
                                 .ToListAsync();
        }

        public async Task ProcessItemsAsync(IEnumerable<Convite> items) {
            foreach (var convite in items) {
                var existingConvite = await _context.Convites.FindAsync(convite.Id);
                if (existingConvite == null) {
                    await AddAsync(convite);
                } else {
                    await UpdateAsync(convite);
                }
            }
        }
    }
}
