using ArenaVirtual.ViewModels;
using ArenaVirtual.Services;

namespace ArenaVirtual.Views {
    public partial class HomePage : ContentPage {
        public HomePage() {
            InitializeComponent();

            var databaseService = new DatabaseService(Path.Combine(FileSystem.AppDataDirectory, "arena.db"));
            BindingContext = new HomeViewModel(databaseService);
        }
    }
}
