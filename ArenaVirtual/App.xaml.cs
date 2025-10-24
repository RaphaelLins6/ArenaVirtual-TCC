using ArenaVirtual.Models;
using ArenaVirtual.Services;
using ArenaVirtual.Views;

namespace ArenaVirtual;

public partial class App : Application {
    public static TimeService? TimeService { get; private set; }
    public static string DatabasePath { get; private set; }


    public App() {
        InitializeComponent();
        DatabasePath = Path.Combine(FileSystem.AppDataDirectory, "arenavirtual.db3");

    }

    protected override Window CreateWindow(IActivationState? activationState) {
        Routing.RegisterRoute(nameof(Views.RegisterPage), typeof(Views.RegisterPage));
        Routing.RegisterRoute(nameof(Views.LoginPage), typeof(Views.LoginPage));
        Routing.RegisterRoute(nameof(Views.PerfilPage), typeof(Views.PerfilPage));

        var serviceProvider = this.Handler?.MauiContext?.Services;
        LoginPage initialPage;

        if (serviceProvider != null) {
            initialPage = serviceProvider.GetRequiredService<LoginPage>();
        } else {
            throw new InvalidOperationException("Service provider is not available during app initialization.");
        }

        var window = new Window(initialPage);

        window.Created += async (sender, args) => {
            if (sender is Window createdWindow) {
                try {
                    var windowServiceProvider = createdWindow.Handler?.MauiContext?.Services;

                    if (windowServiceProvider != null) {
                        var databaseService = windowServiceProvider.GetRequiredService<DatabaseService>();
                        //System.Diagnostics.Debug.WriteLine("Iniciando inicialização do DatabaseService...");
                        await databaseService.InitializeAsync();
                        //System.Diagnostics.Debug.WriteLine("DatabaseService inicializado com sucesso.");

                        var syncService = windowServiceProvider.GetRequiredService<SyncService>();
                        await syncService.SyncAsync(new Progress<string>(status => System.Diagnostics.Debug.WriteLine($"[SyncStatus] {status}")));

                    } else {
                        //System.Diagnostics.Debug.WriteLine("Erro: ServiceProvider da janela não pôde ser obtido para inicialização do DatabaseService.");
                    }
                } catch (Exception ex) {
                    //System.Diagnostics.Debug.WriteLine($"Erro na inicialização/sincronização inicial: {ex.Message}");
                }
            }
        };

        return window;
    }

    public static Usuario? CurrentUser { get; set; }
}