// MauiProgram.cs
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
        // Registrar seus serviços
        builder.Services.AddTransient<IAlertService, AlertService>();
        builder.Services.AddTransient<DatabaseService>();
        // ... (outros serviços)

        // Registrar seus ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        // ... (outros ViewModels)

        // Registrar suas Views (Páginas)
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<PerfilPage>();
        // ... (outras Views)

        // Registrar o AppShell
        builder.Services.AddTransient<AppShell>(); // Use AddTransient para permitir passar Usuario no construtor

        return builder.Build();
    }
}