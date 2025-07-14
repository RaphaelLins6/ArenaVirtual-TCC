using ArenaVirtual.Models;
using ArenaVirtual.Views;

namespace ArenaVirtual {
    public partial class AppShell : Shell {
        public AppShell() {
            InitializeComponent();
            //Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        }

        public AppShell(Usuario usuario) {
            InitializeComponent();
            CriarMenuPorPerfil(usuario);
        }

        private void CriarMenuPorPerfil(Usuario usuario) {
            Items.Clear();

            switch (usuario.Perfil) {
                case "Organizador":
                    Items.Add(new FlyoutItem { Title = "Dashboard", Items = { new ShellContent { ContentTemplate = new DataTemplate(typeof(Views.Organizador.DashboardOrganizadorPage)) } } });
                    Items.Add(new FlyoutItem { Title = "Campeonatos", Items = { new ShellContent { ContentTemplate = new DataTemplate(typeof(Views.Organizador.CriarCampeonatoPage)) } } });
                    Items.Add(new FlyoutItem { Title = "Times", Items = { new ShellContent { ContentTemplate = new DataTemplate(typeof(Views.Organizador.TimesPage)) } } });
                    Items.Add(new FlyoutItem { Title = "Partidas", Items = { new ShellContent { ContentTemplate = new DataTemplate(typeof(Views.Organizador.PartidasPage)) } } });
                    Items.Add(new FlyoutItem { Title = "Relatórios", Items = { new ShellContent { ContentTemplate = new DataTemplate(typeof(Views.Organizador.RelatoriosPage)) } } });
                    break;

                case "Atleta":
                    Items.Add(new FlyoutItem { Title = "Dashboard", Items = { new ShellContent { ContentTemplate = new DataTemplate(typeof(Views.Atleta.DashboardAtletaPage)) } } });
                    Items.Add(new FlyoutItem { Title = "Meus Times", Items = { new ShellContent { ContentTemplate = new DataTemplate(typeof(Views.Atleta.MeusTimesPage)) } } });
                    Items.Add(new FlyoutItem { Title = "Estatísticas", Items = { new ShellContent { ContentTemplate = new DataTemplate(typeof(Views.Atleta.EstatisticasPessoaisPage)) } } });
                    Items.Add(new FlyoutItem { Title = "Procurar Times", Items = { new ShellContent { ContentTemplate = new DataTemplate(typeof(Views.Atleta.ProcurarTimesPage)) } } });
                    Items.Add(new FlyoutItem { Title = "Partidas", Items = { new ShellContent { ContentTemplate = new DataTemplate(typeof(Views.Atleta.PartidasPage)) } } });
                    break;

                case "Arbitro":
                    Items.Add(new FlyoutItem { Title = "Dashboard", Items = { new ShellContent { ContentTemplate = new DataTemplate(typeof(Views.Arbitro.DashboardArbitroPage)) } } });
                    Items.Add(new FlyoutItem { Title = "Minhas Partidas", Items = { new ShellContent { ContentTemplate = new DataTemplate(typeof(Views.Arbitro.MinhasPartidasPage)) } } });
                    Items.Add(new FlyoutItem { Title = "Avaliações", Items = { new ShellContent { ContentTemplate = new DataTemplate(typeof(Views.Arbitro.AvaliacaoArbitralPage)) } } });
                    Items.Add(new FlyoutItem { Title = "Disponibilidade", Items = { new ShellContent { ContentTemplate = new DataTemplate(typeof(Views.Arbitro.DisponibilidadePage)) } } });
                    break;

                case "Patrocinador":
                    Items.Add(new FlyoutItem { Title = "Dashboard", Items = { new ShellContent { ContentTemplate = new DataTemplate(typeof(Views.Patrocinador.DashboardPatrocinadorPage)) } } });
                    Items.Add(new FlyoutItem { Title = "Campanhas", Items = { new ShellContent { ContentTemplate = new DataTemplate(typeof(Views.Patrocinador.CampanhasPage)) } } });
                    Items.Add(new FlyoutItem { Title = "Estatísticas", Items = { new ShellContent { ContentTemplate = new DataTemplate(typeof(Views.Patrocinador.EstatisticasCampanhaPage)) } } });
                    Items.Add(new FlyoutItem { Title = "Propostas", Items = { new ShellContent { ContentTemplate = new DataTemplate(typeof(Views.Patrocinador.PropostasPatrocinioPage)) } } });
                    break;
            }

            Items.Add(new MenuItem {
                Text = "Sair",
                IconImageSource = "logout.png", // Opcional, se tiver ícone
                Command = new Command(async () => {
                    // Verifica se a propriedade Windows e o índice 0 não são nulos
                    if (Application.Current?.Windows?.Count > 0 && Application.Current.Windows[0] != null) {
                        Application.Current.Windows[0].Page = new LoginPage();
                    }
                })
            });
        }
    }
}