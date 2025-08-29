using ArenaVirtualAPI.Models;
using ArenaVirtualAPI.Data; // Certifique-se de referenciar seu DbContext
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System; // Necessário para DateTime

namespace ArenaVirtualAPI.Services {
    // A classe deve implementar a interface IBackendService<Usuario> para ser compatível
    // com o ProcessItemsAsync do BackendSyncService
    public class UsuarioService : IBackendService<Usuario> {
        private readonly AppDbContext _context;

        public UsuarioService(AppDbContext context) {
            _context = context;
        }

        public async Task<Usuario?> GetByIdAsync(int id) {
            return await _context.Usuarios.FindAsync(id);
        }

        public async Task AddAsync(Usuario usuario) {
            // No backend, um item recém-adicionado é considerado sincronizado
            usuario.IsSynced = true;
            usuario.UpdatedAt = DateTime.UtcNow; // Garante que o timestamp é definido no backend
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Usuario usuario) {
            var existingUsuario = await _context.Usuarios.FindAsync(usuario.Id);
            if (existingUsuario != null) {
                // Atualiza as propriedades do usuário existente com os dados recebidos.
                // Adapte esta lista de propriedades conforme o que deve ser atualizado.
                existingUsuario.Nome = usuario.Nome;
                existingUsuario.Email = usuario.Email;
                existingUsuario.SenhaHash = usuario.SenhaHash;
                existingUsuario.Perfil = usuario.Perfil;
                existingUsuario.ImagemPath = usuario.ImagemPath;
                existingUsuario.Localizacao = usuario.Localizacao;
                existingUsuario.Telefone = usuario.Telefone;
                existingUsuario.LinkRedeSocial = usuario.LinkRedeSocial;
                existingUsuario.DataNascimento = usuario.DataNascimento;
                existingUsuario.Genero = usuario.Genero;
                existingUsuario.NomeEmpresa = usuario.NomeEmpresa;
                existingUsuario.CNPJ = usuario.CNPJ;
                existingUsuario.Peso = usuario.Peso;
                existingUsuario.Altura = usuario.Altura;
                existingUsuario.FaixaOrcamentoPatrocinio = usuario.FaixaOrcamentoPatrocinio;
                existingUsuario.TimeId = usuario.TimeId;

                // Propriedades de sincronização gerenciadas pelo BackendSyncService
                existingUsuario.IsSynced = true;
                existingUsuario.UpdatedAt = DateTime.UtcNow; // Atualiza o timestamp de modificação no backend

                _context.Usuarios.Update(existingUsuario);
                await _context.SaveChangesAsync();
            }
        }

        // CORREÇÃO: Altere o tipo de retorno para Task<IEnumerable<ISyncable>>
        public async Task<IEnumerable<ISyncable>> GetUpdatedSinceAsync(DateTime lastSyncTime) {
            // Retorna todos os usuários que foram atualizados (ou criados) desde a última sincronização
            // A conversão de IEnumerable<Usuario> para IEnumerable<ISyncable> é automática
            return await _context.Usuarios
                                 .Where(u => u.UpdatedAt > lastSyncTime)
                                 .ToListAsync();
        }

        // Adicione a implementação do método ProcessItemsAsync da interface
        public async Task ProcessItemsAsync(IEnumerable<Usuario> items) {
            foreach (var usuario in items) {
                var existingUsuario = await _context.Usuarios.FindAsync(usuario.Id);
                if (existingUsuario == null) {
                    await AddAsync(usuario);
                } else {
                    await UpdateAsync(usuario);
                }
            }
        }

        // Métodos específicos da API (se necessário, por exemplo para autenticação direta na API)
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