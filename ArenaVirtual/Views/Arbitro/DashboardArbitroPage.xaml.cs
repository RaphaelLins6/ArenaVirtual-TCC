using ArenaVirtual.ViewModels.Arbitro;

namespace ArenaVirtual.Views.Arbitro {

    // Você pode precisar adicionar a diretiva se estiver usando MVVM Toolkit
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DashboardArbitroPage : ContentPage {

        public DashboardArbitroPage(DashboardArbitroViewModel viewModel) {
            InitializeComponent();
            BindingContext = viewModel;

        }
    }
}