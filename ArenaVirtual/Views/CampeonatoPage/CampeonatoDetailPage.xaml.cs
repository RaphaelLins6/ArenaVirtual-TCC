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

            // ?? CORREÇÃO APLICADA: REMOVIDA a chamada manual do LoadCampeonato no OnAppearing.
            // O carregamento inicial é feito APENAS no ApplyQueryAttributes do ViewModel, 
            // que possui a lógica para evitar recarregar ao retornar de um Modal.
            if (Campeonato != null) {
                // A linha abaixo FOI REMOVIDA:
                // _viewModel.LoadCampeonato(Campeonato);

                Debug.WriteLine($"[CampeonatoDetailPage] ViewModel.IsOrganizador no OnAppearing: {_viewModel.IsOrganizador}");
                Debug.WriteLine($"[CampeonatoDetailPage] Dados do campeonato recebidos: {Campeonato?.Nome}");
            }
        }

        private async void OnAnexarArbitrosClicked(object sender, EventArgs e) {
            // 1. Obter o objeto Jogo (passado como CommandParameter)
            var button = sender as Button;

            // Verifica se o objeto e o ViewModel são válidos
            if (button?.CommandParameter is not ArenaVirtual.Models.Jogo jogo) {
                System.Diagnostics.Debug.WriteLine("[DEBUG-CLICK-ERROR] Jogo não pôde ser recuperado do CommandParameter.");
                return;
            }

            if (BindingContext is not ArenaVirtual.ViewModels.CampeonatoPage.CampeonatoDetailViewModel viewModel) {
                System.Diagnostics.Debug.WriteLine("[DEBUG-CLICK-ERROR] ViewModel não encontrado.");
                return;
            }

            // Chamando o método subjacente AnexarArbitros do ViewModel.
            await viewModel.AnexarArbitros(jogo);
        }
    }
}