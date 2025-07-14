using System;
using Microsoft.Maui.Controls;
using ArenaVirtual.Views;
using ArenaVirtual;

namespace ArenaVirtual {
    public partial class App : Application {
        public App() {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState) {
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            return new Window(new AppShell());
        }

        public async Task NavigateToRegisterPageAsync() {
            await Shell.Current.GoToAsync(nameof(RegisterPage));
        }
    }
}