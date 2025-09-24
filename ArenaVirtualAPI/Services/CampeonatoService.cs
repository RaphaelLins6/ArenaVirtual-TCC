using ArenaVirtualAPI.Data;
using ArenaVirtualAPI.Services;
using Microsoft.EntityFrameworkCore;
using ArenaVirtualAPI.Models;
using ArenaVirtualAPI.DTOs;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

public class CampeonatoService : IBackendService<Campeonato, CampeonatoSyncDto> {
    private readonly ApiDbContext _context;

    public CampeonatoService(ApiDbContext context) {
        _context = context;
    }

    public async Task<Dictionary<Guid, int>> ProcessAndMapItemsAsync(IEnumerable<CampeonatoSyncDto> items) {
        var idMapping = new Dictionary<Guid, int>();
        foreach (var dto in items) {
            var existingItem = await _context.Campeonatos.FirstOrDefaultAsync(c => c.ClientAppId == dto.ClientAppId);

            if (existingItem == null) {
                var newItem = new Campeonato {
                    ClientAppId = dto.ClientAppId,
                    Nome = dto.Nome,
                    Local = dto.Local,
                    DataInicio = dto.DataInicio,
                    DataFim = dto.DataFim,
                    LogoUrl = dto.LogoUrl,
                    NomeOrganizador = dto.NomeOrganizador,
                    EmailOrganizador = dto.EmailOrganizador,
                    TelefoneOrganizador = dto.TelefoneOrganizador,
                    NumeroMaximoEquipes = dto.NumeroMaximoEquipes,
                    ValorTaxaInscricao = dto.ValorTaxaInscricao,
                    FormatoCampeonato = dto.FormatoCampeonato,
                    LocaisDosJogos = dto.LocaisDosJogos,
                    HaveraPremiacao = dto.HaveraPremiacao,
                    Descricao = dto.Descricao,
                    Modalidade = dto.Modalidade,
                    Regras = dto.Regras,
                    DataTermino = dto.DataTermino,
                    NumeroEquipes = dto.NumeroEquipes,
                    IsSynced = true,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Campeonatos.Add(newItem);
                idMapping[newItem.ClientAppId] = newItem.Id;
            } else {
                existingItem.Nome = dto.Nome;
                existingItem.Local = dto.Local;
                existingItem.DataInicio = dto.DataInicio;
                existingItem.DataFim = dto.DataFim;
                existingItem.LogoUrl = dto.LogoUrl;
                existingItem.NomeOrganizador = dto.NomeOrganizador;
                existingItem.EmailOrganizador = dto.EmailOrganizador;
                existingItem.TelefoneOrganizador = dto.TelefoneOrganizador;
                existingItem.NumeroMaximoEquipes = dto.NumeroMaximoEquipes;
                existingItem.ValorTaxaInscricao = dto.ValorTaxaInscricao;
                existingItem.FormatoCampeonato = dto.FormatoCampeonato;
                existingItem.LocaisDosJogos = dto.LocaisDosJogos;
                existingItem.HaveraPremiacao = dto.HaveraPremiacao;
                existingItem.Descricao = dto.Descricao;
                existingItem.Modalidade = dto.Modalidade;
                existingItem.Regras = dto.Regras;
                existingItem.DataTermino = dto.DataTermino;
                existingItem.NumeroEquipes = dto.NumeroEquipes;
                existingItem.UpdatedAt = DateTime.UtcNow;
                existingItem.IsSynced = true;
                _context.Entry(existingItem).State = EntityState.Modified;
                idMapping[existingItem.ClientAppId] = existingItem.Id;
            }
        }
        return idMapping;
    }

    public async Task UpdateForeignKeysAsync(IEnumerable<CampeonatoSyncDto> dtos, Dictionary<string, Dictionary<Guid, int>> idMappings) {
        if (!idMappings.TryGetValue("Usuario", out var userMappings)) {
            return;
        }
        foreach (var dto in dtos) {
            var existingItem = await _context.Campeonatos.FirstOrDefaultAsync(c => c.ClientAppId == dto.ClientAppId);
            if (existingItem != null && userMappings.TryGetValue(dto.OrganizadorClientAppId, out int newOrganizadorId)) {
                existingItem.OrganizadorId = newOrganizadorId;
                _context.Entry(existingItem).State = EntityState.Modified;
            }
        }
    }

    public async Task<Campeonato?> GetByIdAsync(int id) {
        return await _context.Campeonatos.FindAsync(id);
    }
    public async Task AddAsync(Campeonato campeonato) {
        campeonato.IsSynced = true;
        campeonato.UpdatedAt = DateTime.UtcNow;
        _context.Campeonatos.Add(campeonato);
        await _context.SaveChangesAsync();
    }
    public async Task UpdateAsync(Campeonato campeonato) {
        var existingCampeonato = await _context.Campeonatos.FindAsync(campeonato.Id);
        if (existingCampeonato != null) {
            existingCampeonato.Nome = campeonato.Nome;
            existingCampeonato.Descricao = campeonato.Descricao;
            existingCampeonato.DataInicio = campeonato.DataInicio;
            existingCampeonato.DataFim = campeonato.DataFim;
            existingCampeonato.Regras = campeonato.Regras;
            existingCampeonato.IsSynced = true;
            existingCampeonato.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
    public async Task<IEnumerable<CampeonatoSyncDto>> GetUpdatedSinceAsync(DateTime lastSyncTime) {
        return await _context.Campeonatos
          .Where(c => c.UpdatedAt > lastSyncTime)
          .Select(c => new CampeonatoSyncDto {
              ClientAppId = c.ClientAppId,
              UpdatedAt = c.UpdatedAt,
              Nome = c.Nome,
              Local = c.Local,
              DataInicio = c.DataInicio,
              DataFim = c.DataFim,
              OrganizadorClientAppId = c.Organizador!.ClientAppId,
              LogoUrl = c.LogoUrl,
              NomeOrganizador = c.NomeOrganizador,
              EmailOrganizador = c.EmailOrganizador,
              TelefoneOrganizador = c.TelefoneOrganizador,
              NumeroMaximoEquipes = c.NumeroMaximoEquipes,
              ValorTaxaInscricao = c.ValorTaxaInscricao,
              FormatoCampeonato = c.FormatoCampeonato,
              LocaisDosJogos = c.LocaisDosJogos,
              HaveraPremiacao = c.HaveraPremiacao,
              Descricao = c.Descricao,
              Modalidade = c.Modalidade,
              Regras = c.Regras,
              DataTermino = c.DataTermino,
              NumeroEquipes = c.NumeroEquipes
          })
          .ToListAsync();
    }
}