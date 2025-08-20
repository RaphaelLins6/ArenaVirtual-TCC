using ArenaVirtual.ViewModels;
using ArenaVirtual.Services;

namespace ArenaVirtual.Views.Organizador {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CriarCampeonatoPage : ContentPage {
        public CriarCampeonatoPage() {
            InitializeComponent();
            var databaseService = App.Current?.Handler?.MauiContext?.Services?.GetRequiredService<DatabaseService>();
            BindingContext = new CriarCampeonatoViewModel(databaseService!);
        }
    }
}