using ArenaVirtualAPI.Data;
using ArenaVirtualAPI.Services;
using Microsoft.EntityFrameworkCore;
using ArenaVirtualAPI.Models; // Adicione esta linha
using ArenaVirtualAPI.DTOs; // Adicione esta linha

public class TimeService : IBackendService<Time, TimeSyncDto> {
    private readonly ApiDbContext _context;

    public TimeService(ApiDbContext context) {
        _context = context;
    }

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
            existingTime.Nome = time.Nome;
            existingTime.LogoUrl = time.LogoUrl;
            existingTime.CampeonatoId = time.CampeonatoId;
            existingTime.Descricao = time.Descricao;
            existingTime.DataCriacao = time.DataCriacao;
            existingTime.Regiao = time.Regiao;
            existingTime.PontuacaoTotal = time.PontuacaoTotal;
            existingTime.Vitorias = time.Vitorias;
            existingTime.Derrotas = time.Derrotas;
            existingTime.Empates = time.Empates;
            existingTime.CapitaoId = time.CapitaoId;
            existingTime.IsSynced = true;
            existingTime.UpdatedAt = DateTime.UtcNow;
            _context.Times.Update(existingTime);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ProcessItemsAsync(IEnumerable<TimeSyncDto> items) {
        foreach (var dto in items) {
            var existingItem = await _context.Times.FindAsync(dto.Id);

            if (existingItem == null) {
                var newItem = new Time {
                    Id = dto.Id,
                    Nome = dto.Nome,
                    LogoUrl = dto.LogoUrl,
                    CampeonatoId = dto.CampeonatoId,
                    Descricao = dto.Descricao,
                    DataCriacao = dto.DataCriacao,
                    Regiao = dto.Regiao,
                    PontuacaoTotal = dto.PontuacaoTotal,
                    Vitorias = dto.Vitorias,
                    Derrotas = dto.Derrotas,
                    Empates = dto.Empates,
                    CapitaoId = dto.CapitaoId,
                    UpdatedAt = DateTime.UtcNow,
                    IsSynced = true
                };
                _context.Times.Add(newItem);
            } else {
                existingItem.Nome = dto.Nome;
                existingItem.LogoUrl = dto.LogoUrl;
                existingItem.CampeonatoId = dto.CampeonatoId;
                existingItem.Descricao = dto.Descricao;
                existingItem.DataCriacao = dto.DataCriacao;
                existingItem.Regiao = dto.Regiao;
                existingItem.PontuacaoTotal = dto.PontuacaoTotal;
                existingItem.Vitorias = dto.Vitorias;
                existingItem.Derrotas = dto.Derrotas;
                existingItem.Empates = dto.Empates;
                existingItem.CapitaoId = dto.CapitaoId;
                existingItem.UpdatedAt = DateTime.UtcNow;
                existingItem.IsSynced = true;
                _context.Entry(existingItem).State = EntityState.Modified;
            }
        }
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Time>> GetUpdatedSinceAsync(DateTime lastSyncTime) {
        return await _context.Times
          .Where(t => t.UpdatedAt > lastSyncTime)
          .ToListAsync();
    }
}