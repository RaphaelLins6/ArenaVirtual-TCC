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
                _viewModel.LoadCampeonato(Campeonato);
                Debug.WriteLine($"[CampeonatoDetailPage] Dados do campeonato recebidos: {Campeonato?.Nome}");
            }
        }
    }
}