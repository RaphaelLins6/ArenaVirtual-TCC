using ArenaVirtual.Models;
using ArenaVirtual.Services;
using ArenaVirtual.Views;
using ArenaVirtual.Views.Arbitro;
using ArenaVirtual.Views.Atleta;
using ArenaVirtual.Views.Organizador;
using ArenaVirtual.Views.Patrocinador;

namespace ArenaVirtual {
    public partial class AppShell : Shell {
        private Usuario _usuarioLogado;

        public AppShell(Usuario usuarioLogado) {
            InitializeComponent();
            _usuarioLogado = usuarioLogado;

            CriarMenuPorPerfil(usuarioLogado);

            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        }

        private void CriarMenuPorPerfil(Usuario usuario) {
            this.Items.Clear();

            var serviceProvider = Application.Current.Handler.MauiContext.Services;
            var alertService = serviceProvider.GetService<IAlertService>();

            this.Items.Add(new ShellContent {
                Title = "Meu Perfil",
                    ContentTemplate = new DataTemplate(() => {
                    return new PerfilPage(_usuarioLogado, alertService, serviceProvider);
                })
            });

            if (usuario.Perfil == TipoPerfil.Atleta) {
                this.Items.Add(new FlyoutItem {
                    Title = "Meu Time",
                    Items = {
                            new ShellContent { Title = "Informações do Time", ContentTemplate = new DataTemplate(() => new MeusTimesPage()) },
                            new ShellContent { Title = "Jogos", ContentTemplate = new DataTemplate(() => new Views.Atleta.PartidasPage()) }
                        }
                });
            } else if (usuario.Perfil == TipoPerfil.Organizador) {
                this.Items.Add(new FlyoutItem {
                    Title = "Gerenciar Campeonatos",
                    Items = {
                            new ShellContent { Title = "Criar Campeonato", ContentTemplate = new DataTemplate(() => new CriarCampeonatoPage()) },
                            new ShellContent { Title = "Ver Campeonatos", ContentTemplate = new DataTemplate(() => new DashboardOrganizadorPage()) }
                        }
                });
            } else if (usuario.Perfil == TipoPerfil.Arbitro) {
                this.Items.Add(new FlyoutItem {
                    Title = "Meus Jogos",
                    Items = {
                            new ShellContent { Title = "Ver Jogos Atribuidos", ContentTemplate = new DataTemplate(() => new MinhasPartidasPage()) }
                        }
                });
            } else if (usuario.Perfil == TipoPerfil.Patrocinador) {
                this.Items.Add(new FlyoutItem {
                    Title = "Minhas Campanhas",
                    Items = {
                            new ShellContent { Title = "Criar Campanha", ContentTemplate = new DataTemplate(() => new PropostasPatrocinioPage()) },
                            new ShellContent { Title = "Ver Campanhas", ContentTemplate = new DataTemplate(() => new CampanhasPage()) }
                        }
                });
            }

            this.Items.Add(new MenuItem {
                Text = "Sair",
                Command = new Command(() => {
                    MainThread.BeginInvokeOnMainThread(() => {
                        var serviceProvider = Application.Current.Handler.MauiContext.Services;
                        Application.Current.MainPage = serviceProvider.GetService<LoginPage>(); // <--- Usar GetService aqui  
                    });
                })
            });
        }
    }
}