// MauiProgram.cs
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using ArenaVirtual.Services;
using ArenaVirtual.Views;
using ArenaVirtual.ViewModels;
using Microsoft.Maui.ApplicationModel; // Necessário para FileSystem

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
        // Serviços
        builder.Services.AddTransient<IAlertService, AlertService>();

        // Registre DatabaseService como Singleton
        // NÃO chame InitializeAsync() aqui!
        builder.Services.AddSingleton<DatabaseService>(s => {
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "ArenaVirtual.db3");
            var databaseService = new DatabaseService(dbPath);
            // NÃO chame databaseService.InitializeAsync() aqui!
            return databaseService;
        });

        builder.Services.AddTransient<UsuarioService>();

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<PerfilViewModel>();

        // Views (Páginas)
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<PerfilPage>();

        // AppShell
        builder.Services.AddTransient<AppShell>();

        return builder.Build();
    }
}