using ArenaVirtual.Models;
using ArenaVirtual.Services;

namespace ArenaVirtual.ViewModels {
    internal class RegisterViewModel {
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
        public string PerfilSelecionado { get; set; }

        public async Task<bool> RegistrarUsuarioAsync() {
            // Converte a string PerfilSelecionado para o enum TipoPerfil  
            if (!Enum.TryParse(PerfilSelecionado, out TipoPerfil perfilEnum)) {
                throw new ArgumentException($"O perfil '{PerfilSelecionado}' não é válido.");
            }

            var novoUsuario = new Usuario {
                Nome = Nome,
                Email = Email,
                Senha = Senha,
                Perfil = perfilEnum
            };

            // Corrige o tipo de App.Database para DatabaseService  
            if (App.Database is DatabaseService databaseService) {
                await databaseService.InserirUsuarioAsync(novoUsuario);
            } else {
                throw new InvalidOperationException("App.Database não é do tipo DatabaseService.");
            }

            // Retorne true se necessário para indicar sucesso  
            return true;
        }
    }
}
