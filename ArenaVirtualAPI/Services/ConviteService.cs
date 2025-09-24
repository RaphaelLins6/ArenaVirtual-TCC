using ArenaVirtualAPI.Data;
using ArenaVirtualAPI.Models;
using ArenaVirtualAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace ArenaVirtualAPI.Services {
    public class ConviteService : IBackendService<Convite, ConviteSyncDto> {
        private readonly ApiDbContext _context;

        public ConviteService(ApiDbContext context) {
            _context = context;
        }

        public async Task<Dictionary<Guid, int>> ProcessAndMapItemsAsync(IEnumerable<ConviteSyncDto> dtos) {
            var idMapping = new Dictionary<Guid, int>();
            foreach (var dto in dtos) {
                var existingItem = await _context.Convites.FirstOrDefaultAsync(c => c.ClientAppId == dto.ClientAppId);

                if (existingItem == null) {
                    var newItem = new Convite {
                        ClientAppId = dto.ClientAppId,
                        ConvidadoEmail = dto.ConvidadoEmail,
                        DataEnvio = dto.DataEnvio,
                        Status = dto.Status,
                        IsSynced = true,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Convites.Add(newItem);
                    idMapping[newItem.ClientAppId] = newItem.Id;
                } else {
                    existingItem.ConvidadoEmail = dto.ConvidadoEmail;
                    existingItem.DataEnvio = dto.DataEnvio;
                    existingItem.Status = dto.Status;
                    existingItem.UpdatedAt = DateTime.UtcNow;
                    existingItem.IsSynced = true;
                    _context.Entry(existingItem).State = EntityState.Modified;
                    idMapping[existingItem.ClientAppId] = existingItem.Id;
                }
            }
            return idMapping;
        }

        public async Task UpdateForeignKeysAsync(IEnumerable<ConviteSyncDto> dtos, Dictionary<string, Dictionary<Guid, int>> idMappings) {
            if (!idMappings.TryGetValue("Usuario", out var usuarioMapping)) {
                throw new InvalidOperationException("Mapeamento de Usuário não encontrado.");
            }
            if (!idMappings.TryGetValue("Time", out var timeMapping)) {
                throw new InvalidOperationException("Mapeamento de Time não encontrado.");
            }

            foreach (var dto in dtos) {
                var existingItem = await _context.Convites.FirstOrDefaultAsync(c => c.ClientAppId == dto.ClientAppId);
                if (existingItem != null) {
                    if (usuarioMapping.TryGetValue(dto.IdSolicitanteClientAppId, out var solicitanteId)) {
                        existingItem.IdSolicitanteServidor = solicitanteId;
                    }
                    if (timeMapping.TryGetValue(dto.TimeClientAppId, out var timeId)) {
                        existingItem.TimeId = timeId;
                    }
                    _context.Entry(existingItem).State = EntityState.Modified;
                }
            }
        }

        public async Task<Convite?> GetByIdAsync(int id) {
            return await _context.Convites.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(Convite item) {
            item.UpdatedAt = DateTime.UtcNow;
            _context.Convites.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Convite item) {
            item.UpdatedAt = DateTime.UtcNow;
            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ConviteSyncDto>> GetUpdatedSinceAsync(DateTime lastSyncTime) {
            return await _context.Convites
              .Where(c => c.UpdatedAt > lastSyncTime)
              .Select(c => new ConviteSyncDto {
                  ClientAppId = c.ClientAppId,
                  UpdatedAt = c.UpdatedAt,
                  ConvidadoEmail = c.ConvidadoEmail,
                  DataEnvio = c.DataEnvio,
                  IdSolicitanteClientAppId = c.Solicitante!.ClientAppId,
                  TimeClientAppId = c.Time!.ClientAppId,
                  Status = c.Status
              })
              .ToListAsync();
        }
    }
}