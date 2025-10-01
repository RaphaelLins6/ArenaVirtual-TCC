using Microsoft.Maui.Controls;
using ArenaVirtual.ViewModels.Atleta;
using System.Diagnostics;

namespace ArenaVirtual.Views.Atleta {

    public partial class PartidasPage : ContentPage {

        private readonly PartidasViewModel _viewModel;

        public PartidasPage(PartidasViewModel viewModel) {
            InitializeComponent();
            Title = "Jogos";
            // Usa a instância Singleton injetada
            BindingContext = _viewModel = viewModel;
        }

        protected override void OnAppearing() {
            base.OnAppearing();

            // Com o Singleton, o Count deve ser 2 aqui.
            Debug.WriteLine($"[PartidasPage.xaml.cs] OnAppearing: PartidasDoTime Count: {_viewModel.PartidasDoTime.Count}");
        }
    }
}