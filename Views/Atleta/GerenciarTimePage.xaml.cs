using ArenaVirtual.ViewModels.Atleta;

namespace ArenaVirtual.Views.Atleta {
    public partial class GerenciarTimePage : ContentPage {
        private readonly GerenciarTimePageViewModel _viewModel;

        public GerenciarTimePage(GerenciarTimePageViewModel viewModel) {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing() {
            base.OnAppearing();
            await _viewModel.LoadData();
        }
    }
}