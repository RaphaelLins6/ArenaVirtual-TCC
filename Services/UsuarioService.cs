using ArenaVirtual.Models;

namespace ArenaVirtual.Services {
    public static class UsuarioService {
        private static DatabaseService _databaseService => App.Database;

        public static async Task<Usuario?> Cadastrar(Usuario usuario) {
            bool emailExiste = await _databaseService.EmailExisteAsync(usuario.Email);
            if (emailExiste) {
                return null;
            }

            usuario.Senha = DatabaseService.GerarHash(usuario.Senha);

            int result = await _databaseService.InserirUsuarioAsync(usuario);

            if (result > 0) {
                return await _databaseService.ObterUsuarioPorEmailSenhaAsync(usuario.Email, usuario.Senha);
            }
            return null;
        }

        public static async Task<Usuario?> Autenticar(string email, string senha) {
            string senhaHash = DatabaseService.GerarHash(senha);

            Usuario? usuario = await _databaseService.ObterUsuarioPorEmailSenhaAsync(email, senhaHash);
            return usuario;
        }
    }
}