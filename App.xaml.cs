using System;
using Microsoft.Maui.Controls;
using ArenaVirtuall.Views;

namespace ArenaVirtuall
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState) {
            return new Window(new FlyoutMenu()); // Replace TelaInicial with your root page
        }
    }
}