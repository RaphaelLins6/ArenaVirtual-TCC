using ArenaVirtualAPI.Data;
using ArenaVirtualAPI.Models;
using Microsoft.EntityFrameworkCore;
using ArenaVirtualAPI.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArenaVirtualAPI.Services {
    public class ConviteService : IBackendService<Convite, ConviteSyncDto> {
        private readonly ApiDbContext _context;

        public ConviteService(ApiDbContext context) {
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

        public async Task ProcessItemsAsync(IEnumerable<ConviteSyncDto> items) {
            foreach (var dto in items) {
                var convite = new Convite {
                    Id = dto.Id,
                    IdTime = dto.TimeId, // Mapeamento correto
                    ConvidadoEmail = dto.ConvidadoEmail,
                    Status = (StatusConvite)dto.StatusConvite, // Conversão de int para enum
                    UpdatedAt = DateTime.UtcNow,
                    IsSynced = true
                };

                if (convite.Id == 0) {
                    _context.Convites.Add(convite);
                } else {
                    _context.Convites.Update(convite);
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Convite>> GetUpdatedSinceAsync(DateTime lastSyncTime) {
            return await _context.Convites
                .Where(c => c.UpdatedAt > lastSyncTime)
                .ToListAsync();
        }
    }
}