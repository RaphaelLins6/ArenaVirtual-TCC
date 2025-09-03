// Em ArenaVirtualAPI/Services/UsuarioCampeonatoFavoritoService.cs

using ArenaVirtualAPI.Data;
using ArenaVirtualAPI.Models;
using ArenaVirtualAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArenaVirtualAPI.Services {

    public class UsuarioCampeonatoFavoritoService : IBackendService<UsuarioCampeonatoFavorito, UsuarioCampeonatoFavoritoSyncDto> {
        private readonly ApiDbContext _context;

        public UsuarioCampeonatoFavoritoService(ApiDbContext context) {
            _context = context;
        }

        public async Task<UsuarioCampeonatoFavorito?> GetByIdAsync(int id) {
            return await _context.UsuarioCampeonatoFavoritos.FindAsync(id);
        }

        public async Task AddAsync(UsuarioCampeonatoFavorito item) {
            item.IsSynced = true;
            item.UpdatedAt = DateTime.UtcNow;
            _context.UsuarioCampeonatoFavoritos.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UsuarioCampeonatoFavorito item) {
            _context.Entry(item).State = EntityState.Modified;
            item.IsSynced = true;
            item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task ProcessItemsAsync(IEnumerable<UsuarioCampeonatoFavoritoSyncDto> items) {
            foreach (var dto in items) {
                // Verifique se a combinação de UsuarioId e CampeonatoId já existe
                var existingItem = await _context.UsuarioCampeonatoFavoritos
                    .FirstOrDefaultAsync(f => f.UsuarioId == dto.UsuarioId && f.CampeonatoId == dto.CampeonatoId);

                if (existingItem == null) {
                    // Adiciona um novo favorito se ele não existir
                    var newItem = new UsuarioCampeonatoFavorito {
                        // O Id será gerado automaticamente pelo banco de dados, não o inclua aqui.
                        UsuarioId = dto.UsuarioId,
                        CampeonatoId = dto.CampeonatoId,
                        UpdatedAt = DateTime.UtcNow,
                        IsSynced = true
                    };
                    _context.UsuarioCampeonatoFavoritos.Add(newItem);
                } else {
                    // Se o item já existir, atualiza apenas se o DTO for mais recente
                    if (dto.UpdatedAt > existingItem.UpdatedAt) {
                        existingItem.UpdatedAt = DateTime.UtcNow;
                        existingItem.IsSynced = true;
                        _context.Entry(existingItem).State = EntityState.Modified;
                    }
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<UsuarioCampeonatoFavorito>> GetUpdatedSinceAsync(DateTime lastSyncTime) {
            return await _context.UsuarioCampeonatoFavoritos
                .Where(f => f.UpdatedAt > lastSyncTime)
                .ToListAsync();
        }
    }
}