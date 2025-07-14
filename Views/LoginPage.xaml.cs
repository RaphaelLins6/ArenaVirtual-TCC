using ArenaVirtual.Models;
using ArenaVirtual.ViewModels;
using ArenaVirtual.Services;

namespace ArenaVirtual.Views {
    public partial class LoginPage : ContentPage {
        public LoginPage() {
            InitializeComponent();
            BindingContext = new ArenaVirtual.ViewModels.LoginViewModel();
            System.Diagnostics.Debug.WriteLine(BindingContext?.GetType().Name); // Deve mostrar "LoginViewModel"
        }
    }
}