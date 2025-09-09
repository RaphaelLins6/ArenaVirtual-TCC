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

        public async Task ProcessItemsAsync(IEnumerable<ConviteSyncDto> dtos) {
            await ProcessAndMapItemsAsync(dtos);
        }

        // AQUI ESTÁ A CORREÇÃO PRINCIPAL: TIPO DE RETORNO DO DICIONÁRIO E MAPEAMENTO
        public async Task<Dictionary<Guid, int>> ProcessAndMapItemsAsync(IEnumerable<ConviteSyncDto> dtos) {
            var idMapping = new Dictionary<Guid, int>();

            // Coleta os ClientAppIds para buscar em massa
            var solicitanteClientAppIds = dtos.Select(d => d.IdSolicitanteClientAppId).ToHashSet();
            var timeClientAppIds = dtos.Select(d => d.TimeClientAppId).ToHashSet();

            // Busca os IDs de referência do servidor em uma única operação
            var solicitantes = await _context.Usuarios
                .Where(u => solicitanteClientAppIds.Contains(u.ClientAppId))
                .ToDictionaryAsync(u => u.ClientAppId, u => u.Id);

            var times = await _context.Times
                .Where(t => timeClientAppIds.Contains(t.ClientAppId))
                .ToDictionaryAsync(t => t.ClientAppId, t => t.Id);

            foreach (var dto in dtos) {
                // Valida se as entidades referenciadas existem na API
                if (!solicitantes.TryGetValue(dto.IdSolicitanteClientAppId, out var solicitanteId)) {
                    // Logar ou lidar com o erro de entidade não encontrada, se necessário.
                    continue;
                }

                if (!times.TryGetValue(dto.TimeClientAppId, out var timeId)) {
                    // Logar ou lidar com o erro de entidade não encontrada, se necessário.
                    continue;
                }

                var existingItem = await _context.Convites.FirstOrDefaultAsync(c => c.ClientAppId == dto.ClientAppId);

                if (existingItem == null) {
                    var newItem = new Convite {
                        ClientAppId = dto.ClientAppId,
                        ConvidadoEmail = dto.ConvidadoEmail,
                        DataEnvio = dto.DataEnvio,
                        // ATRIBUI OS IDS MAPEADOS DO SERVIDOR
                        IdSolicitanteServidor = solicitanteId,
                        TimeId = timeId,
                        Status = dto.Status,
                        IsSynced = true,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Convites.Add(newItem);
                    await _context.SaveChangesAsync();
                    idMapping[newItem.ClientAppId] = newItem.Id;
                } else {
                    if (dto.UpdatedAt > existingItem.UpdatedAt) {
                        existingItem.ConvidadoEmail = dto.ConvidadoEmail;
                        existingItem.DataEnvio = dto.DataEnvio;
                        // ATRIBUI OS IDS MAPEADOS DO SERVIDOR
                        existingItem.IdSolicitanteServidor = solicitanteId;
                        existingItem.TimeId = timeId;
                        existingItem.Status = dto.Status;
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