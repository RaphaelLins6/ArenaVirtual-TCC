using ArenaVirtual.ViewModels.Atleta;

namespace ArenaVirtual.Views.Atleta {
    public partial class EditarTimePage : ContentPage {
        public EditarTimePage(EditarTimePageViewModel viewModel) {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override async void OnAppearing() {
            base.OnAppearing();
            if (BindingContext is EditarTimePageViewModel viewModel) {
                await viewModel.LoadDataAsync();
            }
        }
    }
}