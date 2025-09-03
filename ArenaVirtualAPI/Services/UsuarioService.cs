using ArenaVirtualAPI.Models;
using ArenaVirtualAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace ArenaVirtualAPI.Services {
    public class UsuarioService : IBackendService<Usuario> {
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

        // **MÉTODO CORRIGIDO**: Mais robusto e eficiente para a sincronização
        public async Task ProcessItemsAsync(IEnumerable<Usuario> items) {
            foreach (var usuario in items) {
                if (usuario.Id == 0) {
                    // Novo usuário. O EF Core irá atribuir o novo Id automaticamente.
                    usuario.UpdatedAt = DateTime.UtcNow;
                    _context.Usuarios.Add(usuario);
                } else {
                    // Atualização de usuário. O EF Core irá encontrar o usuário pelo Id
                    // e atualizar apenas as propriedades que foram alteradas.
                    _context.Entry(usuario).State = EntityState.Modified;
                    usuario.UpdatedAt = DateTime.UtcNow;
                }
            }
            // Salvando todas as alterações de uma vez
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ISyncable>> GetUpdatedSinceAsync(DateTime lastSyncTime) {
            return await _context.Usuarios
              .Where(u => u.UpdatedAt > lastSyncTime)
              .ToListAsync();
        }

        // Métodos específicos da API (não relacionados diretamente à interface de sincronização)
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
}