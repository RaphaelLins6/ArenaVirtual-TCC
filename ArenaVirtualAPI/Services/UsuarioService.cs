using ArenaVirtualAPI.Data;
using ArenaVirtualAPI.Services;
using Microsoft.EntityFrameworkCore;
using ArenaVirtualAPI.Models;
using ArenaVirtualAPI.DTOs;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

public class UsuarioService : IBackendService<Usuario, UsuarioSyncDto> {
    private readonly ApiDbContext _context;

    public UsuarioService(ApiDbContext context) {
        _context = context;
    }

    // Primeira fase: upsert (criação/atualização)
    public async Task<Dictionary<Guid, int>> ProcessAndMapItemsAsync(IEnumerable<UsuarioSyncDto> dtos) {
        var idMapping = new Dictionary<Guid, int>();
        foreach (var dto in dtos) {
            var existingItem = await _context.Usuarios.FirstOrDefaultAsync(u => u.ClientAppId == dto.ClientAppId);

            if (existingItem == null) {
                var newItem = new Usuario {
                    ClientAppId = dto.ClientAppId,
                    Nome = dto.Nome,
                    Email = dto.Email,
                    Perfil = dto.Perfil,
                    ImagemPath = dto.ImagemPath ?? string.Empty,
                    Localizacao = dto.Localizacao ?? string.Empty,
                    Telefone = dto.Telefone ?? string.Empty,
                    LinkRedeSocial = dto.LinkRedeSocial ?? string.Empty,
                    DataNascimento = dto.DataNascimento,
                    Genero = dto.Genero,
                    NomeEmpresa = dto.NomeEmpresa ?? string.Empty,
                    CNPJ = dto.CNPJ ?? string.Empty,
                    Peso = dto.Peso,
                    Altura = dto.Altura,
                    FaixaOrcamentoPatrocinio = dto.FaixaOrcamentoPatrocinio ?? string.Empty,
                    UpdatedAt = DateTime.UtcNow,
                    IsSynced = true,
                    SenhaHash = "not_available"
                };
                _context.Usuarios.Add(newItem);
                idMapping[newItem.ClientAppId] = newItem.Id;
            } else {
                existingItem.Nome = dto.Nome ?? existingItem.Nome;
                existingItem.Perfil = dto.Perfil;
                existingItem.ImagemPath = dto.ImagemPath ?? existingItem.ImagemPath;
                existingItem.Localizacao = dto.Localizacao ?? existingItem.Localizacao;
                existingItem.Telefone = dto.Telefone ?? existingItem.Telefone;
                existingItem.LinkRedeSocial = dto.LinkRedeSocial ?? existingItem.LinkRedeSocial;
                existingItem.DataNascimento = dto.DataNascimento ?? existingItem.DataNascimento;
                existingItem.Genero = dto.Genero ?? existingItem.Genero;
                existingItem.NomeEmpresa = dto.NomeEmpresa ?? existingItem.NomeEmpresa;
                existingItem.CNPJ = dto.CNPJ ?? existingItem.CNPJ;
                existingItem.Peso = dto.Peso ?? existingItem.Peso;
                existingItem.Altura = dto.Altura ?? existingItem.Altura;
                existingItem.FaixaOrcamentoPatrocinio = dto.FaixaOrcamentoPatrocinio ?? existingItem.FaixaOrcamentoPatrocinio;
                existingItem.UpdatedAt = DateTime.UtcNow;
                existingItem.IsSynced = true;
                _context.Entry(existingItem).State = EntityState.Modified;
                idMapping[existingItem.ClientAppId] = existingItem.Id;
            }
        }
        return idMapping;
    }

    // Segunda fase: atualização de chaves estrangeiras
    public async Task UpdateForeignKeysAsync(IEnumerable<UsuarioSyncDto> dtos, Dictionary<string, Dictionary<Guid, int>> idMappings) {
        if (!idMappings.TryGetValue("Time", out var timeMappings)) {
            return;
        }

        // Pré-carregar os Usuários para evitar múltiplas consultas
        var clientAppIds = dtos.Select(d => d.ClientAppId).ToHashSet();
        var existingItems = await _context.Usuarios
            .Where(u => clientAppIds.Contains(u.ClientAppId))
            .ToDictionaryAsync(u => u.ClientAppId);

        foreach (var dto in dtos) {
            if (existingItems.TryGetValue(dto.ClientAppId, out var existingItem)) {
                if (dto.TimeClientAppId.HasValue && dto.TimeClientAppId.Value != Guid.Empty) {
                    // Tenta resolver o ClientAppId para o ID inteiro
                    if (timeMappings.TryGetValue(dto.TimeClientAppId.Value, out int newTimeId)) {
                        existingItem.TimeId = newTimeId;
                    } else {
                        // Se o time for novo e não tiver sido mapeado nesta fase, preserva o TimeId
                        // ou, se a regra for que o FK deve ser resolvido, define como null/0.
                        // Para evitar erro de FK, vamos definir explicitamente como null se não for mapeado
                        existingItem.TimeId = null;
                    }
                } else {
                    // Se o DTO enviar TimeClientAppId nulo ou Guid.Empty, remove a ligação
                    existingItem.TimeId = null;
                }
                _context.Entry(existingItem).State = EntityState.Modified;
            }
        }
    }

    // Terceira fase: GetUpdates (correção aqui)
    public async Task<IEnumerable<UsuarioSyncDto>> GetUpdatedSinceAsync(DateTime lastSyncTime) {
        return await _context.Usuarios
            // Incluindo a propriedade de navegação Time para garantir que a FK Guid seja populada corretamente
            // NOTE: Isso pode ser removido se 'u.TimeClientAppId' já for uma coluna no DB e for Guid?
            // Se 'u.TimeClientAppId' for uma coluna no DB:
            .Where(u => u.UpdatedAt > lastSyncTime)
            .Select(u => new UsuarioSyncDto {
                ClientAppId = u.ClientAppId,
                UpdatedAt = u.UpdatedAt,
                Nome = u.Nome,
                Email = u.Email,
                Perfil = u.Perfil,
                ImagemPath = u.ImagemPath,
                Localizacao = u.Localizacao,
                Telefone = u.Telefone,
                LinkRedeSocial = u.LinkRedeSocial,
                DataNascimento = u.DataNascimento,
                Genero = u.Genero,
                NomeEmpresa = u.NomeEmpresa,
                CNPJ = u.CNPJ,
                Peso = u.Peso,
                Altura = u.Altura,
                FaixaOrcamentoPatrocinio = u.FaixaOrcamentoPatrocinio,
                // CORREÇÃO: Força o tratamento do GUID nulo para evitar erros de serialização JSON
                TimeClientAppId = u.TimeClientAppId.HasValue && u.TimeClientAppId.Value != Guid.Empty ? u.TimeClientAppId.Value : (Guid?)null
            })
            .ToListAsync();
    }

    // Outros métodos não alterados
    public async Task<Usuario?> GetByIdAsync(int id) {
        return await _context.Usuarios.FindAsync(id);
    }
    public async Task AddAsync(Usuario usuario) {
        usuario.IsSynced = true;
        usuario.UpdatedAt = DateTime.UtcNow;
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
    }
    public async Task UpdateAsync(Usuario usuario) {
        _context.Entry(usuario).State = EntityState.Modified;
        usuario.IsSynced = true;
        usuario.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}