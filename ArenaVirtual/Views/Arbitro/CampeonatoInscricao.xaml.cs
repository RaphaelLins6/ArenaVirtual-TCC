using ArenaVirtual.ViewModels.Arbitro;

namespace ArenaVirtual.Views.Arbitro {

    public partial class CampeonatoInscricao : ContentPage {

        private readonly CampeonatoInscricaoViewModel _viewModel;

        public CampeonatoInscricao(CampeonatoInscricaoViewModel viewModel) {

            InitializeComponent();

            _viewModel = viewModel;
            this.BindingContext = _viewModel;
        }

        protected override async void OnAppearing() {
            base.OnAppearing();

            if (_viewModel.CarregarCampeonatosCommand.CanExecute(null)) {
                await _viewModel.CarregarCampeonatosCommand.ExecuteAsync(string.Empty);
            }
        }

        private async void OnSolicitarArbitragemClicked(object sender, EventArgs e) {
            if (sender is Button button) {
                if (button.BindingContext is CampeonatoItemViewModel itemViewModel) {
                    await _viewModel.SolicitarArbitragemAsync(itemViewModel);
                }
            }
        }
    }
}