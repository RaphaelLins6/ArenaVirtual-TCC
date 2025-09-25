using ArenaVirtual.Models;
using System.Diagnostics;

namespace ArenaVirtual.Services {
    public class SessaoService {
        private static SessaoService? _instancia;
        private Usuario? _usuarioAtual;

        // Construtor privado para evitar a criação de novas instâncias
        private SessaoService() {
            Debug.WriteLine("[SessaoService] Construtor privado chamado.");
        }

        public static SessaoService Instancia {
            get {
                if (_instancia == null) {
                    _instancia = new SessaoService();
                    Debug.WriteLine("[SessaoService] Nova instância criada.");
                }
                return _instancia;
            }
        }

        public void Login(Usuario usuario) {
            _usuarioAtual = usuario;
            Debug.WriteLine($"[SessaoService] Login chamado. Usuário logado: {usuario?.Email ?? "NULL"}, ClientAppId: {usuario?.ClientAppId}");
        }

        public Usuario? GetUsuarioAtual() {
            Debug.WriteLine($"[SessaoService] GetUsuarioAtual chamado. Usuário atual: {(_usuarioAtual == null ? "NULL" : _usuarioAtual.Email)}");
            return _usuarioAtual;
        }

        public void Logout() {
            Debug.WriteLine($"[SessaoService] Logout chamado. Usuário anterior: {(_usuarioAtual == null ? "NULL" : _usuarioAtual.Email)}");
            _usuarioAtual = null;
        }

        public bool EstaLogado {
            get {
                Debug.WriteLine($"[SessaoService] EstaLogado verificado: {_usuarioAtual != null}");
                return _usuarioAtual != null;
            }
        }

        public void SetUsuarioAtual(Usuario usuario) {
            _usuarioAtual = usuario;
        }
    }
}
