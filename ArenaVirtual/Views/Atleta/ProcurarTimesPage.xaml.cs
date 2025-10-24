using ArenaVirtual.ViewModels.Atleta;
using Microsoft.Maui.Controls;

namespace ArenaVirtual.Views.Atleta {
    public partial class ProcurarTimesPage : ContentPage {
        public ProcurarTimesPage(ProcurarTimesPageViewModel viewModel) {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override async void OnAppearing() {
            base.OnAppearing();
            if (BindingContext is ProcurarTimesPageViewModel vm) {
                await vm.CarregarTimesAsync();
            }
        }
    }
}