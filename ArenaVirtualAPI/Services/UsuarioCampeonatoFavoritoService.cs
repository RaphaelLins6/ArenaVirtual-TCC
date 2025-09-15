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

        // Método corrigido para usar o dicionário de mapeamentos
        public async Task<Dictionary<Guid, int>> ProcessAndMapItemsAsync(IEnumerable<UsuarioCampeonatoFavoritoSyncDto> items, Dictionary<string, Dictionary<Guid, int>> idMappings) {
            var idMapping = new Dictionary<Guid, int>();

            // Pega os dicionários de mapeamento para o tipo de entidade correto
            if (!idMappings.TryGetValue("Usuario", out var usuarioMapping)) {
                throw new InvalidOperationException("Mapeamento de Usuário não encontrado.");
            }
            if (!idMappings.TryGetValue("Campeonato", out var campeonatoMapping)) {
                throw new InvalidOperationException("Mapeamento de Campeonato não encontrado.");
            }

            foreach (var dto in items) {
                // Mapeia o ClientAppId para o ID do servidor
                if (!usuarioMapping.TryGetValue(dto.UsuarioClientAppId, out int usuarioId)) continue;
                if (!campeonatoMapping.TryGetValue(dto.CampeonatoClientAppId, out int campeonatoId)) continue;

                var existingItem = await _context.UsuarioCampeonatoFavoritos
                    .FirstOrDefaultAsync(f => f.UsuarioClientAppId == dto.UsuarioClientAppId && f.CampeonatoClientAppId == dto.CampeonatoClientAppId);

                if (existingItem == null) {
                    var newItem = new UsuarioCampeonatoFavorito {
                        ClientAppId = dto.ClientAppId,
                        // Atribui os IDs do servidor (int) às novas propriedades
                        UsuarioId = usuarioId,
                        CampeonatoId = campeonatoId,
                        // Atribui os IDs de cliente (Guid) diretamente do DTO
                        UsuarioClientAppId = dto.UsuarioClientAppId,
                        CampeonatoClientAppId = dto.CampeonatoClientAppId,
                        IsSynced = true,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.UsuarioCampeonatoFavoritos.Add(newItem);
                    await _context.SaveChangesAsync();
                    idMapping[newItem.ClientAppId] = newItem.Id;
                } else {
                    if (dto.UpdatedAt > existingItem.UpdatedAt) {
                        existingItem.UpdatedAt = DateTime.UtcNow;
                        existingItem.IsSynced = true;
                        _context.Entry(existingItem).State = EntityState.Modified;
                        await _context.SaveChangesAsync();
                    }
                    idMapping[existingItem.ClientAppId] = existingItem.Id;
                }
            }
            return idMapping;
        }
    }
}