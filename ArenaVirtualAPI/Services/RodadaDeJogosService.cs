using ArenaVirtualAPI.Data;
using ArenaVirtualAPI.DTOs;
using ArenaVirtualAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArenaVirtualAPI.Services {
    // Implementa IBackendService<Modelo, DTO>
    public class RodadaDeJogosService : IBackendService<RodadaDeJogos, RodadaDeJogosSyncDto> {
        private readonly ApiDbContext _context;
        private readonly ILogger<RodadaDeJogosService> _logger;

        public RodadaDeJogosService(ApiDbContext context, ILogger<RodadaDeJogosService> logger) {
            _context = context;
            _logger = logger;
        }

        // ---------------------------------------------------------------------
        // FASE 1: Processa e Mapeia (Upsert)
        // Cria ou atualiza RodadaDeJogos, retornando o mapeamento de IDs.
        // ---------------------------------------------------------------------
        public async Task<Dictionary<Guid, int>> ProcessAndMapItemsAsync(IEnumerable<RodadaDeJogosSyncDto> dtos) {
            var idMapping = new Dictionary<Guid, int>();
            foreach (var dto in dtos) {
                var existingItem = await _context.RodadasDeJogos.FirstOrDefaultAsync(r => r.ClientAppId == dto.ClientAppId);

                if (existingItem == null) {
                    var newItem = new RodadaDeJogos {
                        ClientAppId = dto.ClientAppId,
                        NomeRodada = dto.NomeRodada,
                        IsSynced = true,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.RodadasDeJogos.Add(newItem);
                    _logger.LogInformation($"[RodadaDeJogosService] Criado novo RodadaDeJogos com ClientAppId: {newItem.ClientAppId}");
                } else {
                    // Atualiza campos
                    existingItem.NomeRodada = dto.NomeRodada;
                    existingItem.IsSynced = true;
                    existingItem.UpdatedAt = DateTime.UtcNow;

                    _context.Entry(existingItem).State = EntityState.Modified;
                    _logger.LogInformation($"[RodadaDeJogosService] Atualizado RodadaDeJogos com ClientAppId: {existingItem.ClientAppId}");
                }
            }

            // Salva as alterações no banco de dados para que os IDs sejam gerados
            await _context.SaveChangesAsync();

            // Popula o mapa de IDs com os IDs gerados/existentes
            foreach (var entry in _context.ChangeTracker.Entries<RodadaDeJogos>()) {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.State == EntityState.Unchanged) {
                    idMapping[entry.Entity.ClientAppId] = entry.Entity.Id;
                }
            }

            return idMapping;
        }

        // ---------------------------------------------------------------------
        // FASE 2: Atualização de Chaves Estrangeiras
        // RodadaDeJogos não possui FKs de outras entidades, apenas uma coleção (1-N) de Jogos, 
        // que é resolvida no JogoService.
        // ---------------------------------------------------------------------
        public Task UpdateForeignKeysAsync(IEnumerable<RodadaDeJogosSyncDto> items, Dictionary<string, Dictionary<Guid, int>> idMappings) {
            // Não há chaves estrangeiras a serem resolvidas nesta entidade
            return Task.CompletedTask;
        }

        // ---------------------------------------------------------------------
        // GetUpdatedSinceAsync (Busca por atualizações para o cliente)
        // ---------------------------------------------------------------------
        public async Task<IEnumerable<RodadaDeJogosSyncDto>> GetUpdatedSinceAsync(DateTime lastSyncTime) {
            return await _context.RodadasDeJogos
                .Where(r => r.UpdatedAt > lastSyncTime)
                .Select(r => new RodadaDeJogosSyncDto {
                    ClientAppId = r.ClientAppId,
                    Id = r.Id,
                    NomeRodada = r.NomeRodada,
                    UpdatedAt = r.UpdatedAt,
                    IsSynced = r.IsSynced
                })
                .ToListAsync();
        }

        // ---------------------------------------------------------------------
        // Outros Métodos CRUD (padrão)
        // ---------------------------------------------------------------------
        public async Task<RodadaDeJogos?> GetByIdAsync(int id) {
            return await _context.RodadasDeJogos.FindAsync(id);
        }

        public async Task AddAsync(RodadaDeJogos item) {
            item.IsSynced = true;
            item.UpdatedAt = DateTime.UtcNow;
            _context.RodadasDeJogos.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(RodadaDeJogos item) {
            _context.Entry(item).State = EntityState.Modified;
            item.IsSynced = true;
            item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}