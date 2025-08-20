using ArenaVirtual.ViewModels.Atleta;

namespace ArenaVirtual.Views.Atleta {
    public partial class SolicitacaoTimePage : ContentPage {
        private readonly SolicitacaoTimePageViewModel _viewModel;

        public SolicitacaoTimePage(SolicitacaoTimePageViewModel viewModel) {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing() {
            base.OnAppearing();
            await _viewModel.LoadData();
        }
    }
}