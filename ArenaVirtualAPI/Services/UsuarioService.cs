using ArenaVirtualAPI.Models;
using ArenaVirtualAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace ArenaVirtualAPI.Services {
    public class UsuarioService : IBackendService<Usuario> {
        private readonly AppDbContext _context;

        public UsuarioService(AppDbContext context) {
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

        public async Task ProcessItemsAsync(IEnumerable<Usuario> items) {
            foreach (var usuario in items) {
                // Usa o 'AsNoTracking()' para evitar o rastreamento desnecessário de entidades.
                var existingUsuario = await _context.Usuarios
          .AsNoTracking()
          .FirstOrDefaultAsync(u => u.Id == usuario.Id);

                if (existingUsuario == null) {
                    // Se não existe, é um novo usuário.
                    usuario.UpdatedAt = DateTime.UtcNow;
                    _context.Usuarios.Add(usuario);
                } else {
                    // Se já existe, é uma atualização.
                    usuario.UpdatedAt = DateTime.UtcNow;
                    _context.Usuarios.Update(usuario);
                }
            }
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