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
    public class JogoService : IBackendService<Jogo, JogoSyncDto> {
        private readonly ApiDbContext _context;
        private readonly ILogger<JogoService> _logger;

        public JogoService(ApiDbContext context, ILogger<JogoService> logger) {
            _context = context;
            _logger = logger;
        }

        public async Task<Dictionary<Guid, int>> ProcessAndMapItemsAsync(IEnumerable<JogoSyncDto> dtos) {
            var idMapping = new Dictionary<Guid, int>();
            foreach (var dto in dtos) {
                var existingItem = await _context.Jogos.FirstOrDefaultAsync(j => j.ClientAppId == dto.ClientAppId);

                if (existingItem == null) {
                    var newItem = new Jogo {
                        ClientAppId = dto.ClientAppId,
                        DataHora = dto.DataHora,
                        Local = dto.Local,
                        PlacarA = dto.PlacarA,
                        PlacarB = dto.PlacarB,
                        IsSynced = true,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Jogos.Add(newItem);
                } else {
                    existingItem.DataHora = dto.DataHora;
                    existingItem.Local = dto.Local;
                    existingItem.PlacarA = dto.PlacarA;
                    existingItem.PlacarB = dto.PlacarB;
                    existingItem.IsSynced = true;
                    existingItem.UpdatedAt = DateTime.UtcNow;
                    _context.Entry(existingItem).State = EntityState.Modified;
                }
            }

            await _context.SaveChangesAsync();

            foreach (var dto in dtos) {
                var itemNoDb = await _context.Jogos.AsNoTracking().FirstOrDefaultAsync(j => j.ClientAppId == dto.ClientAppId);
                if (itemNoDb != null) {
                    idMapping[dto.ClientAppId] = itemNoDb.Id;
                }
            }
            return idMapping;
        }

        public async Task UpdateForeignKeysAsync(IEnumerable<JogoSyncDto> dtos, Dictionary<string, Dictionary<Guid, int>> idMappings) {
            if (!idMappings.TryGetValue("Time", out var timeMapping) ||
                !idMappings.TryGetValue("Campeonato", out var campeonatoMapping) ||
                !idMappings.TryGetValue("Usuario", out var usuarioMapping)) {
                _logger.LogWarning("[JogoService] Mapeamento de IDs para Time, Campeonato ou Usuario não encontrado.");
                return;
            }

            foreach (var dto in dtos) {
                var jogo = await _context.Jogos.FirstOrDefaultAsync(j => j.ClientAppId == dto.ClientAppId);
                if (jogo != null) {
                    if (timeMapping.TryGetValue(dto.TimeAClientAppId, out int timeAId)) {
                        jogo.TimeAId = timeAId;
                    }
                    if (timeMapping.TryGetValue(dto.TimeBClientAppId, out int timeBId)) {
                        jogo.TimeBId = timeBId;
                    }
                    if (campeonatoMapping.TryGetValue(dto.CampeonatoClientAppId, out int campeonatoId)) {
                        jogo.CampeonatoId = campeonatoId;
                    }
                    if (usuarioMapping.TryGetValue((Guid)dto.ArbitroClientAppId, out int arbitroId)) {
                        jogo.ArbitroId = arbitroId;
                    }
                    _context.Entry(jogo).State = EntityState.Modified;
                }
            }
        }

        public async Task<IEnumerable<JogoSyncDto>> GetUpdatedSinceAsync(DateTime lastSyncTime) {
            return await _context.Jogos
                .AsNoTracking()
                .Where(j => j.UpdatedAt > lastSyncTime)
                .Select(j => new JogoSyncDto {
                    ClientAppId = j.ClientAppId,
                    DataHora = j.DataHora,
                    Local = j.Local,
                    PlacarA = j.PlacarA,
                    PlacarB = j.PlacarB,
                    // CORREÇÃO FINAL: A consulta agora retorna Guid, usando Guid.Empty como padrão se a FK for nula.
                    TimeAClientAppId = _context.Time.Where(t => t.Id == j.TimeAId).Select(t => t.ClientAppId).FirstOrDefault(),
                    TimeBClientAppId = _context.Time.Where(t => t.Id == j.TimeBId).Select(t => t.ClientAppId).FirstOrDefault(),
                    CampeonatoClientAppId = _context.Campeonatos.Where(c => c.Id == j.CampeonatoId).Select(c => c.ClientAppId).FirstOrDefault(),
                    ArbitroClientAppId = _context.Usuarios.Where(u => u.Id == j.ArbitroId).Select(u => u.ClientAppId).FirstOrDefault(),
                    UpdatedAt = j.UpdatedAt,
                    IsSynced = j.IsSynced
                })
                .ToListAsync();
        }

        // Métodos da interface não alterados
        public Task<Jogo?> GetByIdAsync(int id) => _context.Jogos.FindAsync(id).AsTask();
        public async Task AddAsync(Jogo item) {
            _context.Jogos.Add(item);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Jogo item) {
            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
