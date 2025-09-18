using ArenaVirtual.ViewModels;
using ArenaVirtual.Services;
using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace ArenaVirtual.Views {
    public partial class LoginPage : ContentPage {
        private readonly ConnectivityService _connectivityService;

        public LoginPage(LoginViewModel viewModel, ConnectivityService connectivityService) {
            InitializeComponent();
            _connectivityService = connectivityService;
            BindingContext = viewModel;
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            _connectivityService.ConnectivityChanged += OnConnectivityChanged;
            if (BindingContext is LoginViewModel vm) {
                vm.UpdateConnectivityStatus();
            }
        }

        protected override void OnDisappearing() {
            base.OnDisappearing();
            _connectivityService.ConnectivityChanged -= OnConnectivityChanged;
        }

        private void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e) {
            if (BindingContext is LoginViewModel vm) {
                vm.UpdateConnectivityStatus();
            }
        }

        private async void OnLoginEnterPressed(object sender, EventArgs e) {
            if (BindingContext is LoginViewModel vm && vm.LoginCommand.CanExecute(null)) {
                await vm.LoginCommand.ExecuteAsync(null);
            }
        }
    }
}