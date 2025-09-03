using ArenaVirtualAPI.Data;
using ArenaVirtualAPI.DTOs;
using ArenaVirtualAPI.Models;
using ArenaVirtualAPI.Services;
using Microsoft.EntityFrameworkCore;

public class CampeonatoService : IBackendService<Campeonato, CampeonatoSyncDto> {
    private readonly ApiDbContext _context;

    public CampeonatoService(ApiDbContext context) {
        _context = context;
    }

    public async Task<Campeonato?> GetByIdAsync(int id) {
        return await _context.Campeonatos.FindAsync(id);
    }

    public async Task AddAsync(Campeonato item) {
        item.UpdatedAt = DateTime.UtcNow;
        _context.Campeonatos.Add(item);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Campeonato item) {
        item.UpdatedAt = DateTime.UtcNow;
        _context.Entry(item).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task ProcessItemsAsync(IEnumerable<CampeonatoSyncDto> items) {
        foreach (var dto in items) {
            var existingItem = await _context.Campeonatos.FindAsync(dto.Id);

            if (existingItem == null) {
                var newItem = new Campeonato {
                    Id = dto.Id,
                    Nome = dto.Nome,
                    Local = dto.Local,
                    DataInicio = dto.DataInicio,
                    DataFim = dto.DataFim,
                    OrganizadorId = dto.OrganizadorId,
                    LogoUrl = dto.LogoUrl,
                    NomeOrganizador = dto.NomeOrganizador,
                    EmailOrganizador = dto.EmailOrganizador,
                    TelefoneOrganizador = dto.TelefoneOrganizador,
                    NumeroMaximoEquipes = dto.NumeroMaximoEquipes,
                    ValorTaxaInscricao = dto.ValorTaxaInscricao,
                    FormatoCampeonato = dto.FormatoCampeonato,
                    LocaisDosJogos = dto.LocaisDosJogos,
                    HaveraPremiacao = dto.HaveraPremiacao,
                    UpdatedAt = DateTime.UtcNow,
                    IsSynced = true
                };
                _context.Campeonatos.Add(newItem);
            } else {
                // Verificação crucial: só atualiza se a versão do DTO for mais recente.
                if (dto.UpdatedAt > existingItem.UpdatedAt) {
                    existingItem.Nome = dto.Nome;
                    existingItem.Local = dto.Local;
                    existingItem.DataInicio = dto.DataInicio;
                    existingItem.DataFim = dto.DataFim;
                    existingItem.OrganizadorId = dto.OrganizadorId;
                    existingItem.LogoUrl = dto.LogoUrl;
                    existingItem.NomeOrganizador = dto.NomeOrganizador;
                    existingItem.EmailOrganizador = dto.EmailOrganizador;
                    existingItem.TelefoneOrganizador = dto.TelefoneOrganizador;
                    existingItem.NumeroMaximoEquipes = dto.NumeroMaximoEquipes;
                    existingItem.ValorTaxaInscricao = dto.ValorTaxaInscricao;
                    existingItem.FormatoCampeonato = dto.FormatoCampeonato;
                    existingItem.LocaisDosJogos = dto.LocaisDosJogos;
                    existingItem.HaveraPremiacao = dto.HaveraPremiacao;
                    existingItem.UpdatedAt = DateTime.UtcNow;
                    existingItem.IsSynced = true;
                    _context.Entry(existingItem).State = EntityState.Modified;
                }
            }
        }
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Campeonato>> GetUpdatedSinceAsync(DateTime lastSyncTime) {
        var updatedItems = await _context.Campeonatos
            .Where(c => c.UpdatedAt > lastSyncTime)
            .ToListAsync();

        return updatedItems;
    }
}