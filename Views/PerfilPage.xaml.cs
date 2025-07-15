using ArenaVirtual.Models;

namespace ArenaVirtual.Views {
    public partial class PerfilPage : ContentPage {

        private Usuario _usuarioAtual;

        public PerfilPage(Usuario usuario) {
            InitializeComponent(); 
            _usuarioAtual = usuario; 
            ConfigurarUIComBaseNoPerfil();
        }

        private void ConfigurarUIComBaseNoPerfil() {
            if (WelcomeMessageLabel != null) { 
                WelcomeMessageLabel.Text = $"Bem-vindo, {_usuarioAtual?.Nome ?? "Usuário"}! ({_usuarioAtual?.Email ?? "N/A"})";
            }

            if (AdminOnlyButton != null) {
                if (_usuarioAtual?.Perfil == TipoPerfil.Organizador) { // Supondo que "Organizador" seja um valor do seu enum TipoPerfil
                    AdminOnlyButton.Text = "Gerenciar Campeonatos";
                    AdminOnlyButton.IsVisible = true;
                } else {
                    AdminOnlyButton.IsVisible = false;
                }
            }

            // Exemplo para outros perfis, se existirem no seu TipoPerfil
            // else if (_usuarioAtual?.Perfil == TipoPerfil.Arbitro) {
            //     AdminOnlyButton.Text = "Ver Escala de Jogos";
            //     AdminOnlyButton.IsVisible = true;
            // }

            Title = $"Perfil de {_usuarioAtual?.Nome ?? "Usuário"} - {_usuarioAtual?.Perfil.ToString() ?? "N/A"}";
        }
    }
}