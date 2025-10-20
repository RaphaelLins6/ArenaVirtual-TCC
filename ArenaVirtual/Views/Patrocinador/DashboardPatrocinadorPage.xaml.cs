using Microsoft.Maui.Controls;
using ArenaVirtual.ViewModels.Patrocinador;
using ArenaVirtual.Popups;
using ArenaVirtual.Services;
using System.Threading.Tasks;

namespace ArenaVirtual.Views.Patrocinador {
    public partial class DashboardPatrocinadorPage : ContentPage {
        private readonly DatabaseService _databaseService;
        private readonly IAlertService _alertService;
        private readonly DashboardPatrocinadorViewModel _viewModel; 

        public DashboardPatrocinadorPage(
        DashboardPatrocinadorViewModel viewModel,
        DatabaseService databaseService,
        IAlertService alertService) {

            InitializeComponent();
            Title = "Dashboard";
            BindingContext = viewModel;

            _viewModel = viewModel; 
            _databaseService = databaseService;
            _alertService = alertService;
        }

        protected override void OnAppearing() {
            base.OnAppearing();

            MessagingCenter.Unsubscribe<DetalhesCampanhaPopup>(this, "CampanhaAtualizada"); 

            MessagingCenter.Subscribe<DetalhesCampanhaPopup>(this, "CampanhaAtualizada", async (sender) => {
                await _viewModel.LoadCampanhasCommand.ExecuteAsync(null);
            });

            if (_viewModel.LoadCampanhasCommand.CanExecute(null)) {
                _viewModel.LoadCampanhasCommand.Execute(null);
            }
        }

        private async void BotaoVerDetalhes_Clicked(object sender, EventArgs e) {
            if (sender is Button button && button.BindingContext is CampanhaAtivaViewModel campanhaSelecionada) {
                await Task.Delay(50);
                var detalhesPopup = new DetalhesCampanhaPopup(campanhaSelecionada, _databaseService, _alertService);
                await Navigation.PushModalAsync(detalhesPopup);
            }
        }

        protected override void OnDisappearing() {
            base.OnDisappearing();
            MessagingCenter.Unsubscribe<DetalhesCampanhaPopup>(this, "CampanhaAtualizada");
        }
    }
}