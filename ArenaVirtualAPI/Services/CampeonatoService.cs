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

    /// <summary>
    /// Fase 1: Cria ou atualiza o Campeonato, resolvendo o OrganizadorId APENAS se o usuário JÁ existir na base.
    /// Caso o usuário seja novo e tenha sido enviado no mesmo pacote de sincronização, o OrganizadorId será resolvido na Fase 2.
    /// </summary>
    public async Task<Dictionary<Guid, int>> ProcessAndMapItemsAsync(IEnumerable<CampeonatoSyncDto> items) {
        var idMapping = new Dictionary<Guid, int>();

        // Pré-carrega os IDs de usuários para evitar múltiplas consultas no loop
        var organizadorGuids = items.Select(dto => dto.OrganizadorClientAppId).ToHashSet();
        var existingUsers = await _context.Usuarios
            .Where(u => organizadorGuids.Contains(u.ClientAppId))
            .ToDictionaryAsync(u => u.ClientAppId, u => u.Id);

        foreach (var dto in items) {
            var existingItem = await _context.Campeonatos.FirstOrDefaultAsync(c => c.ClientAppId == dto.ClientAppId);

            // Tenta obter o ID int do organizador (se ele já estiver salvo no banco)
            existingUsers.TryGetValue(dto.OrganizadorClientAppId, out int organizadorId);

            if (existingItem == null) {
                // É um novo item. OrganizadorId será resolvido agora se o usuário existir,
                // ou será 0/null para ser resolvido na Fase 2.
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
                    UpdatedAt = DateTime.UtcNow,
                    // CORREÇÃO AQUI: Se o ID foi encontrado, use. Caso contrário, use 0 (ou null se for int?)
                    OrganizadorId = organizadorId != 0 ? organizadorId : (int?)null // Ajuste para (int?)null se for nullable
                };
                _context.Campeonatos.Add(newItem);
                idMapping[newItem.ClientAppId] = newItem.Id;

            } else {
                // É uma atualização. Preserva o OrganizadorId se já existir, mas resolve se puder.
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

                // Atualiza o FK se a resolução for possível na Fase 1
                if (organizadorId != 0) {
                    existingItem.OrganizadorId = organizadorId;
                }

                _context.Entry(existingItem).State = EntityState.Modified;
                idMapping[existingItem.ClientAppId] = existingItem.Id;
            }
        }
        return idMapping;
    }

    /// <summary>
    /// Fase 2: Atualiza o OrganizadorId com o ID inteiro mapeado a partir da Fase 1,
    /// cobrindo tanto novos Campeonatos quanto atualizações onde o organizador mudou.
    /// </summary>
    public async Task UpdateForeignKeysAsync(IEnumerable<CampeonatoSyncDto> dtos, Dictionary<string, Dictionary<Guid, int>> idMappings) {
        if (!idMappings.TryGetValue("Usuario", out var userMappings)) {
            return;
        }

        // Pega todos os ClientAppIds dos campeonatos a serem atualizados
        var clientAppIds = dtos.Select(d => d.ClientAppId).ToList();

        // Carrega todos os Campeonatos que precisam de atualização de FK
        var existingItems = await _context.Campeonatos
            .Where(c => clientAppIds.Contains(c.ClientAppId))
            .ToDictionaryAsync(c => c.ClientAppId);

        foreach (var dto in dtos) {
            if (existingItems.TryGetValue(dto.ClientAppId, out var existingItem)) {

                // Tenta resolver o GUID do organizador para o INT
                if (userMappings.TryGetValue(dto.OrganizadorClientAppId, out int newOrganizadorId)) {
                    // Se o ID INT foi resolvido com sucesso
                    if (existingItem.OrganizadorId != newOrganizadorId) {
                        existingItem.OrganizadorId = newOrganizadorId;
                        _context.Entry(existingItem).State = EntityState.Modified;
                    }
                }
            }
        }
        // OBS: O SaveChangesAsync() está corretamente no BackendSyncService (Fase 3).
    }

    // ... (Outros métodos como GetByIdAsync, AddAsync, UpdateAsync, GetUpdatedSinceAsync permanecem inalterados)

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