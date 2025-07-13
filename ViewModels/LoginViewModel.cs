using ArenaVirtual.Models;
using MvvmHelpers;
using System.Linq;
using System.Windows.Input;

namespace ArenaVirtual.ViewModels {
    public class LoginViewModel : BaseViewModel {
        private string email = string.Empty; // Initialize with a default value
        public string Email {
            get => email;
            set => SetProperty(ref email, value);
        }

        private string senha = string.Empty; // Initialize with a default value
        public string Senha {
            get => senha;
            set => SetProperty(ref senha, value);
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel() {
            LoginCommand = new Command(async () => {
                var usuario = FakeLogin(Email, Senha);

                if (usuario != null) {
                    var mainWindow = Application.Current?.Windows.FirstOrDefault();
                    if (mainWindow != null && mainWindow.Page != null) {
                        mainWindow.Page = new AppShell(usuario);
                    }
                } else {
                    var currentPage = Application.Current?.Windows.FirstOrDefault()?.Page;
                    if (currentPage != null) {
                        await currentPage.DisplayAlert("Erro", "Login inválido", "OK");
                    }
                }
            });
        }

        private Usuario? FakeLogin(string email, string senha) {
            if (email == "org@teste.com") return new Usuario { Nome = "Organizador", Perfil = "Organizador" };
            if (email == "atleta@teste.com") return new Usuario { Nome = "Atleta", Perfil = "Atleta" };
            if (email == "arbitro@teste.com") return new Usuario { Nome = "Arbitro", Perfil = "Arbitro" };
            if (email == "patro@teste.com") return new Usuario { Nome = "Patrocinador", Perfil = "Patrocinador" };
            return null;
        }
    }
}