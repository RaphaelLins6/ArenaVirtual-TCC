using Microsoft.Maui.Controls;
using ArenaVirtual.ViewModels.Patrocinador;
using ArenaVirtual.Popups; 
using ArenaVirtual.Services; 

namespace ArenaVirtual.Views.Patrocinador {
    public partial class DashboardPatrocinadorPage : ContentPage {
        private readonly DatabaseService _databaseService;
        private readonly IAlertService _alertService;

        public DashboardPatrocinadorPage(
            DashboardPatrocinadorViewModel viewModel,
            DatabaseService databaseService, 
            IAlertService alertService) { 

            InitializeComponent();
            Title = "Dashboard";
            BindingContext = viewModel;

            _databaseService = databaseService;
            _alertService = alertService;

            MessagingCenter.Subscribe<DetalhesCampanhaPopup>(this, "CampanhaAtualizada", async (sender) => {
                await viewModel.LoadCampanhasCommand.ExecuteAsync(null);
            });
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