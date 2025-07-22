using ArenaVirtual.ViewModels.Organizador;
using ArenaVirtual.Services;

namespace ArenaVirtual.Views.Organizador {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DashboardOrganizadorPage : ContentPage {
        private readonly DashboardOrganizadorViewModel _viewModel;

        public DashboardOrganizadorPage() {
            InitializeComponent();
            var databaseService = App.Current?.Handler?.MauiContext?.Services?.GetRequiredService<DatabaseService>();
            _viewModel = new DashboardOrganizadorViewModel(databaseService!);
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing() {
            base.OnAppearing();
            await _viewModel.CarregarCampeonatosCommand.ExecuteAsync(null);
        }
    }
}