using ArenaVirtual.Models;
namespace ArenaVirtual.Services {
    

public class SessaoService {
        private static SessaoService? _instancia; // Declare as nullable

        private SessaoService() { }

        private Usuario? _usuarioAtual; // Declare as nullable

        public static SessaoService Instancia => _instancia ??= new SessaoService();

        public void Login(Usuario usuario) {
            _usuarioAtual = usuario;
        }

        public Usuario? GetUsuarioAtual() { // Return nullable type
            return _usuarioAtual;
        }

        public void Logout() {
            _usuarioAtual = null;
        }

        public bool EstaLogado => _usuarioAtual != null;
    }
}