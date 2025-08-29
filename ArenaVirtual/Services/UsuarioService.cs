using ArenaVirtual.Models;
using BCrypt.Net;
using System.Diagnostics;
using System;
using System.Threading.Tasks;

namespace ArenaVirtual.Services {
    public class UsuarioService(DatabaseService databaseService, SyncService syncService) {
        private readonly DatabaseService _databaseService = databaseService;
        private readonly SyncService _syncService = syncService; // Atribua a dependência

        public async Task<Usuario?> Cadastrar(Usuario usuario) {
            bool emailExiste = await _databaseService.EmailExisteAsync(usuario.Email);
            if (emailExiste) {
                Debug.WriteLine("Email já existe.");
                return null;
            }

            // A lógica de preenchimento está correta
            usuario.IsSynced = false;
            usuario.UpdatedAt = DateTime.UtcNow;

            int result = await _databaseService.InserirUsuarioAsync(usuario);
            Debug.WriteLine($"Resultado da inserção: {result}");

            if (result > 0) {
                // ** DISPARO MANUAL DA SINCRONIZAÇÃO APÓS INSERÇÃO BEM-SUCEDIDA **
                Debug.WriteLine("[UsuarioService] Novo usuário salvo localmente. Disparando sincronização...");

                // Crie e passe um objeto de progresso vazio para o método SyncAsync
                await _syncService.SyncAsync(new Progress<string>());

                var usuarioRetornado = await _databaseService.ObterUsuarioPorEmailAsync(usuario.Email);
                if (usuarioRetornado != null)
                    Debug.WriteLine($"Usuário cadastrado e retornado: {usuarioRetornado.Nome}");
                else
                    Debug.WriteLine("Usuário não encontrado após cadastro (possível problema de ID).");
                return usuarioRetornado;
            }
            Debug.WriteLine("Falha ao inserir usuário.");
            return null;
        }

        public async Task<Usuario?> Autenticar(string email, string senha) {
            Usuario? usuario = await _databaseService.ObterUsuarioPorEmailAsync(email);

            if (usuario == null) {
                return null;
            }

            if (BCrypt.Net.BCrypt.Verify(senha, usuario.SenhaHash)) {
                return usuario;
            } else {
                return null;
            }
        }

        public static string GerarHash(string senha) {
            return BCrypt.Net.BCrypt.HashPassword(senha, workFactor: 12);
        }

        public async Task<List<Usuario>> ListarMembrosDoTimeAsync(int timeId) {
            var todos = await _databaseService.ListarUsuariosAsync();
            var membros = todos.Where(u => u.TimeId == timeId).ToList();

            return membros;
        }

        public async Task<List<Usuario>> GetMembrosByTimeIdAsync(int timeId) {
            return await _databaseService.GetUsuarioTable().Where(u => u.TimeId == timeId).ToListAsync();
        }

        public async Task<Usuario> ObterUsuarioPorIdAsync(int usuarioId) {
            return await _databaseService.GetUsuarioTable().FirstOrDefaultAsync(u => u.Id == usuarioId);
        }
    }
}