using ArenaVirtual.Models;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace ArenaVirtual.Services {
    public class TimeService(DatabaseService databaseService, UsuarioService usuarioService, SyncService syncService) {
        private readonly DatabaseService _db = databaseService;
        private readonly UsuarioService _usuarioService = usuarioService;
        private readonly SyncService _syncService = syncService;

        public Task<List<Time>> ObterTodosAsync() => _db.ListarTimesAsync();

        // Já existente, usa a chave de sincronização
        public Task<Time?> ObterPorClientAppIdAsync(Guid clientAppId) => _db.GetTimeByClientAppIdAsync(clientAppId);

        // Retorna um time pelo ID local
        public Task<Time?> ObterPorIdAsync(int id) => _db.GetTimeByIdAsync(id);

        public async Task<int> CriarTimeEAssociarUsuarioAsync(Time novoTime) {
            var usuario = SessaoService.Instancia.GetUsuarioAtual() ?? throw new Exception("Usuário não logado");

            novoTime.ClientAppId = Guid.NewGuid();
            novoTime.IsSynced = false;
            novoTime.UpdatedAt = DateTime.UtcNow;

            int resultado = await _db.InserirTimeAsync(novoTime);

            if (resultado > 0) {
                Debug.WriteLine("[TimeService] Novo time salvo localmente. Disparando sincronização...");
                await _syncService.SyncAsync(new Progress<string>());

                usuario.TimeClientAppId = novoTime.ClientAppId;
                usuario.IsSynced = false;
                usuario.UpdatedAt = DateTime.UtcNow;
                await _db.AtualizarUsuarioAsync(usuario);
                SessaoService.Instancia.Login(usuario);

                await _syncService.SyncAsync(new Progress<string>());
            }
            return resultado;
        }

        public async Task<int> AssociarUsuarioAoTimeAsync(Time time) {
            var usuario = SessaoService.Instancia.GetUsuarioAtual() ?? throw new Exception("Usuário não logado");

            usuario.TimeClientAppId = time.ClientAppId;
            usuario.IsSynced = false;
            usuario.UpdatedAt = DateTime.UtcNow;
            int resultado = await _db.AtualizarUsuarioAsync(usuario);
            SessaoService.Instancia.Login(usuario);

            if (resultado > 0) {
                Debug.WriteLine("[TimeService] Usuário associado ao time. Disparando sincronização...");
                await _syncService.SyncAsync(new Progress<string>());
            }
            return resultado;
        }

        public async Task AtualizarTimeAsync(Time time) {
            time.IsSynced = false;
            time.UpdatedAt = DateTime.UtcNow;
            await _db.AtualizarTimeAsync(time);

            Debug.WriteLine("[TimeService] Time atualizado. Disparando sincronização...");
            await _syncService.SyncAsync(new Progress<string>());
        }

        public async Task ExcluirTimeAsync(Guid timeClientAppId) {
            var time = await _db.GetTimeByClientAppIdAsync(timeClientAppId);
            if (time != null) {
                var membros = await _db.GetMembrosByTimeClientAppIdAsync(timeClientAppId);
                foreach (var membro in membros) {
                    membro.TimeClientAppId = null;
                    membro.IsSynced = false;
                    membro.UpdatedAt = DateTime.UtcNow;
                    await _db.AtualizarUsuarioAsync(membro);
                }

                if (!string.IsNullOrEmpty(time.LogoUrl) && File.Exists(time.LogoUrl)) {
                    File.Delete(time.LogoUrl);
                }

                await _db.ExcluirTimeAsync(time);

                await _syncService.SyncAsync(new Progress<string>());
            }
        }
    }
}