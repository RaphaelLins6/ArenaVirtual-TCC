using ArenaVirtual.ViewModels.Arbitro;

namespace ArenaVirtual.Views.Arbitro {

    // Você pode precisar adicionar a diretiva se estiver usando MVVM Toolkit
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DashboardArbitroPage : ContentPage {

        public DashboardArbitroPage(DashboardArbitroViewModel viewModel) {
            InitializeComponent();
            BindingContext = viewModel;

        }

        protected override async void OnAppearing() {
            base.OnAppearing();

            if (BindingContext is DashboardArbitroViewModel viewModel) {
                // Esta é a linha CRUCIAL que provavelmente está faltando ou errada.
                await viewModel.LoadPartidasCommand.ExecuteAsync(null);
            }
        }
    }
}