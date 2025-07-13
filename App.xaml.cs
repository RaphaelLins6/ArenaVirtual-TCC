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
            return new Window(new NavigationPage(new LoginPage()));
        }
    }
}