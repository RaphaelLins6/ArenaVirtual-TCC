using ArenaVirtual.Services;
using ArenaVirtual.Views;

namespace ArenaVirtual;

public partial class App : Application {
    public static DatabaseService Database { get; private set; } = null!;

    public App() {
        InitializeComponent();

        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "ArenaVirtual.db3");
        Database = new DatabaseService(dbPath);
    }

    protected override Window CreateWindow(IActivationState? activationState) {
        // Seus registros de rota estão ok
        Routing.RegisterRoute(nameof(Views.RegisterPage), typeof(Views.RegisterPage));
        Routing.RegisterRoute(nameof(Views.LoginPage), typeof(Views.LoginPage));
        Routing.RegisterRoute(nameof(Views.PerfilPage), typeof(Views.PerfilPage));

        // CORREÇÃO AQUI: Obtenha a LoginPage do ServiceProvider
        // Isso garante que a LoginPage e seu LoginViewModel (com IAlertService) sejam criados corretamente.
        return new Window(this.Handler.MauiContext.Services.GetService<LoginPage>());
    }
}
