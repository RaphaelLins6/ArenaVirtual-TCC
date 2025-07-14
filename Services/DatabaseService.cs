using SQLite;
using ArenaVirtual.Models;

namespace ArenaVirtual.Services {
    public class DatabaseService {
        private readonly SQLiteAsyncConnection _database;

        public DatabaseService(string dbPath) {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<Usuario>().Wait();
        }

        // Inserir usuário
        public Task<int> InserirUsuarioAsync(Usuario usuario) {
            return _database.InsertAsync(usuario);
        }

        // Verificar login
        public Task<Usuario> ObterUsuarioPorEmailSenhaAsync(string email, string senha) {
            return _database.Table<Usuario>()
                .Where(u => u.Email == email && u.Senha == senha)
                .FirstOrDefaultAsync();
        }

        // Listar todos
        public Task<List<Usuario>> ListarUsuariosAsync() {
            return _database.Table<Usuario>().ToListAsync();
        }
    }
}