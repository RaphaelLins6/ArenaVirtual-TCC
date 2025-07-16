using ArenaVirtual.Models;

namespace ArenaVirtual.Services {
    public static class UsuarioService {
        private static DatabaseService _databaseService => App.Database;

        public static async Task<Usuario?> Cadastrar(Usuario usuario) {
            bool emailExiste = await _databaseService.EmailExisteAsync(usuario.Email);
            if (emailExiste) {
                System.Diagnostics.Debug.WriteLine("Email já existe.");
                return null;
            }

            usuario.Senha = DatabaseService.GerarHash(usuario.Senha);

            int result = await _databaseService.InserirUsuarioAsync(usuario);
            System.Diagnostics.Debug.WriteLine($"Resultado da inserção: {result}");

            if (result > 0) {
                var usuarioRetornado = await _databaseService.ObterUsuarioPorEmailSenhaAsync(usuario.Email, usuario.Senha);
                if (usuarioRetornado != null)
                    System.Diagnostics.Debug.WriteLine($"Usuário cadastrado: {usuarioRetornado.Nome}");
                else
                    System.Diagnostics.Debug.WriteLine("Usuário não encontrado após cadastro.");
                return usuarioRetornado;
            }
            System.Diagnostics.Debug.WriteLine("Falha ao inserir usuário.");
            return null;
        }

        public static async Task<Usuario?> Autenticar(string email, string senha) {
            string senhaHash = DatabaseService.GerarHash(senha);

            Usuario? usuario = await _databaseService.ObterUsuarioPorEmailSenhaAsync(email, senhaHash);
            return usuario;
        }
    }
}