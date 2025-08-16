using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using ArenaVirtual.Services;
using ArenaVirtual.Views;
using ArenaVirtual.ViewModels;
using ArenaVirtual.ViewModels.Atleta;
using ArenaVirtual.Views.Atleta;

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

        builder.Services.AddSingleton<DatabaseService>(s => {
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "ArenaVirtual.db3");
            var databaseService = new DatabaseService(dbPath);
            return databaseService;
        });

        // Caminho do banco de dados SQLite
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "arenavirtual.db3");
        builder.Services.AddSingleton(new DatabaseService(dbPath)); // Registro correto com parâmetro

        // Serviços
        builder.Services.AddTransient<UsuarioService>();
        builder.Services.AddSingleton<TimeService>();

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<PerfilViewModel>();
        builder.Services.AddTransient<CriarTimePageViewModel>();
        builder.Services.AddTransient<MeuTimePageViewModel>();
        builder.Services.AddTransient<EntrarTimePageViewModel>();
        builder.Services.AddTransient<SolicitacaoTimePageViewModel>();

        // Páginas
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<PerfilPage>();
        builder.Services.AddTransient<CriarTimePage>();
        builder.Services.AddTransient<MeusTimesPage>();
        builder.Services.AddTransient<AppShell>();
        builder.Services.AddTransient<EntrarTimePage>();
        builder.Services.AddTransient<SolicitacaoTimePage>();

        return builder.Build();
    }
}