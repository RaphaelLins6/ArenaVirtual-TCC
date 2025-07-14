using Microsoft.Maui.Controls;
using ArenaVirtual.Models;
using ArenaVirtual.ViewModels;


namespace ArenaVirtual.Views {
    public partial class RegisterPage : ContentPage {
        public RegisterPage() {
            InitializeComponent();
        }
        private async void OnRegistrarClicked(object sender, EventArgs e) {
            // Simulação de criação de usuário
            var usuario = new Usuario {
                Nome = "Maria",
                Email = "maria@email.com",
                Perfil = "Atleta" 
            };

            Application.Current.MainPage = new AppShell(usuario);
        }
    }
}
