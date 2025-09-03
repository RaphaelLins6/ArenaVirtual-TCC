using ArenaVirtualAPI.Data;
using ArenaVirtualAPI.DTOs;
using ArenaVirtualAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
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
            return await _context.Convites.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(Convite item) {
            _context.Convites.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Convite item) {
            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Convite>> GetUpdatedSinceAsync(DateTime lastSyncTime) {
            return await _context.Convites
                .Where(c => c.UpdatedAt > lastSyncTime)
                .ToListAsync();
        }

        public async Task ProcessItemsAsync(IEnumerable<ConviteSyncDto> dtos) {
            foreach (var dto in dtos) {
                // Verificação de nulidade no DTO antes de qualquer operação
                if (string.IsNullOrWhiteSpace(dto.ConvidadoEmail)) {
                    // Logar ou ignorar o item, pois a propriedade obrigatória está faltando.
                    // Isso evita que a exceção seja lançada.
                    // Você pode logar um aviso aqui para depuração.
                    continue;
                }

                var existingItem = await GetByIdAsync(dto.Id);

                if (existingItem == null) {
                    // Item não existe, então crie um novo
                    var newItem = new Convite {
                        Id = dto.Id,
                        ConvidadoEmail = dto.ConvidadoEmail,
                        DataEnvio = dto.DataEnvio,
                        IdSolicitante = dto.IdSolicitante,
                        IdTime = dto.TimeId,
                        Status = dto.Status,
                        IsSynced = false,
                        UpdatedAt = dto.UpdatedAt
                    };
                    await AddAsync(newItem);
                } else {
                    // Item já existe, então atualize se o DTO for mais recente
                    if (dto.UpdatedAt > existingItem.UpdatedAt) {
                        existingItem.ConvidadoEmail = dto.ConvidadoEmail;
                        existingItem.DataEnvio = dto.DataEnvio;
                        existingItem.IdSolicitante = dto.IdSolicitante;
                        existingItem.IdTime = dto.TimeId;
                        existingItem.Status = dto.Status;
                        existingItem.UpdatedAt = dto.UpdatedAt;
                        await UpdateAsync(existingItem);
                    }
                }
            }
        }
    }
}