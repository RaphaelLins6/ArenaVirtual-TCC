using ArenaVirtual.Services;
using ArenaVirtual.Views;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace ArenaVirtual;

public partial class App : Application {
    public App() {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState) {
        Routing.RegisterRoute(nameof(Views.RegisterPage), typeof(Views.RegisterPage));
        Routing.RegisterRoute(nameof(Views.LoginPage), typeof(Views.LoginPage));
        Routing.RegisterRoute(nameof(Views.PerfilPage), typeof(Views.PerfilPage));

        var initialPage = this.Handler.MauiContext.Services.GetService<LoginPage>();
        var window = new Window(initialPage);

        // Assina o evento Created da Window para garantir que o Handler está pronto
        window.Created += async (sender, args) => {
            if (sender is Window createdWindow) {
                try {
                    var databaseService = createdWindow.Handler.MauiContext.Services.GetService<DatabaseService>();
                    if (databaseService != null) {
                        System.Diagnostics.Debug.WriteLine("Iniciando inicialização do DatabaseService...");
                        await databaseService.InitializeAsync();
                        System.Diagnostics.Debug.WriteLine("DatabaseService inicializado com sucesso.");
                    } else {
                        System.Diagnostics.Debug.WriteLine("Erro: DatabaseService não pôde ser obtido para inicialização.");
                    }
                }
                catch (Exception ex) {
                    System.Diagnostics.Debug.WriteLine($"Erro na inicialização do DatabaseService: {ex.Message}");
                }
            }
        };

        return window;
    }
}
