using ArenaVirtual.ViewModels;
using Microsoft.Maui.Controls;

namespace ArenaVirtual.Views {
    public partial class PerfilPage : ContentPage {
        private readonly PerfilViewModel _viewModel;

        // O construtor agora recebe apenas o ViewModel, que é um serviço que o DI pode resolver.
        public PerfilPage(PerfilViewModel viewModel) {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            // A lógica de carregar os dados ainda é chamada aqui.
            _viewModel.CarregarDadosDoUsuario();
            this.Focus();
        }

        protected override void OnDisappearing() {
            base.OnDisappearing();
        }
    }
}
