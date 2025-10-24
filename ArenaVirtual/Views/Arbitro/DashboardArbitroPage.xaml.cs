using ArenaVirtual.ViewModels.Arbitro;
using ArenaVirtual.Models.ViewModels.Shared;
using System.Diagnostics;

namespace ArenaVirtual.Views.Arbitro {

    public partial class DashboardArbitroPage : ContentPage {

        public DashboardArbitroPage(DashboardArbitroViewModel viewModel) {
            InitializeComponent();
            BindingContext = viewModel;

        }

        protected override async void OnAppearing() {
            base.OnAppearing();

            if (BindingContext is DashboardArbitroViewModel viewModel) {
                await viewModel.LoadPartidasCommand.ExecuteAsync(null);
            }
        }

        private void OnLancarEstatisticasClicked(object sender, EventArgs e) {
            var button = sender as Button;

            if (button?.CommandParameter is JogoDetalheViewModel jogoDetalhe) {
                if (BindingContext is DashboardArbitroViewModel viewModel) {
                    //Debug.WriteLine("[CLICKED LOG] PartidaSelecionadaAsync acionado via Code-Behind (Clicked).");

                    viewModel.PartidaSelecionadaCommand.Execute(jogoDetalhe);
                } else {
                    //Debug.WriteLine("[CLICKED LOG ERROR] BindingContext não é DashboardArbitroViewModel.");
                }
            } else {
                //Debug.WriteLine("[CLICKED LOG ERROR] CommandParameter é nulo ou não é JogoDetalheViewModel.");
            }
        }
    }
}