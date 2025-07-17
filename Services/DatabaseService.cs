using SQLite;
using ArenaVirtual.Models;
using System.Security.Cryptography;
using System.Text;

namespace ArenaVirtual.Services {
    public class DatabaseService {
        private readonly SQLiteAsyncConnection _database;

        public DatabaseService(string dbPath) {
            _database = new SQLiteAsyncConnection(dbPath);
            // Não chame CreateTableAsync ou InitializeAsync aqui!
        }

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
        }

        public Task<int> InserirUsuarioAsync(Usuario usuario) {
            return _database.InsertAsync(usuario);
        }

        public Task<Usuario> ObterUsuarioPorEmailSenhaAsync(string email, string senha) {
            return _database.Table<Usuario>()
                .Where(u => u.Email == email && u.Senha == senha)
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

        public Task<int> AtualizarUsuarioAsync(Usuario usuario) {
            return _database.UpdateAsync(usuario);
        }

        public Task<int> DeletarUsuarioAsync(Usuario usuario) {
            return _database.DeleteAsync(usuario);
        }

        public Task<int> InserirCampeonatoAsync(Campeonato item) {
            return _database.InsertAsync(item);
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
    }
}