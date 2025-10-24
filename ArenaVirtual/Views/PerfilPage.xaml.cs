using ArenaVirtual.ViewModels;
using Microsoft.Maui.Controls;

namespace ArenaVirtual.Views {
    public partial class PerfilPage : ContentPage {
        private readonly PerfilViewModel _viewModel;

        public PerfilPage(PerfilViewModel viewModel) {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            _viewModel.CarregarDadosDoUsuario();
            this.Focus();
        }

        protected override void OnDisappearing() {
            base.OnDisappearing();
        }
    }
}
