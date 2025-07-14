using ArenaVirtual.Services;

namespace ArenaVirtual;

public partial class App : Application {
    public static DatabaseService Database { get; private set; } = null!; // Use null-forgiving operator to suppress warning

    public App() {
        InitializeComponent();
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "ArenaVirtual.db3");
        Database = new DatabaseService(dbPath);
    }

    protected override Window CreateWindow(IActivationState? activationState) { // Updated to match nullability
        return new Window(new AppShell());
    }
}
