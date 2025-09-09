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
            await ProcessAndMapItemsAsync(items);
        }

        public async Task<Dictionary<Guid, int>> ProcessAndMapItemsAsync(IEnumerable<UsuarioCampeonatoFavoritoSyncDto> items) {
            var idMapping = new Dictionary<Guid, int>();

            // Coleta todos os ClientAppIds necessários para o mapeamento
            var usuarioClientAppIds = items.Select(i => i.UsuarioClientAppId).ToHashSet();
            var campeonatoClientAppIds = items.Select(i => i.CampeonatoClientAppId).ToHashSet();

            // Busca os IDs correspondentes no banco de dados
            var apiUsuarioIds = await _context.Usuarios
                .Where(u => usuarioClientAppIds.Contains(u.ClientAppId))
                .ToDictionaryAsync(u => u.ClientAppId, u => u.Id);

            var apiCampeonatoIds = await _context.Campeonatos
                .Where(c => campeonatoClientAppIds.Contains(c.ClientAppId))
                .ToDictionaryAsync(c => c.ClientAppId, c => c.Id);

            foreach (var dto in items) {
                // Mapeia os ClientAppIds para os IDs locais
                if (!apiUsuarioIds.TryGetValue(dto.UsuarioClientAppId, out int usuarioId) ||
                    !apiCampeonatoIds.TryGetValue(dto.CampeonatoClientAppId, out int campeonatoId)) {
                    // Se o usuário ou campeonato não for encontrado, pule este item.
                    continue;
                }

                // Busca por uma chave composta (UsuarioId, CampeonatoId)
                var existingItem = await _context.UsuarioCampeonatoFavoritos
                    .FirstOrDefaultAsync(f => f.UsuarioClientAppId == dto.UsuarioClientAppId && f.CampeonatoClientAppId == dto.CampeonatoClientAppId);

                // Se o item não for encontrado, crie um novo.
                if (existingItem == null) {
                    var newItem = new UsuarioCampeonatoFavorito {
                        ClientAppId = dto.ClientAppId,
                        UsuarioClientAppId = dto.UsuarioClientAppId,
                        CampeonatoClientAppId = dto.CampeonatoClientAppId,
                        IsSynced = true,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.UsuarioCampeonatoFavoritos.Add(newItem);
                    await _context.SaveChangesAsync();
                    idMapping[newItem.ClientAppId] = newItem.Id;
                } else {
                    // Se o item já existir, apenas garanta o mapeamento e a atualização
                    if (dto.UpdatedAt > existingItem.UpdatedAt) {
                        existingItem.UpdatedAt = DateTime.UtcNow;
                        existingItem.IsSynced = true;
                        _context.Entry(existingItem).State = EntityState.Modified;
                        await _context.SaveChangesAsync();
                    }
                    // Adicione o mapeamento mesmo se não houver atualização
                    idMapping[existingItem.ClientAppId] = existingItem.Id;
                }
            }
            return idMapping;
        }

        public async Task<IEnumerable<UsuarioCampeonatoFavoritoSyncDto>> GetUpdatedSinceAsync(DateTime lastSyncTime) {
            return await _context.UsuarioCampeonatoFavoritos
                .Where(f => f.UpdatedAt > lastSyncTime)
                .Select(f => new UsuarioCampeonatoFavoritoSyncDto {
                    ClientAppId = f.ClientAppId,
                    UpdatedAt = f.UpdatedAt,
                    // CORREÇÃO: Use as propriedades que existem no seu modelo
                    UsuarioClientAppId = f.UsuarioClientAppId,
                    CampeonatoClientAppId = f.CampeonatoClientAppId
                })
                .ToListAsync();
        }
    }
}