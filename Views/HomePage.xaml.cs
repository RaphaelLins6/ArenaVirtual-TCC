using ArenaVirtual.ViewModels;
using ArenaVirtual.Services;

namespace ArenaVirtual.Views {
    public partial class HomePage : ContentPage {
        public HomePage() {
            InitializeComponent();

            var databaseService = App.Current?.Handler?.MauiContext?.Services?.GetRequiredService<DatabaseService>();
            BindingContext = new HomeViewModel(databaseService!);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is ArenaVirtual.ViewModels.HomeViewModel vm)
                await vm.CarregarCampeonatos();
        }
    }
}
