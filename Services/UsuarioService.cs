using ArenaVirtual.Models;
using BCrypt.Net; 

namespace ArenaVirtual.Services {
    public class UsuarioService(DatabaseService databaseService) {
        private readonly DatabaseService _databaseService = databaseService;

        public async Task<Usuario?> Cadastrar(Usuario usuario) {
            bool emailExiste = await _databaseService.EmailExisteAsync(usuario.Email);
            if (emailExiste) {
                System.Diagnostics.Debug.WriteLine("Email já existe.");
                return null;
            }

            int result = await _databaseService.InserirUsuarioAsync(usuario);
            System.Diagnostics.Debug.WriteLine($"Resultado da inserção: {result}");

            if (result > 0) {
                var usuarioRetornado = await _databaseService.ObterUsuarioPorEmailAsync(usuario.Email);
                if (usuarioRetornado != null)
                    System.Diagnostics.Debug.WriteLine($"Usuário cadastrado e retornado: {usuarioRetornado.Nome}");
                else
                    System.Diagnostics.Debug.WriteLine("Usuário não encontrado após cadastro (possível problema de ID).");
                return usuarioRetornado;
            }
            System.Diagnostics.Debug.WriteLine("Falha ao inserir usuário.");
            return null;
        }

        public async Task<Usuario?> Autenticar(string email, string senha) {
            Usuario? usuario = await _databaseService.ObterUsuarioPorEmailAsync(email);

            if (usuario == null) {
                return null;
            }

            if (BCrypt.Net.BCrypt.Verify(senha, usuario.SenhaHash)) {
                return usuario;
            } else {
                return null;
            }
        }
        public static string GerarHash(string senha) {
            return BCrypt.Net.BCrypt.HashPassword(senha, workFactor: 12);
        }

        public async Task<List<Usuario>> ListarMembrosDoTimeAsync(int timeId) {
            var todosUsuarios = await _databaseService.ListarUsuariosAsync();
            return todosUsuarios.Where(u => u.TimeId == timeId).ToList();
        }
    }
}