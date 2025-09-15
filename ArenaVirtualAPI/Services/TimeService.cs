using ArenaVirtualAPI.Data;
using ArenaVirtualAPI.Services;
using Microsoft.EntityFrameworkCore;
using ArenaVirtualAPI.Models;
using ArenaVirtualAPI.DTOs;

public class TimeService : IBackendService<Time, TimeSyncDto> {
    private readonly ApiDbContext _context;

    public TimeService(ApiDbContext context) {
        _context = context;
    }

    // O método ProcessAndMapItemsAsync agora aceita o dicionário de mapeamentos
    public async Task<Dictionary<Guid, int>> ProcessAndMapItemsAsync(IEnumerable<TimeSyncDto> items, Dictionary<string, Dictionary<Guid, int>> idMappings) {
        var idMapping = new Dictionary<Guid, int>();

        if (!idMappings.TryGetValue("Usuario", out var userMappings)) {
            throw new InvalidOperationException("Mapeamento de Usuário não encontrado. O upload deve processar Usuários antes de Times.");
        }

        if (!idMappings.TryGetValue("Campeonato", out var campeonatoMappings)) {
            // Em alguns casos, um time pode não ter um campeonato associado no upload,
            // então não é uma exceção crítica.
            campeonatoMappings = new Dictionary<Guid, int>();
        }

        foreach (var dto in items) {
            var existingItem = await _context.Times.FirstOrDefaultAsync(t => t.ClientAppId == dto.ClientAppId);

            int? apiCapitaoId = null;
            if (dto.CapitaoClientAppId.HasValue && userMappings.TryGetValue(dto.CapitaoClientAppId.Value, out int matchedCapitaoId)) {
                apiCapitaoId = matchedCapitaoId;
            }

            int? apiCampeonatoId = null;
            if (dto.CampeonatoClientAppId.HasValue && campeonatoMappings.TryGetValue(dto.CampeonatoClientAppId.Value, out int matchedCampeonatoId)) {
                apiCampeonatoId = matchedCampeonatoId;
            }

            if (existingItem == null) {
                var newItem = new Time {
                    ClientAppId = dto.ClientAppId,
                    Nome = dto.Nome!,
                    LogoUrl = dto.LogoUrl,
                    Descricao = dto.Descricao,
                    DataCriacao = dto.DataCriacao,
                    Regiao = dto.Regiao,
                    PontuacaoTotal = dto.PontuacaoTotal,
                    Vitorias = dto.Vitorias,
                    Derrotas = dto.Derrotas,
                    Empates = dto.Empates,
                    CapitaoId = apiCapitaoId,
                    CampeonatoId = apiCampeonatoId,
                    UpdatedAt = DateTime.UtcNow,
                    IsSynced = true
                };
                _context.Times.Add(newItem);
                await _context.SaveChangesAsync();
                idMapping[newItem.ClientAppId] = newItem.Id;
            } else {
                if (dto.UpdatedAt > existingItem.UpdatedAt) {
                    existingItem.Nome = dto.Nome!;
                    existingItem.LogoUrl = dto.LogoUrl;
                    existingItem.Descricao = dto.Descricao;
                    existingItem.DataCriacao = dto.DataCriacao;
                    existingItem.Regiao = dto.Regiao;
                    existingItem.PontuacaoTotal = dto.PontuacaoTotal;
                    existingItem.Vitorias = dto.Vitorias;
                    existingItem.Derrotas = dto.Derrotas;
                    existingItem.Empates = dto.Empates;
                    existingItem.CapitaoId = apiCapitaoId;
                    existingItem.CampeonatoId = apiCampeonatoId;
                    existingItem.UpdatedAt = DateTime.UtcNow;
                    existingItem.IsSynced = true;
                    _context.Entry(existingItem).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
            }
        }
        return idMapping;
    }

    // Os outros métodos do serviço não foram alterados.
    public async Task<Time?> GetByIdAsync(int id) {
        return await _context.Times.FindAsync(id);
    }

    public async Task AddAsync(Time time) {
        time.IsSynced = true;
        time.UpdatedAt = DateTime.UtcNow;
        _context.Times.Add(time);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Time time) {
        var existingTime = await _context.Times.FindAsync(time.Id);
        if (existingTime != null) {
            _context.Entry(existingTime).CurrentValues.SetValues(time);
            existingTime.IsSynced = true;
            existingTime.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task ProcessItemsAsync(IEnumerable<TimeSyncDto> items) {
        await ProcessAndMapItemsAsync(items, new Dictionary<string, Dictionary<Guid, int>>());
    }

    public async Task<IEnumerable<TimeSyncDto>> GetUpdatedSinceAsync(DateTime lastSyncTime) {
        return await _context.Times
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
                Regiao = t.Regiao,
                PontuacaoTotal = t.PontuacaoTotal,
                Vitorias = t.Vitorias,
                Derrotas = t.Derrotas,
                Empates = t.Empates,
                CapitaoClientAppId = t.Capitao != null ? t.Capitao.ClientAppId : (Guid?)null,
                CampeonatoClientAppId = t.Campeonato != null ? t.Campeonato.ClientAppId : (Guid?)null
            })
            .ToListAsync();
    }
}