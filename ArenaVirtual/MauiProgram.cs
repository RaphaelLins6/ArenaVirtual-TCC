using ArenaVirtual.Popups; 
using ArenaVirtual.Services;
using ArenaVirtual.ViewModels;
using ArenaVirtual.ViewModels.Arbitro;
using ArenaVirtual.ViewModels.Atleta;
using ArenaVirtual.ViewModels.CampeonatoPage; 
using ArenaVirtual.ViewModels.Organizador;
using ArenaVirtual.ViewModels.Patrocinador;
using ArenaVirtual.Views;
using ArenaVirtual.Views.Arbitro;
using ArenaVirtual.Views.Atleta;
using ArenaVirtual.Views.CampeonatoPage; 
using ArenaVirtual.Views.Organizador;
using ArenaVirtual.Views.Patrocinador;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace ArenaVirtual;

public static class MauiProgram {
    public static MauiApp CreateMauiApp() {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts => {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif
        // Registro de serviços como Singletons ou Transients para injeção de dependência.
        // Singleton é usado para serviços que persistem durante toda a vida do app (ex: bancos de dados, sincronização).
        // Transient é usado para serviços ou páginas que podem ser criados e descartados (ex: ViewModels).

        // Adicione o serviço de alerta como Transient.
        builder.Services.AddTransient<IAlertService, AlertService>();

        // Registre os serviços de dados e sincronização como Singletons.
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "arenavirtual.db3");
        builder.Services.AddSingleton(new DatabaseService(dbPath));
        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddSingleton<SyncService>();
        builder.Services.AddSingleton<CampeonatoService>();
        builder.Services.AddSingleton<ConnectivityService>();
        builder.Services.AddSingleton(SessaoService.Instancia);
        builder.Services.AddSingleton<IJogoService, JogoService>();
        builder.Services.AddSingleton<JogoService>();
        builder.Services.AddSingleton<PatrocinioService>();

        // Registre os serviços de domínio como Transients
        builder.Services.AddTransient<UsuarioService>();
        builder.Services.AddTransient<TimeService>();

        // Registre todos os ViewModels como Transient
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<PerfilViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<CriarTimePageViewModel>();
        builder.Services.AddTransient<MeuTimePageViewModel>();
        builder.Services.AddTransient<EntrarTimePageViewModel>();
        builder.Services.AddTransient<SolicitacaoTimePageViewModel>();
        builder.Services.AddTransient<EditarTimePageViewModel>();
        builder.Services.AddTransient<EditarCampeonatoViewModel>();
        builder.Services.AddTransient<CriarCampeonatoViewModel>();
        builder.Services.AddTransient<ProcurarCampeonatosViewModel>();
        builder.Services.AddTransient<GerenciarSolicitacoesViewModel>();
        builder.Services.AddTransient<CampeonatoDetailViewModel>();
        builder.Services.AddTransient<DashboardOrganizadorViewModel>();
        builder.Services.AddTransient<TimesCadastradosViewModel>();
        builder.Services.AddSingleton<PartidasViewModel>();
        builder.Services.AddTransient<LancamentoEstatisticaViewModel>();
        builder.Services.AddTransient<DashboardArbitroViewModel>();
        builder.Services.AddTransient<CampeonatoInscricaoViewModel>();
        builder.Services.AddTransient<ArbitrosInscritosViewModel>();
        builder.Services.AddTransient<EstatisticasPessoaisViewModel>();
        builder.Services.AddTransient<DashboardPatrocinadorViewModel>();
        builder.Services.AddTransient<PropostaCampeonatoViewModel>();
        builder.Services.AddTransient<BuscarCampeonatosViewModel>();

        // Registre as Páginas e Popups como Transient
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<PerfilPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<CriarTimePage>();
        builder.Services.AddTransient<MeusTimesPage>();
        builder.Services.AddTransient<AppShell>();
        builder.Services.AddTransient<EntrarTimePage>();
        builder.Services.AddTransient<SolicitacaoTimePage>();
        builder.Services.AddTransient<EditarTimePage>();
        builder.Services.AddTransient<CriarCampeonatoPage>(); 
        builder.Services.AddTransient<EditarCampeonatoPage>();
        builder.Services.AddTransient<ProcurarCampeonatosPage>();
        builder.Services.AddTransient<GerenciarSolicitacoesPage>();
        builder.Services.AddTransient<CampeonatoDetailPage>();
        builder.Services.AddTransient<DashboardOrganizadorPage>();
        builder.Services.AddTransient<TimesCadastradosPage>();
        builder.Services.AddTransient<Views.Atleta.PartidasPage>();
        builder.Services.AddTransient<DashboardArbitroPage>();
        builder.Services.AddTransient<LancamentoEstatisticaPage>();
        builder.Services.AddTransient<CampeonatoInscricao>();
        builder.Services.AddTransient<ArbitrosInscritosPage>();
        builder.Services.AddTransient<EstatisticasPessoaisPage>();
        builder.Services.AddTransient<DashboardPatrocinadorPage>();
        builder.Services.AddTransient<PropostaCampeonatoPage>();
        builder.Services.AddTransient<BuscarCampeonatosPage>();

        // Registre os popups que agora recebem injeção de dependência
        builder.Services.AddTransient<AlterarImagemPopup>();
        builder.Services.AddTransient<AlterarSenhaPopup>();
        builder.Services.AddTransient<EditarPerfilPopup>();
        builder.Services.AddTransient<AtribuirArbitrosPopup>();

        return builder.Build();
    }
}