using ArenaVirtualAPI.Data;
using ArenaVirtualAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using ArenaVirtualAPI.Services;
using ArenaVirtualAPI.Models;


public class TimeService : IBackendService<Time, TimeSyncDto> {
    private readonly ApiDbContext _context;

    public TimeService(ApiDbContext context) {
        _context = context;
    }

    public async Task<Time?> GetByIdAsync(int id) {
        return await _context.Time.FindAsync(id);
    }

    public async Task AddAsync(Time time) {
        time.IsSynced = true;
        time.UpdatedAt = DateTime.UtcNow;
        _context.Time.Add(time);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Time time) {
        var existingTime = await _context.Time.FindAsync(time.Id);
        if (existingTime != null) {
            _context.Entry(existingTime).CurrentValues.SetValues(time);
            existingTime.IsSynced = true;
            existingTime.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    // Primeira fase: upsert (criação/atualização)
    public async Task<Dictionary<Guid, int>> ProcessAndMapItemsAsync(IEnumerable<TimeSyncDto> items) {
        var idMapping = new Dictionary<Guid, int>();
        foreach (var dto in items) {
            var existingItem = await _context.Time.FirstOrDefaultAsync(t => t.ClientAppId == dto.ClientAppId);

            if (existingItem == null) {
                var newItem = new Time {
                    ClientAppId = dto.ClientAppId,
                    Nome = dto.Nome!,
                    LogoUrl = dto.LogoUrl,
                    Descricao = dto.Descricao,
                    DataCriacao = dto.DataCriacao,
                    UpdatedAt = DateTime.UtcNow,
                    IsSynced = true
                };
                _context.Time.Add(newItem);
                idMapping[newItem.ClientAppId] = newItem.Id;
            } else {
                if (dto.UpdatedAt > existingItem.UpdatedAt) {
                    existingItem.Nome = dto.Nome!;
                    existingItem.LogoUrl = dto.LogoUrl;
                    existingItem.Descricao = dto.Descricao;
                    existingItem.DataCriacao = dto.DataCriacao;
                    existingItem.UpdatedAt = DateTime.UtcNow;
                    existingItem.IsSynced = true;
                    _context.Entry(existingItem).State = EntityState.Modified;
                }
                idMapping[existingItem.ClientAppId] = existingItem.Id;
            }
        }
        // CORREÇÃO: Remova o _context.SaveChangesAsync() daqui.
        return idMapping;
    }

    // Segunda fase: atualização de chaves estrangeiras
    public async Task UpdateForeignKeysAsync(IEnumerable<TimeSyncDto> dtos, Dictionary<string, Dictionary<Guid, int>> idMappings) {

        if (!idMappings.TryGetValue("Usuario", out var userMappings)) {
            return;
        }
        if (!idMappings.TryGetValue("Campeonato", out var campeonatoMappings)) {
            return;
        }

        foreach (var dto in dtos) {
            var existingItem = await _context.Time.FirstOrDefaultAsync(t => t.ClientAppId == dto.ClientAppId);

            if (existingItem != null) {

                // CORREÇÃO: Inicialize as variáveis locais
                int newCapitaoId = 0;   // Inicialize com 0 (ou outro valor padrão)
                int newCampeonatoId = 0; // Inicialize com 0

                // --- 1. Mapeamento de CapitaoId (Obrigatório) ---
                bool capitaoFound = dto.CapitaoClientAppId.HasValue &&
                                    userMappings.TryGetValue(dto.CapitaoClientAppId.Value, out newCapitaoId);
            }
        }
    }

    // GetUpdatedSinceAsync está correto e não precisa de alterações.
    public async Task<IEnumerable<TimeSyncDto>> GetUpdatedSinceAsync(DateTime lastSyncTime) {
        return await _context.Time
            .Include(t => t.Capitao)
            .Include(t => t.Campeonato)
            .Where(t => t.UpdatedAt > lastSyncTime)
            .Select(t => new TimeSyncDto {
                ClientAppId = t.ClientAppId,
                UpdatedAt = t.UpdatedAt,
                Nome = t.Nome,
                LogoUrl = t.LogoUrl,
                Descricao = t.Descricao,
                DataCriacao = t.DataCriacao,
                CapitaoClientAppId = t.Capitao != null ? t.Capitao.ClientAppId : (Guid?)null,
            })
            .ToListAsync();
    }
}