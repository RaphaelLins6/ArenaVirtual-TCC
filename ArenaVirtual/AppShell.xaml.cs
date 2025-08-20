using ArenaVirtual.Models;
using ArenaVirtual.Services;
using ArenaVirtual.Views;
using ArenaVirtual.Views.Arbitro;
using ArenaVirtual.Views.Atleta;
using ArenaVirtual.Views.Organizador;
using ArenaVirtual.Views.Patrocinador;

namespace ArenaVirtual {
    public partial class AppShell : Shell {
        private readonly Usuario _usuarioLogado;

        public AppShell(Usuario usuarioLogado) {
            InitializeComponent();
            _usuarioLogado = usuarioLogado;

            Routing.RegisterRoute(nameof(HomePage), typeof(HomePage));
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(EditarCampeonatoPage), typeof(EditarCampeonatoPage));
            Routing.RegisterRoute(nameof(CriarTimePage), typeof(CriarTimePage));
            Routing.RegisterRoute(nameof(ProcurarTimesPage), typeof(ProcurarTimesPage));
            Routing.RegisterRoute(nameof(EntrarTimePage), typeof(EntrarTimePage));
            Routing.RegisterRoute(nameof(SolicitacaoTimePage), typeof(SolicitacaoTimePage));
            Routing.RegisterRoute(nameof(EditarTimePage), typeof(EditarTimePage));
            CriarMenuPorPerfil(usuarioLogado);
        }

        private void CriarMenuPorPerfil(Usuario usuario) {
            this.Items.Clear();

            var serviceProvider = Application.Current?.Handler?.MauiContext?.Services;
            if (serviceProvider == null) {
                Console.WriteLine("Erro: ServiceProvider não pôde ser resolvido.");
                return;
            }
            var alertService = serviceProvider.GetService<IAlertService>();

            this.Items.Add(new FlyoutItem {
                Title = "Início",
                Route = "HomePage",
                Icon = "home_icon.png", 
                Items = {
                    new ShellContent {
                        ContentTemplate = new DataTemplate(() => new HomePage())
                    }
                }
            });

            this.Items.Add(new ShellContent {
                Title = "Meu Perfil",
                ContentTemplate = new DataTemplate(() => {
                    return new PerfilPage(_usuarioLogado, alertService!, serviceProvider!);
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
                        new ShellContent { Title = "Meus Campeonatos", ContentTemplate = new DataTemplate(() => new DashboardOrganizadorPage()) }
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
                        var localServiceProvider = Application.Current?.Handler?.MauiContext?.Services;
                        if (localServiceProvider != null) {
                            if (Application.Current?.Windows.Count > 0) {
                                var loginPage = localServiceProvider.GetService<LoginPage>();
                                if (loginPage != null) {
                                    Application.Current.Windows[0].Page = loginPage;
                                } else {
                                    Console.WriteLine("Erro: LoginPage não pôde ser resolvido pelo ServiceProvider.");
                                }
                            } else {
                                Console.WriteLine("Erro: Nenhuma janela do aplicativo encontrada.");
                            }
                        }
                    });
                })
            });
        }
    }
}
