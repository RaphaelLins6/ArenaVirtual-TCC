using ArenaVirtual.Models;

namespace ArenaVirtual.Services {
    public static class UsuarioService {
        private static List<Usuario> usuarios = new();

        public static bool RegistrarUsuario(Usuario usuario) {
            if (usuarios.Any(u => u.Email == usuario.Email)) {
                return false; // Já existe
            }

            usuarios.Add(usuario);
            return true;
        }

        public static Usuario? Autenticar(string email, string senha, out string mensagem) {
            var usuario = usuarios.FirstOrDefault(u => u.Email == email && u.Senha == senha);

            if (usuario != null) {
                mensagem = "Autenticação bem-sucedida.";
                return usuario;
            }

            mensagem = "Usuário ou senha inválidos.";
            return null;
        }
    }
}