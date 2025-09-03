using ArenaVirtualAPI.Data;
using ArenaVirtualAPI.Services;
using Microsoft.EntityFrameworkCore;
using ArenaVirtualAPI.Models;
using ArenaVirtualAPI.DTOs;

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
        foreach (var dto in items) {
            var existingItem = await _context.Usuarios.FindAsync(dto.Id);

            if (existingItem == null) {
                // Adiciona novo item se não existir no banco de dados
                var newItem = new Usuario {
                    Nome = dto.Nome,
                    Email = dto.Email,
                    Perfil = dto.Perfil,
                    ImagemPath = dto.ImagemPath,
                    Localizacao = dto.Localizacao,
                    Telefone = dto.Telefone,
                    LinkRedeSocial = dto.LinkRedeSocial,
                    DataNascimento = dto.DataNascimento,
                    Genero = dto.Genero,
                    NomeEmpresa = dto.NomeEmpresa,
                    CNPJ = dto.CNPJ,
                    Peso = dto.Peso,
                    Altura = dto.Altura,
                    FaixaOrcamentoPatrocinio = dto.FaixaOrcamentoPatrocinio,
                    TimeId = dto.TimeId,
                    UpdatedAt = DateTime.UtcNow,
                    IsSynced = true
                };
                _context.Usuarios.Add(newItem);
            } else {
                // Verificação crucial: só atualiza se a versão do DTO for mais recente.
                if (dto.UpdatedAt > existingItem.UpdatedAt) {
                    existingItem.Nome = dto.Nome;
                    existingItem.Email = dto.Email;
                    existingItem.Perfil = dto.Perfil;
                    existingItem.ImagemPath = dto.ImagemPath;
                    existingItem.Localizacao = dto.Localizacao;
                    existingItem.Telefone = dto.Telefone;
                    existingItem.LinkRedeSocial = dto.LinkRedeSocial;
                    existingItem.DataNascimento = dto.DataNascimento;
                    existingItem.Genero = dto.Genero;
                    existingItem.NomeEmpresa = dto.NomeEmpresa;
                    existingItem.CNPJ = dto.CNPJ;
                    existingItem.Peso = dto.Peso;
                    existingItem.Altura = dto.Altura;
                    existingItem.FaixaOrcamentoPatrocinio = dto.FaixaOrcamentoPatrocinio;
                    existingItem.TimeId = dto.TimeId;
                    existingItem.UpdatedAt = DateTime.UtcNow;
                    existingItem.IsSynced = true;
                    _context.Entry(existingItem).State = EntityState.Modified;
                }
            }
        }
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Usuario>> GetUpdatedSinceAsync(DateTime lastSyncTime) {
        return await _context.Usuarios
            .Where(u => u.UpdatedAt > lastSyncTime)
            .ToListAsync();
    }

    public async Task<Usuario?> ObterUsuarioPorEmailAsync(string email) {
        return await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> EmailExisteAsync(string email) {
        return await _context.Usuarios.AnyAsync(u => u.Email == email);
    }

    public async Task<List<Usuario>> GetMembrosByTimeIdAsync(int timeId) {
        return await _context.Usuarios.Where(u => u.TimeId == timeId).ToListAsync();
    }
}
