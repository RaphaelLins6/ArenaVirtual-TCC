using ArenaVirtual.Models;
using BCrypt.Net;
using System.Diagnostics;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using ArenaVirtual.DTOs;

namespace ArenaVirtual.Services {
    public class UsuarioService(
        DatabaseService databaseService,
        SyncService syncService,
        ApiService apiService 
    ) {
        private readonly DatabaseService _databaseService = databaseService;
        private readonly SyncService _syncService = syncService;
        private readonly ApiService _apiService = apiService;

        //public async Task<Usuario?> Autenticar(string email, string senha) {
        //    Debug.WriteLine($"[UsuarioService] Tentando autenticação ONLINE para: {email}");

        //    try {
        //        var loginDto = new { Email = email, Senha = senha };
        //        var response = await _apiService.PostAsync<LoginResponseDto>("api/auth/login", loginDto);

        //        if (response == null) {
        //            Debug.WriteLine("[UsuarioService] Falha na autenticação online: Resposta da API é NULL.");
        //            return null;
        //        }

        //        if (string.IsNullOrEmpty(response.Token)) {
        //            Debug.WriteLine("[UsuarioService] Falha na autenticação online: Token vazio (Credenciais inválidas ou erro de DTO).");
        //            return null;
        //        }

        //        Debug.WriteLine($"[UsuarioService] Autenticação online OK. Token recebido: {response.Token.Substring(0, 10)}...");

        //        SessaoService.Instancia.Token = response.Token;

        //        var usuarioLogado = MapearParaUsuario(response);

        //        if (usuarioLogado == null) {
        //            Debug.WriteLine("[UsuarioService] Erro: Mapeamento do usuário retornado pela API falhou.");
        //            return null;
        //        }

        //        await _databaseService.UpsertUsuarioAsync(usuarioLogado);
        //        return usuarioLogado;
        //    } catch (Exception ex) {
        //        Debug.WriteLine($"[UsuarioService] Erro catastrófico na autenticação online (Exception): {ex.Message}");
        //        return null;
        //    }
        //}

        //private Usuario MapearParaUsuario(LoginResponseDto dto) {
        //    return new Usuario {
        //        Id = dto.Id, 
        //        ClientAppId = dto.ClientAppId,
        //        Email = dto.Email,
        //        Nome = dto.Nome,
        //        SenhaHash = dto.SenhaHash, 
        //        IsSynced = true,
        //        UpdatedAt = DateTime.UtcNow 
        //    };
        //}

        public async Task<Usuario?> AutenticarOffline(string email, string senha) {
            try {
                var usuario = await _databaseService.ObterUsuarioPorEmailAsync(email);

                if (usuario != null && BCrypt.Net.BCrypt.Verify(senha, usuario.SenhaHash)) {
                    //Debug.WriteLine($"[UsuarioService] Autenticação offline bem-sucedida para o usuário: {email}");
                    return usuario;
                } else {
                    //Debug.WriteLine($"[UsuarioService] Falha na autenticação offline para o usuário: {email}");
                    return null;
                }
            } catch (Exception ex) {
                //Debug.WriteLine($"[UsuarioService] Erro ao autenticar offline: {ex.Message}");
                return null;
            }
        }

        public async Task<Usuario?> Cadastrar(Usuario usuario) {
            bool emailExiste = await _databaseService.EmailExisteAsync(usuario.Email);
            if (emailExiste) {
                //Debug.WriteLine("Email já existe.");
                return null;
            }

            usuario.IsSynced = false;
            usuario.UpdatedAt = DateTime.UtcNow;

            int result = await _databaseService.InserirUsuarioAsync(usuario);
            //Debug.WriteLine($"Resultado da inserção: {result}");

            if (result > 0) {
                //Debug.WriteLine("[UsuarioService] Novo usuário salvo localmente. Disparando sincronização...");
                await _syncService.SyncAsync(new Progress<string>());

                var usuarioRetornado = await _databaseService.ObterUsuarioPorClientAppIdAsync(usuario.ClientAppId);

                if (usuarioRetornado != null)
                    //Debug.WriteLine($"Usuário cadastrado e retornado: {usuarioRetornado.Nome}");
                    //else
                    //Debug.WriteLine("Usuário não encontrado após cadastro (problema de sincronização ou ID).");

                    return usuarioRetornado;
            }
            //Debug.WriteLine("Falha ao inserir usuário.");
            return null;
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
            return _databaseService.ObterUsuarioPorClientAppIdAsync(clientAppId);
        }

        public async Task<Dictionary<Guid, string>> ObterNomesUsuariosPorIdsAsync(List<Guid> userIds) {
            if (userIds == null || !userIds.Any()) {
                return new Dictionary<Guid, string>();
            }

            var usuarios = await _databaseService.ObterUsuariosPorIdsAsync(userIds);

            var arbitrosMap = usuarios.ToDictionary(u => u.ClientAppId, u => u.Nome);

            return arbitrosMap;
        }
    }
}