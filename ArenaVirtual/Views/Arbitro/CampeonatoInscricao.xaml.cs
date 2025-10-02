using ArenaVirtual.ViewModels.Arbitro;
using Microsoft.Maui.Controls;

namespace ArenaVirtual.Views.Arbitro {

    public partial class CampeonatoInscricao : ContentPage {

        private readonly CampeonatoInscricaoViewModel _viewModel;

        public CampeonatoInscricao(CampeonatoInscricaoViewModel viewModel) {

            InitializeComponent();

            _viewModel = viewModel;
            this.BindingContext = _viewModel;
        }

        // NOVO: Sobrescreve o método OnAppearing padrão
        protected override async void OnAppearing() {
            base.OnAppearing();

            // Chama a lógica de carregamento do ViewModel
            // O CarregarCampeonatosAsync é quem cuida do IsBusy
            if (_viewModel.CarregarCampeonatosCommand.CanExecute(null)) {
                await _viewModel.CarregarCampeonatosCommand.ExecuteAsync(string.Empty);
            }
        }
    }
}