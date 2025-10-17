using Microsoft.Maui.Controls;

namespace ArenaVirtual.Views.Patrocinador {
    public partial class DashboardPatrocinadorPage : ContentPage {
        public DashboardPatrocinadorPage(ViewModels.Patrocinador.DashboardPatrocinadorViewModel viewModel) {
            InitializeComponent();
            Title = "Dashboard";
            BindingContext = viewModel;
        }
    }
}