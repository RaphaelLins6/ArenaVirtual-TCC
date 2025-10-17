using ArenaVirtual.ViewModels.Patrocinador;
using System.Diagnostics;

namespace ArenaVirtual.Views.Patrocinador {

    public partial class BuscarCampeonatosPage : ContentPage {

        private readonly BuscarCampeonatosViewModel _viewModel;

        public BuscarCampeonatosPage(BuscarCampeonatosViewModel viewModel) {
            InitializeComponent();

            _viewModel = viewModel;
            this.BindingContext = _viewModel;
        }

        protected override async void OnAppearing() {
            base.OnAppearing();
            // Carrega os campeonatos assim que a página é exibida
            await _viewModel.OnAppearingAsync();
        }

        private async void OnProporPatrocinioClicked(object sender, EventArgs e) {
            var button = sender as Button;
            if (button == null) return;

            var itemViewModel = button.BindingContext as CampeonatoPatrocinioItemViewModel;

            if (itemViewModel == null) {
                Debug.WriteLine("[Clicked] ERRO: Não foi possível obter o ItemViewModel.");
                return;
            }

            await _viewModel.NavegarParaPropostaAsync(itemViewModel);
        }
    }
}