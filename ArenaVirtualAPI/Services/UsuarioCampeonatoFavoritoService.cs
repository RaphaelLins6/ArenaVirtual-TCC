using ArenaVirtualAPI.Data;
using ArenaVirtualAPI.Models;
using ArenaVirtualAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;

namespace ArenaVirtualAPI.Services {
    public class UsuarioCampeonatoFavoritoService : IBackendService<UsuarioCampeonatoFavorito, UsuarioCampeonatoFavoritoSyncDto> {
        private readonly ApiDbContext _context;
        private readonly ILogger<UsuarioCampeonatoFavoritoService> _logger;

        public UsuarioCampeonatoFavoritoService(ApiDbContext context, ILogger<UsuarioCampeonatoFavoritoService> logger) {
            _context = context;
            _logger = logger;
        }

        // CORREÇÃO: Este método não deve mais adicionar a entidade ao contexto.
        // Ele apenas retorna um mapa de IDs para itens que já existem no banco de dados.
        public async Task<Dictionary<Guid, int>> ProcessAndMapItemsAsync(IEnumerable<UsuarioCampeonatoFavoritoSyncDto> dtos) {
            var idMapping = new Dictionary<Guid, int>();
            var clientAppIds = dtos.Select(d => d.ClientAppId).ToList();

            var existingItems = await _context.UsuarioCampeonatoFavoritos
                .Where(f => clientAppIds.Contains(f.ClientAppId))
                .ToDictionaryAsync(f => f.ClientAppId, f => f.Id);

            foreach (var clientAppId in clientAppIds) {
                if (existingItems.TryGetValue(clientAppId, out var serverId)) {
                    idMapping[clientAppId] = serverId;
                }
            }

            return idMapping;
        }

        // CORREÇÃO: Toda a lógica de criação e atualização foi movida para cá.
        public async Task UpdateForeignKeysAsync(IEnumerable<UsuarioCampeonatoFavoritoSyncDto> dtos, Dictionary<string, Dictionary<Guid, int>> idMappings) {
            if (!idMappings.TryGetValue("Usuario", out var usuarioMapping)) {
                _logger.LogWarning("[UCF_Service] Mapeamento de IDs de Usuário não encontrado.");
                return;
            }
            if (!idMappings.TryGetValue("Campeonato", out var campeonatoMapping)) {
                _logger.LogWarning("[UCF_Service] Mapeamento de IDs de Campeonato não encontrado.");
                return;
            }

            foreach (var dto in dtos) {
                // 1. Validação para segurança
                if (dto.UsuarioClientAppId == Guid.Empty || dto.CampeonatoClientAppId == Guid.Empty) {
                    _logger.LogWarning($"[UCF_Service] Ignorando UsuarioCampeonatoFavorito (ClientAppId: {dto.ClientAppId}) com chaves estrangeiras nulas.");
                    continue;
                }

                // 2. Resolve os IDs do servidor
                usuarioMapping.TryGetValue(dto.UsuarioClientAppId, out var usuarioId);
                campeonatoMapping.TryGetValue(dto.CampeonatoClientAppId, out var campeonatoId);

                // 3. Se não encontrar os IDs, pula este registro
                if (usuarioId == 0 || campeonatoId == 0) {
                    _logger.LogWarning($"[UCF_Service] Não foi possível resolver os IDs do servidor para UsuarioCampeonatoFavorito (ClientAppId: {dto.ClientAppId}). UsuarioId: {usuarioId}, CampeonatoId: {campeonatoId}");
                    continue;
                }

                // 4. Tenta encontrar um registro existente para evitar duplicatas
                var existingItem = await _context.UsuarioCampeonatoFavoritos
                    .FirstOrDefaultAsync(f => f.UsuarioId == usuarioId && f.CampeonatoId == campeonatoId);

                if (existingItem != null) {
                    // Se já existe, apenas atualiza os metadados se necessário.
                    existingItem.ClientAppId = dto.ClientAppId; // Garante que o ClientAppId está correto
                    existingItem.UpdatedAt = DateTime.UtcNow;
                    existingItem.IsSynced = true;
                    _context.Entry(existingItem).State = EntityState.Modified;
                } else {
                    // 5. Se não existe, cria a nova entidade JÁ COM AS CHAVES CORRETAS
                    var newItem = new UsuarioCampeonatoFavorito {
                        ClientAppId = dto.ClientAppId,
                        UsuarioId = usuarioId,
                        CampeonatoId = campeonatoId,
                        UsuarioClientAppId = dto.UsuarioClientAppId,
                        CampeonatoClientAppId = dto.CampeonatoClientAppId,
                        IsSynced = true,
                        UpdatedAt = DateTime.UtcNow
                    };

                    // 6. Adiciona a entidade completa ao DbContext
                    _context.UsuarioCampeonatoFavoritos.Add(newItem);
                }
            }
        }

        // Métodos restantes (não precisam de alteração para esta correção)
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
