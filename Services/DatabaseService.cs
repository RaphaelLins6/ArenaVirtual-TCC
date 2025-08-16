using SQLite;
using ArenaVirtual.Models;

namespace ArenaVirtual.Services {
    public class DatabaseService(string dbPath) {
        private readonly SQLiteAsyncConnection _database = new(dbPath);

        public async Task InitializeAsync() {
            await _database.CreateTableAsync<Usuario>();
            await _database.CreateTableAsync<Campeonato>();
            await _database.CreateTableAsync<Time>();
            await _database.CreateTableAsync<Partida>();
            await _database.CreateTableAsync<AvaliacaoArbitro>();
            await _database.CreateTableAsync<CampanhaPatrocinio>();
            await _database.CreateTableAsync<Estatistica>();
            await _database.CreateTableAsync<Jogo>();
            await _database.CreateTableAsync<PropostaPatrocinio>();
            await _database.CreateTableAsync<UsuarioCampeonatoFavorito>();
            await _database.CreateTableAsync<Convite>();
        }

        public Task<int> InserirUsuarioAsync(Usuario usuario) {
            return _database.InsertAsync(usuario);
        }

        public async Task<Usuario?> ObterUsuarioPorEmailAsync(string email) {
            return await _database.Table<Usuario>()
                .Where(u => u.Email == email)
                .FirstOrDefaultAsync();
        }

        public Task<List<Usuario>> ListarUsuariosAsync() {
            return _database.Table<Usuario>().ToListAsync();
        }

        public async Task<bool> EmailExisteAsync(string email) {
            var usuario = await _database.Table<Usuario>()
                                         .Where(u => u.Email == email)
                                         .FirstOrDefaultAsync();
            return usuario != null;
        }

        public async Task<int> AtualizarUsuarioAsync(Usuario usuario) {
            System.Diagnostics.Debug.WriteLine($"[DatabaseService] Atualizando usuário ID: {usuario.Id}, ImagemPath: {usuario.ImagemPath}");

            var existingUser = await _database.FindAsync<Usuario>(usuario.Id);
            if (existingUser != null) {
                System.Diagnostics.Debug.WriteLine($"[DatabaseService] Usuário existente no DB (ID={existingUser.Id}): ImagemPath={existingUser.ImagemPath}");
            } else {
                System.Diagnostics.Debug.WriteLine($"[DatabaseService] Usuário com ID {usuario.Id} NÃO encontrado no DB para atualização.");
                return 0;
            }

            int rowsAffected = await _database.UpdateAsync(usuario);

            System.Diagnostics.Debug.WriteLine($"[DatabaseService] UpdateAsync retornou: {rowsAffected} linhas afetadas.");

            return rowsAffected;
        }

        public Task<int> DeletarUsuarioAsync(Usuario usuario) {
            return _database.DeleteAsync(usuario);
        }

        public async Task<int> InserirCampeonatoAsync(Campeonato campeonato) {
            var existente = await _database.Table<Campeonato>()
                .Where(c => c.Nome == campeonato.Nome && c.DataInicio == campeonato.DataInicio)
                .FirstOrDefaultAsync();

            if (existente != null)
                return 0;

            return await _database.InsertAsync(campeonato);
        }
        public Task<List<Campeonato>> ListarCampeonatosAsync() {
            return _database.Table<Campeonato>().ToListAsync();
        }
        public Task<int> AtualizarCampeonatoAsync(Campeonato item) {
            return _database.UpdateAsync(item);
        }
        public Task<int> DeletarCampeonatoAsync(Campeonato item) {
            return _database.DeleteAsync(item);
        }

        public Task<int> InserirTimeAsync(Time item) {
            return _database.InsertAsync(item);
        }
        public Task<List<Time>> ListarTimesAsync() {
            return _database.Table<Time>().ToListAsync();
        }
        public Task<int> AtualizarTimeAsync(Time item) {
            return _database.UpdateAsync(item);
        }
        public Task<int> DeletarTimeAsync(Time item) {
            return _database.DeleteAsync(item);
        }

        public Task<int> InserirPartidaAsync(Partida item) {
            return _database.InsertAsync(item);
        }
        public Task<List<Partida>> ListarPartidasAsync() {
            return _database.Table<Partida>().ToListAsync();
        }
        public Task<int> AtualizarPartidaAsync(Partida item) {
            return _database.UpdateAsync(item);
        }
        public Task<int> DeletarPartidaAsync(Partida item) {
            return _database.DeleteAsync(item);
        }

        public Task<int> InserirAvaliacaoArbitroAsync(AvaliacaoArbitro item) {
            return _database.InsertAsync(item);
        }
        public Task<List<AvaliacaoArbitro>> ListarAvaliacoesArbitroAsync() {
            return _database.Table<AvaliacaoArbitro>().ToListAsync();
        }
        public Task<int> AtualizarAvaliacaoArbitroAsync(AvaliacaoArbitro item) {
            return _database.UpdateAsync(item);
        }
        public Task<int> DeletarAvaliacaoArbitroAsync(AvaliacaoArbitro item) {
            return _database.DeleteAsync(item);
        }

        public Task<int> InserirCampanhaPatrocinioAsync(CampanhaPatrocinio item) {
            return _database.InsertAsync(item);
        }
        public Task<List<CampanhaPatrocinio>> ListarCampanhasPatrocinioAsync() {
            return _database.Table<CampanhaPatrocinio>().ToListAsync();
        }
        public Task<int> AtualizarCampanhaPatrocinioAsync(CampanhaPatrocinio item) {
            return _database.UpdateAsync(item);
        }
        public Task<int> DeletarCampanhaPatrocinioAsync(CampanhaPatrocinio item) {
            return _database.DeleteAsync(item);
        }

        public Task<int> InserirEstatisticaAsync(Estatistica item) {
            return _database.InsertAsync(item);
        }
        public Task<List<Estatistica>> ListarEstatisticasAsync() {
            return _database.Table<Estatistica>().ToListAsync();
        }
        public Task<int> AtualizarEstatisticaAsync(Estatistica item) {
            return _database.UpdateAsync(item);
        }
        public Task<int> DeletarEstatisticaAsync(Estatistica item) {
            return _database.DeleteAsync(item);
        }

        public Task<int> InserirJogoAsync(Jogo item) {
            return _database.InsertAsync(item);
        }
        public Task<List<Jogo>> ListarJogosAsync() {
            return _database.Table<Jogo>().ToListAsync();
        }
        public Task<int> AtualizarJogoAsync(Jogo item) {
            return _database.UpdateAsync(item);
        }
        public Task<int> DeletarJogoAsync(Jogo item) {
            return _database.DeleteAsync(item);
        }

        public Task<int> InserirPropostaPatrocinioAsync(PropostaPatrocinio item) {
            return _database.InsertAsync(item);
        }
        public Task<List<PropostaPatrocinio>> ListarPropostasPatrocinioAsync() {
            return _database.Table<PropostaPatrocinio>().ToListAsync();
        }
        public Task<int> AtualizarPropostaPatrocinioAsync(PropostaPatrocinio item) {
            return _database.UpdateAsync(item);
        }
        public Task<int> DeletarPropostaPatrocinioAsync(PropostaPatrocinio item) {
            return _database.DeleteAsync(item);
        }

        public async Task<int> InserirFavoritoAsync(UsuarioCampeonatoFavorito favorito) {
            return await _database.InsertAsync(favorito);
        }

        public async Task<int> DeletarFavoritoAsync(UsuarioCampeonatoFavorito favorito) {
            return await _database.DeleteAsync(favorito);
        }

        public async Task<List<UsuarioCampeonatoFavorito>> ListarFavoritosPorUsuarioAsync(int usuarioId) {
            return await _database.Table<UsuarioCampeonatoFavorito>()
                                  .Where(f => f.UsuarioId == usuarioId)
                                  .ToListAsync();
        }

        public async Task<int> InserirConviteAsync(Convite convite) {
            return await _database.InsertAsync(convite);
        }

        public async Task<int> AtualizarConviteAsync(Convite convite) {
            return await _database.UpdateAsync(convite);
        }

        public async Task<List<Convite>> ListarConvitesPendentesAsync(int timeId) {
            return await _database.Table<Convite>()
                                  .Where(c => c.IdTime == timeId && c.Status == StatusConvite.Pendente)
                                  .ToListAsync();
        }

        public async Task<Convite?> ObterConvitePorUsuarioETimeAsync(int idSolicitante, int idTime) {
            return await _database.Table<Convite>()
                                  .FirstOrDefaultAsync(c => c.IdSolicitante == idSolicitante && c.IdTime == idTime);
        }

        public AsyncTableQuery<Usuario> GetUsuarioTable() {
            return _database.Table<Usuario>();
        }

        public AsyncTableQuery<Convite> GetConviteTable() {
            return _database.Table<Convite>();
        }
    }   
}