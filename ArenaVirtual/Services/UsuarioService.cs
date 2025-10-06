using ArenaVirtual.Models;
using BCrypt.Net;
using System.Diagnostics;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using SQLite;

namespace ArenaVirtual.Services {
    // Usando a sintaxe de construtor primário do C# 12 para UsuarioService
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

        public async Task<Usuario?> AutenticarOffline(string email, string senha) {
            try {
                var usuario = await _databaseService.ObterUsuarioPorEmailAsync(email);

                if (usuario != null && BCrypt.Net.BCrypt.Verify(senha, usuario.SenhaHash)) {
                    Debug.WriteLine($"[UsuarioService] Autenticação offline bem-sucedida para o usuário: {email}");
                    return usuario;
                } else {
                    Debug.WriteLine($"[UsuarioService] Falha na autenticação offline para o usuário: {email}");
                    return null;
                }
            } catch (Exception ex) {
                Debug.WriteLine($"[UsuarioService] Erro ao autenticar offline: {ex.Message}");
                return null;
            }
        }

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

        public Task<Usuario?> ObterUsuarioPorClientAppIdAsync(Guid clientAppId) {
            // Repassa a chamada para o DatabaseService
            return _databaseService.ObterUsuarioPorClientAppIdAsync(clientAppId);
        }

        // --- NOVO MÉTODO ADICIONADO ---
        /// <summary>
        /// Obtém um dicionário de mapeamento ClientAppId do Usuário -> Nome do Usuário para uma lista de IDs.
        /// Este método é chamado a partir da ViewModel.
        /// </summary>
        /// <param name="userIds">Lista de ClientAppIds (Guid) dos usuários a serem buscados.</param>
        /// <returns>Dicionário onde a chave é o ClientAppId (Guid) e o valor é o Nome (string).</returns>
        public async Task<Dictionary<Guid, string>> ObterNomesUsuariosPorIdsAsync(List<Guid> userIds) {
            if (userIds == null || !userIds.Any()) {
                return new Dictionary<Guid, string>();
            }

            // Chama o método de busca em lote no DatabaseService
            var usuarios = await _databaseService.ObterUsuariosPorIdsAsync(userIds);

            // Converte a lista de usuários para um Dicionário (ClientAppId -> Nome)
            var arbitrosMap = usuarios.ToDictionary(u => u.ClientAppId, u => u.Nome);

            return arbitrosMap;
        }
    }
}