using ArenaVirtual.Models;
using ArenaVirtual.Services;
using ArenaVirtual.Views;
using ArenaVirtual.Views.Arbitro;
using ArenaVirtual.Views.Atleta;
using ArenaVirtual.Views.CampeonatoPage;
using ArenaVirtual.Views.Organizador;
using ArenaVirtual.Views.Patrocinador;
using System.Diagnostics;

namespace ArenaVirtual {
    public partial class AppShell : Shell {
        private readonly Usuario _usuarioLogado;
        private readonly IServiceProvider _serviceProvider;

        public AppShell(Usuario usuarioLogado, IServiceProvider serviceProvider) {
            InitializeComponent();
            _usuarioLogado = usuarioLogado;
            _serviceProvider = serviceProvider;

            Routing.RegisterRoute(nameof(HomePage), typeof(HomePage));
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(EditarCampeonatoPage), typeof(EditarCampeonatoPage));
            Routing.RegisterRoute(nameof(CriarTimePage), typeof(CriarTimePage));
            Routing.RegisterRoute(nameof(ProcurarTimesPage), typeof(ProcurarTimesPage));
            Routing.RegisterRoute(nameof(EntrarTimePage), typeof(EntrarTimePage));
            Routing.RegisterRoute(nameof(SolicitacaoTimePage), typeof(SolicitacaoTimePage));
            Routing.RegisterRoute(nameof(EditarTimePage), typeof(EditarTimePage));
            Routing.RegisterRoute(nameof(CampeonatoDetailPage), typeof(CampeonatoDetailPage));
            Routing.RegisterRoute(nameof(GerenciarSolicitacoesPage), typeof(GerenciarSolicitacoesPage));
            Routing.RegisterRoute(nameof(ProcurarCampeonatosPage), typeof(ProcurarCampeonatosPage));
            Routing.RegisterRoute(nameof(DashboardOrganizadorPage), typeof(DashboardOrganizadorPage));
            Routing.RegisterRoute(nameof(TimesCadastradosPage), typeof(TimesCadastradosPage));
            Routing.RegisterRoute(nameof(Views.Atleta.PartidasPage), typeof(Views.Atleta.PartidasPage));
            Routing.RegisterRoute(nameof(DashboardArbitroPage), typeof(DashboardArbitroPage));
            //Routing.RegisterRoute(nameof(LancamentoEstatisticaPage), typeof(LancamentoEstatisticaPage));
            Routing.RegisterRoute(nameof(CampeonatoInscricao), typeof(CampeonatoInscricao));
            Routing.RegisterRoute(nameof(ArbitrosInscritosPage), typeof(ArbitrosInscritosPage));

            CriarMenuPorPerfil(usuarioLogado);
        }

        private void CriarMenuPorPerfil(Usuario usuario) {
            this.Items.Clear();

            this.Items.Add(new FlyoutItem {
                Title = "Início",
                Route = "HomePage",
                Icon = "home_icon.png",
                Items = {
                    new ShellContent {
                        ContentTemplate = new DataTemplate(() => _serviceProvider.GetService<HomePage>())
                    }
                }
            });

            // CORREÇÃO: Usando o serviceProvider para obter a instância da PerfilPage
            this.Items.Add(new FlyoutItem {
                Title = "Meu Perfil",
                Route = "PerfilPage",
                Items = {
                    new ShellContent {
                        ContentTemplate = new DataTemplate(() => _serviceProvider.GetService<PerfilPage>())
                    }
                }
            });

            if (usuario.Perfil == TipoPerfil.Atleta) {
                this.Items.Add(new FlyoutItem {
                    Title = "Meu Time",
                    Items = {
                        new ShellContent { Title = "Informações do Time", ContentTemplate = new DataTemplate(() => _serviceProvider.GetService<MeusTimesPage>()) },
                        new ShellContent { Title = "Jogos", ContentTemplate = new DataTemplate(() => _serviceProvider.GetService<Views.Atleta.PartidasPage>()) }
                    }
                });
            } else if (usuario.Perfil == TipoPerfil.Organizador) {
                this.Items.Add(new FlyoutItem {
                    Title = "Gerenciar Campeonatos",
                    Items = {
                        new ShellContent { Title = "Criar Campeonato", ContentTemplate = new DataTemplate(() => _serviceProvider.GetService<CriarCampeonatoPage>()) },
                        new ShellContent { Title = "Meus Campeonatos", ContentTemplate = new DataTemplate(() => _serviceProvider.GetService<DashboardOrganizadorPage>()) }
                    }
                });
            } else if (usuario.Perfil == TipoPerfil.Arbitro) {
                this.Items.Add(new FlyoutItem {
                    Title = "Meus Jogos",
                    Items = {
                        new ShellContent { Title = "Ver Jogos Atribuidos", ContentTemplate = new DataTemplate(() => _serviceProvider.GetService<DashboardArbitroPage>()) }
                    }
                });
            } else if (usuario.Perfil == TipoPerfil.Patrocinador) {
                this.Items.Add(new FlyoutItem {
                    Title = "Minhas Campanhas",
                    Items = {
                        new ShellContent { Title = "Criar Campanha", ContentTemplate = new DataTemplate(() => _serviceProvider.GetService<PropostasPatrocinioPage>()) },
                        new ShellContent { Title = "Ver Campanhas", ContentTemplate = new DataTemplate(() => _serviceProvider.GetService<CampanhasPage>()) }
                    }
                });
            }

            this.Items.Add(new MenuItem {
                Text = "Sair",
                Command = new Command(() => {
                    MainThread.BeginInvokeOnMainThread(() => {
                        var loginPage = _serviceProvider.GetService<LoginPage>();
                        if (loginPage != null) {
                            Application.Current.MainPage = loginPage;
                        } else {
                            Debug.WriteLine("Erro: LoginPage não pôde ser resolvido pelo ServiceProvider.");
                        }
                    });
                })
            });
        }
    }
}
