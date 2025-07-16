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

    private void Senha_Completed(object sender, EventArgs e) {
            if (BindingContext is LoginViewModel vm && vm.LoginCommand.CanExecute(null)) {
                vm.LoginCommand.Execute(null);
            }
        }
    }
}