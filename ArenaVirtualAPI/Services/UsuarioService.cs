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

    public async Task ProcessItemsAsync(IEnumerable<UsuarioSyncDto> items) {
        // Este método não precisa retornar um dicionário, então ele chama o método mais completo.
        await ProcessAndMapItemsAsync(items);
    }

    // Altera o tipo de retorno para <Guid, int> para resolver o erro CS0738
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
                    // Usa TimeClientAppId no DTO para mapear para a propriedade do modelo
                    TimeClientAppId = dto.TimeClientAppId,
                    UpdatedAt = DateTime.UtcNow,
                    IsSynced = true,
                    SenhaHash = "not_available"
                };
                _context.Usuarios.Add(newItem);
                await _context.SaveChangesAsync();
                idMapping[newItem.ClientAppId] = newItem.Id;
            } else {
                if (dto.UpdatedAt > existingItem.UpdatedAt) {
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
                    // Usa TimeClientAppId do DTO para atualizar a propriedade do modelo
                    existingItem.TimeClientAppId = dto.TimeClientAppId ?? existingItem.TimeClientAppId;
                    existingItem.UpdatedAt = DateTime.UtcNow;
                    existingItem.IsSynced = true;
                    _context.Entry(existingItem).State = EntityState.Modified;
                }
            }
        }
        await _context.SaveChangesAsync();
        return idMapping;
    }

    public async Task<IEnumerable<UsuarioSyncDto>> GetUpdatedSinceAsync(DateTime lastSyncTime) {
        return await _context.Usuarios
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
                TimeClientAppId = u.TimeClientAppId
            })
            .ToListAsync();
    }

    // Métodos adicionais do serviço
    public async Task<Usuario?> ObterUsuarioPorEmailAsync(string email) {
        return await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> EmailExisteAsync(string email) {
        return await _context.Usuarios.AnyAsync(u => u.Email == email);
    }

    // Altera a propriedade para TimeClientAppId
    public async Task<List<Usuario>> GetMembrosByTimeClientAppIdAsync(Guid timeClientAppId) {
        return await _context.Usuarios.Where(u => u.TimeClientAppId == timeClientAppId).ToListAsync();
    }
}