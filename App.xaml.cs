using System;
using Microsoft.Maui.Controls;
using ArenaVirtuall.Views;
using ArenaVirtual;

namespace ArenaVirtuall {
    public partial class App : Application {
        public App() {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState) {
            return new Window(new AppShell());
        }
    }
}
