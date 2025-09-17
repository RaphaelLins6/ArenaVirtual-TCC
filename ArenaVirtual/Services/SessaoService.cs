using ArenaVirtual.Models;
namespace ArenaVirtual.Services {
    

public class SessaoService {
        private static SessaoService? _instancia; 

        public SessaoService() { }

        private Usuario? _usuarioAtual; 

        public static SessaoService Instancia => _instancia ??= new SessaoService();

        public void Login(Usuario usuario) {
            _usuarioAtual = usuario;
        }

        public Usuario? GetUsuarioAtual() { 
            return _usuarioAtual;
        }

        public void Logout() {
            _usuarioAtual = null;
        }

        public bool EstaLogado => _usuarioAtual != null;
    }
}