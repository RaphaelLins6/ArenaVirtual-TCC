using Microsoft.Maui.Hosting;
using CommunityToolkit.Maui;

public static class MauiProgram {
    public static MauiApp CreateMauiApp() {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<ArenaVirtual.App>() // Ensure this is called before UseMauiCommunityToolkit
            .UseMauiCommunityToolkit() // Add this to enable the Community Toolkit
            .ConfigureFonts(fonts => {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        return builder.Build();
    }
}
