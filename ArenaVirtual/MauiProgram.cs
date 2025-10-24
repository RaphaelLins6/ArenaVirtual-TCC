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
        
        builder.Services.AddTransient<IAlertService, AlertService>();

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

        builder.Services.AddTransient<UsuarioService>();
        builder.Services.AddTransient<TimeService>();

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

        builder.Services.AddTransient<AlterarImagemPopup>();
        builder.Services.AddTransient<AlterarSenhaPopup>();
        builder.Services.AddTransient<EditarPerfilPopup>();
        builder.Services.AddTransient<AtribuirArbitrosPopup>();
        builder.Services.AddTransient<AlterarBannerPatrocinioPopup>();
        builder.Services.AddTransient<DetalhesCampanhaPopup>();

        return builder.Build();
    }
}