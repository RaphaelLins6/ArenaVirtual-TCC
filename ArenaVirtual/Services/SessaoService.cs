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

        public async Task<Time?> GetTimeAtualAsync() {
            Debug.WriteLine($"[SessaoService] GetTimeAtualAsync chamado.");
            if (_usuarioAtual == null) {
                Debug.WriteLine("[SessaoService] Usuário atual é nulo. Retornando null.");
                return null;
            }

            // O DatabaseService é instanciado aqui, mantendo o padrão Singleton/anti-DI
            var databaseService = new DatabaseService(App.DatabasePath);
            if (_usuarioAtual.TimeClientAppId.HasValue) {
                return await databaseService.GetTimeByClientAppIdAsync(_usuarioAtual.TimeClientAppId.Value);
            }
            return null;
        }

        public async Task<Usuario?> GetArbitroAtualAsync() {
            if (_usuarioAtual == null)
                return null;

            // Verifica se o usuário logado é árbitro
            if (_usuarioAtual.Perfil == TipoPerfil.Arbitro)
                return _usuarioAtual;

            return null;
        }

    }
}
