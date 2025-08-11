using ArenaVirtual.Models;

namespace ArenaVirtual.Services {
    public class TimeService(DatabaseService databaseService, UsuarioService usuarioService) {
        private readonly DatabaseService _db = databaseService;
        private readonly UsuarioService _usuarioService = usuarioService;

        public async Task<List<Time>> ObterTodosAsync() =>
            await _db.ListarTimesAsync();

        public async Task<Time?> ObterPorIdAsync(int id) =>
            (await _db.ListarTimesAsync()).FirstOrDefault(t => t.Id == id);

        public async Task<int> CriarTimeEAssociarUsuarioAsync(string nome, string descricao) {
            var usuario = SessaoService.Instancia.GetUsuarioAtual() ?? throw new Exception("Usuário não logado");
            
            var time = new Time {
                Nome = nome,
                Descricao = descricao,
                CapitaoId = usuario.Id
            };

            int resultado = await _db.InserirTimeAsync(time);

            if (resultado > 0) {
                var timeCriado = (await _db.ListarTimesAsync())
                    .FirstOrDefault(t => t.Nome == nome && t.CapitaoId == usuario.Id);

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
    }
}
