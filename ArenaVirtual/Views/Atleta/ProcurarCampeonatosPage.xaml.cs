using ArenaVirtual.ViewModels.Atleta;

namespace ArenaVirtual.Views.Atleta {
    public partial class ProcurarCampeonatosPage : ContentPage {
        private readonly ProcurarCampeonatosViewModel _viewModel;

        public ProcurarCampeonatosPage(ProcurarCampeonatosViewModel viewModel) {
            InitializeComponent();
            _viewModel = viewModel;
            this.BindingContext = _viewModel;
        }

        protected override async void OnAppearing() {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();
        }
    }
}