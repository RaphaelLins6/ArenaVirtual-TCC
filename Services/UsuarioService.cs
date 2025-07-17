using ArenaVirtual.Models;
using System.Security.Cryptography;
using System.Text;

namespace ArenaVirtual.Services {
    public class UsuarioService(DatabaseService databaseService) {
        private readonly DatabaseService _databaseService = databaseService;

        public async Task<Usuario?> Cadastrar(Usuario usuario) {
            bool emailExiste = await _databaseService.EmailExisteAsync(usuario.Email);
            if (emailExiste) {
                System.Diagnostics.Debug.WriteLine("Email já existe.");
                return null;
            }

            usuario.Senha = GerarHash(usuario.Senha);

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

        public async Task<Usuario?> Autenticar(string email, string senha) {
            string senhaHash = GerarHash(senha);
            Usuario? usuario = await _databaseService.ObterUsuarioPorEmailSenhaAsync(email, senhaHash);
            return usuario;
        }

        public static string GerarHash(string senha) {
            var bytes = Encoding.UTF8.GetBytes(senha);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}