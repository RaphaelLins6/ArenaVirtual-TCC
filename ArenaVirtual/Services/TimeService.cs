using ArenaVirtual.Models;
using ArenaVirtual.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics; // Adicionar para Debug.WriteLine

namespace ArenaVirtual.Services {
    // Adicione o SyncService ao construtor
    public class TimeService(DatabaseService databaseService, UsuarioService usuarioService, SyncService syncService) {
        private readonly DatabaseService _db = databaseService;
        private readonly UsuarioService _usuarioService = usuarioService;
        private readonly SyncService _syncService = syncService; // Atribua a dependência

        public async Task<List<Time>> ObterTodosAsync() =>
      await _db.ListarTimesAsync();

        public async Task<Time?> ObterPorIdAsync(int id) {
            var todos = await _db.ListarTimesAsync();
            var t = todos.FirstOrDefault(x => x.Id == id);
            return t;
        }

        public async Task<int> CriarTimeEAssociarUsuarioAsync(Time novoTime) {
            var usuario = SessaoService.Instancia.GetUsuarioAtual() ?? throw new Exception("Usuário não logado");

            var time = new Time {
                Nome = novoTime.Nome,
                Descricao = novoTime.Descricao,
                LogoUrl = novoTime.LogoUrl,
                CapitaoId = usuario.Id
            };

            // Adicionando a lógica de sincronização para o novo time
            time.IsSynced = false;
            time.UpdatedAt = DateTime.UtcNow;

            int resultado = await _db.InserirTimeAsync(time);

            if (resultado > 0) {
                Debug.WriteLine("[TimeService] Novo time salvo localmente.");
                // ** DISPARAR SINCRONIZAÇÃO PARA O NOVO TIME **
                await _syncService.SyncAsync(new Progress<string>());

                var timeCriado = (await _db.ListarTimesAsync())
                  .FirstOrDefault(t => t.Nome == novoTime.Nome && t.CapitaoId == usuario.Id);

                if (timeCriado != null) {
                    usuario.TimeId = timeCriado.Id;
                    // Lógica de sincronização para o usuário atualizado
                    usuario.IsSynced = false;
                    usuario.UpdatedAt = DateTime.UtcNow;
                    await _db.AtualizarUsuarioAsync(usuario);
                    SessaoService.Instancia.Login(usuario);
                    // ** DISPARAR SINCRONIZAÇÃO PARA O USUÁRIO ATUALIZADO **
                    await _syncService.SyncAsync(new Progress<string>());
                }
            }
            return resultado;
        }

        public async Task<int> AssociarUsuarioAoTimeAsync(Time time) {
            var usuario = SessaoService.Instancia.GetUsuarioAtual() ?? throw new Exception("Usuário não logado");
            usuario.TimeId = time.Id;

            // Lógica de sincronização para o usuário atualizado
            usuario.IsSynced = false;
            usuario.UpdatedAt = DateTime.UtcNow;
            int resultado = await _db.AtualizarUsuarioAsync(usuario);
            SessaoService.Instancia.Login(usuario);

            // ** DISPARAR SINCRONIZAÇÃO PARA O USUÁRIO ATUALIZADO **
            if (resultado > 0) {
                Debug.WriteLine("[TimeService] Usuário associado ao time. Disparando sincronização...");
                await _syncService.SyncAsync(new Progress<string>());
            }
            return resultado;
        }

        public async Task AtualizarTimeAsync(Time time) {
            // Lógica de sincronização para o time atualizado
            time.IsSynced = false;
            time.UpdatedAt = DateTime.UtcNow;
            await _db.AtualizarTimeAsync(time);

            // ** DISPARAR SINCRONIZAÇÃO PARA O TIME ATUALIZADO **
            Debug.WriteLine("[TimeService] Time atualizado. Disparando sincronização...");
            await _syncService.SyncAsync(new Progress<string>());
        }

        public async Task ExcluirTimeAsync(int timeId) {
            var time = await _db.GetTimeByIdAsync(timeId);
            if (time != null) {
                await _db.ExcluirTimeAsync(time);

                var membros = await _db.GetMembrosByTimeIdAsync(timeId);
                foreach (var membro in membros) {
                    membro.TimeId = null;
                    // Lógica de sincronização para o membro atualizado
                    membro.IsSynced = false;
                    membro.UpdatedAt = DateTime.UtcNow;
                    await _db.AtualizarUsuarioAsync(membro);
                    // ** DISPARAR SINCRONIZAÇÃO PARA O MEMBRO ATUALIZADO **
                    await _syncService.SyncAsync(new Progress<string>());
                }
            }
        }
    }
}