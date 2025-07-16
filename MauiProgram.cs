// MauiProgram.cs
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using ArenaVirtual.Services;
using ArenaVirtual.ViewModels;
using ArenaVirtual.Views;

namespace ArenaVirtual;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>().ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        }).UseMauiCommunityToolkit();
#if DEBUG
        builder.Logging.AddDebug();
#endif
        // Registre o serviço de alerta, ViewModel e a Page
        builder.Services.AddTransient<IAlertService, AlertService>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<RegisterPage>();

        return builder.Build();
    }
}