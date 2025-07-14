using ArenaVirtual.Models;
using ArenaVirtual.ViewModels;

namespace ArenaVirtual.Views {
    public partial class LoginPage : ContentPage {
        public LoginPage() {
            InitializeComponent();
            Title = "Login";
            BindingContext = new LoginViewModel();
        }
        private async void OnEntrarClicked(object sender, EventArgs e) {
            // Aqui você normalmente faria uma chamada ao backend para autenticação
            // Exemplo simples:
            var usuario = new Usuario {
                Nome = "João",
                Email = "joao@email.com",
                Perfil = "Organizador" // Pode ser: "Atleta", "Arbitro", "Patrocinador"
            };

            Application.Current.MainPage = new AppShell(usuario);
        }
    }
}