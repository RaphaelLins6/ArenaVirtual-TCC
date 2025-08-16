using ArenaVirtual.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArenaVirtual.Services {
    public class TimeService {
        private readonly DatabaseService _db;
        private readonly UsuarioService _usuarioService;

        public TimeService(DatabaseService databaseService, UsuarioService usuarioService) {
            _db = databaseService;
            _usuarioService = usuarioService;
        }

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

            int resultado = await _db.InserirTimeAsync(time);

            if (resultado > 0) {
                var timeCriado = (await _db.ListarTimesAsync())
                    .FirstOrDefault(t => t.Nome == novoTime.Nome && t.CapitaoId == usuario.Id);

                if (timeCriado != null) {
                    usuario.TimeId = timeCriado.Id;
                    await _db.AtualizarUsuarioAsync(usuario);

                    SessaoService.Instancia.Login(usuario);
                }
            }
            return resultado;
        }

        public async Task<int> AssociarUsuarioAoTimeAsync(Time time) {
            var usuario = SessaoService.Instancia.GetUsuarioAtual() ?? throw new Exception("Usuário não logado");
            usuario.TimeId = time.Id;
            int resultado = await _db.AtualizarUsuarioAsync(usuario);
            SessaoService.Instancia.Login(usuario);
            return resultado;
        }

        public async Task AtualizarTimeAsync(Time time) {
            await _db.AtualizarTimeAsync(time);
        }

        public async Task ExcluirTimeAsync(int timeId) {
            // Correção: Use _db em vez de _databaseService
            var time = await _db.GetTimeByIdAsync(timeId);
            if (time != null) {
                await _db.ExcluirTimeAsync(time);

                var membros = await _db.GetMembrosByTimeIdAsync(timeId);
                foreach (var membro in membros) {
                    membro.TimeId = null;
                    // Correção: Use _db em vez de _databaseService
                    await _db.AtualizarUsuarioAsync(membro);
                }
            }
        }
    }
}