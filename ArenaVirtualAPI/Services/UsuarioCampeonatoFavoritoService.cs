using ArenaVirtualAPI.Data;
using ArenaVirtualAPI.Models;
using ArenaVirtualAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace ArenaVirtualAPI.Services {
    public class UsuarioCampeonatoFavoritoService : IBackendService<UsuarioCampeonatoFavorito, UsuarioCampeonatoFavoritoSyncDto> {
        private readonly ApiDbContext _context;

        public UsuarioCampeonatoFavoritoService(ApiDbContext context) {
            _context = context;
        }

        public async Task<Dictionary<Guid, int>> ProcessAndMapItemsAsync(IEnumerable<UsuarioCampeonatoFavoritoSyncDto> items) {
            var idMapping = new Dictionary<Guid, int>();
            foreach (var dto in items) {
                var existingItem = await _context.UsuarioCampeonatoFavoritos
                  .FirstOrDefaultAsync(f => f.ClientAppId == dto.ClientAppId);

                if (existingItem == null) {
                    var newItem = new UsuarioCampeonatoFavorito {
                        ClientAppId = dto.ClientAppId,
                        IsSynced = true,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.UsuarioCampeonatoFavoritos.Add(newItem);
                    idMapping[newItem.ClientAppId] = newItem.Id;
                } else {
                    existingItem.UpdatedAt = DateTime.UtcNow;
                    existingItem.IsSynced = true;
                    _context.Entry(existingItem).State = EntityState.Modified;
                    idMapping[existingItem.ClientAppId] = existingItem.Id;
                }
            }
            return idMapping;
        }

        public async Task UpdateForeignKeysAsync(IEnumerable<UsuarioCampeonatoFavoritoSyncDto> dtos, Dictionary<string, Dictionary<Guid, int>> idMappings) {
            if (!idMappings.TryGetValue("Usuario", out var usuarioMapping)) {
                return;
            }
            if (!idMappings.TryGetValue("Campeonato", out var campeonatoMapping)) {
                return;
            }

            foreach (var dto in dtos) {
                var existingItem = await _context.UsuarioCampeonatoFavoritos
                  .FirstOrDefaultAsync(f => f.ClientAppId == dto.ClientAppId);

                if (existingItem != null) {
                    if (usuarioMapping.TryGetValue(dto.UsuarioClientAppId, out int usuarioId) &&
                      campeonatoMapping.TryGetValue(dto.CampeonatoClientAppId, out int campeonatoId)) {
                        existingItem.UsuarioId = usuarioId;
                        existingItem.CampeonatoId = campeonatoId;
                        _context.Entry(existingItem).State = EntityState.Modified;
                    }
                }
            }
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

        public async Task<IEnumerable<UsuarioCampeonatoFavoritoSyncDto>> GetUpdatedSinceAsync(DateTime lastSyncTime) {
            return await _context.UsuarioCampeonatoFavoritos
              .Where(f => f.UpdatedAt > lastSyncTime)
              .Select(f => new UsuarioCampeonatoFavoritoSyncDto {
                  ClientAppId = f.ClientAppId,
                  UpdatedAt = f.UpdatedAt,
                  UsuarioClientAppId = f.UsuarioClientAppId,
                  CampeonatoClientAppId = f.CampeonatoClientAppId
              })
              .ToListAsync();
        }
    }
}