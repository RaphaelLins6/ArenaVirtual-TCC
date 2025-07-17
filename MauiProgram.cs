using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using ArenaVirtual.Services;
using ArenaVirtual.Views;
using ArenaVirtual.ViewModels;

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

        builder.Services.AddTransient<UsuarioService>();

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<PerfilViewModel>();

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<PerfilPage>();

        builder.Services.AddTransient<AppShell>();

        return builder.Build();
    }
}