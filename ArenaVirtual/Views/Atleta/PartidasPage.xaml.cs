using Microsoft.Maui.Controls;
using ArenaVirtual.ViewModels.Atleta;
using System.Diagnostics;

namespace ArenaVirtual.Views.Atleta {

    public partial class PartidasPage : ContentPage {

        private readonly PartidasViewModel _viewModel;

        public PartidasPage(PartidasViewModel viewModel) {
            InitializeComponent();
            Title = "Jogos";
            BindingContext = _viewModel = viewModel;
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            //Debug.WriteLine($"[PartidasPage.xaml.cs] OnAppearing: PartidasDoTime Count: {_viewModel.PartidasDoTime.Count}");
        }
    }
}