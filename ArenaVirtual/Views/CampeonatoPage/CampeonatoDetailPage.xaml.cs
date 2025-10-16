using Microsoft.Maui.Controls;
using ArenaVirtual.Models;
using ArenaVirtual.ViewModels.CampeonatoPage;
using System.Diagnostics;

namespace ArenaVirtual.Views.CampeonatoPage {
    [QueryProperty(nameof(Campeonato), "Campeonato")]
    public partial class CampeonatoDetailPage : ContentPage {
        private readonly CampeonatoDetailViewModel _viewModel;

        public Campeonato Campeonato { get; set; }

        public CampeonatoDetailPage(CampeonatoDetailViewModel viewModel) {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            if (Campeonato != null) {
                Debug.WriteLine($"[CampeonatoDetailPage] ViewModel.IsOrganizador no OnAppearing: {_viewModel.IsOrganizador}");
                Debug.WriteLine($"[CampeonatoDetailPage] Dados do campeonato recebidos: {Campeonato?.Nome}");
            }
        }

        private async void OnAnexarArbitrosClicked(object sender, EventArgs e) {
            var button = sender as Button;
            if (button?.CommandParameter is not ArenaVirtual.Models.Jogo jogo) {
                System.Diagnostics.Debug.WriteLine("[DEBUG-CLICK-ERROR] Jogo não pôde ser recuperado do CommandParameter.");
                return;
            }
            if (BindingContext is not ArenaVirtual.ViewModels.CampeonatoPage.CampeonatoDetailViewModel viewModel) {
                System.Diagnostics.Debug.WriteLine("[DEBUG-CLICK-ERROR] ViewModel não encontrado.");
                return;
            }
            await viewModel.AnexarArbitros(jogo);
        }
    }
}