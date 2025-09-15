using ArenaVirtual.Models;
using BCrypt.Net;
using System.Diagnostics;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using SQLite;

namespace ArenaVirtual.Services {
    public class UsuarioService(DatabaseService databaseService, SyncService syncService) {
        private readonly DatabaseService _databaseService = databaseService;
        private readonly SyncService _syncService = syncService;

        public async Task<Usuario?> Cadastrar(Usuario usuario) {
            bool emailExiste = await _databaseService.EmailExisteAsync(usuario.Email);
            if (emailExiste) {
                Debug.WriteLine("Email já existe.");
                return null;
            }

            usuario.IsSynced = false;
            usuario.UpdatedAt = DateTime.UtcNow;

            int result = await _databaseService.InserirUsuarioAsync(usuario);
            Debug.WriteLine($"Resultado da inserção: {result}");

            if (result > 0) {
                Debug.WriteLine("[UsuarioService] Novo usuário salvo localmente. Disparando sincronização...");
                await _syncService.SyncAsync(new Progress<string>());

                // Agora usa o método corrigido para buscar o usuário pelo ClientAppId
                var usuarioRetornado = await _databaseService.ObterUsuarioPorClientAppIdAsync(usuario.ClientAppId);

                if (usuarioRetornado != null)
                    Debug.WriteLine($"Usuário cadastrado e retornado: {usuarioRetornado.Nome}");
                else
                    Debug.WriteLine("Usuário não encontrado após cadastro (problema de sincronização ou ID).");

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

        // Método adicionado para corrigir o erro CS1061
        public async Task<Usuario?> GetUsuarioByEmailAsync(string email) {
            return await _databaseService.ObterUsuarioPorEmailAsync(email);
        }

        public static string GerarHash(string senha) {
            return BCrypt.Net.BCrypt.HashPassword(senha, workFactor: 12);
        }

        public async Task<List<Usuario>> ListarMembrosDoTimeAsync(Guid timeClientAppId) {
            var todos = await _databaseService.ListarUsuariosAsync();
            var membros = todos.Where(u => u.TimeClientAppId == timeClientAppId).ToList();
            return membros;
        }

        public async Task<List<Usuario>> GetMembrosByTimeClientAppIdAsync(Guid timeClientAppId) {
            return await _databaseService.GetUsuarioTable().Where(u => u.TimeClientAppId == timeClientAppId).ToListAsync();
        }

        public async Task<Usuario> ObterUsuarioPorIdAsync(int usuarioId) {
            return await _databaseService.GetUsuarioTable().FirstOrDefaultAsync(u => u.Id == usuarioId);
        }
    }
}