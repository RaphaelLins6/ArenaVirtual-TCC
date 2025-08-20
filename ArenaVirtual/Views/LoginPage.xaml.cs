using ArenaVirtual.ViewModels; 

namespace ArenaVirtual.Views {
    public partial class LoginPage : ContentPage {
        public LoginPage(LoginViewModel viewModel) { 
            InitializeComponent();
            BindingContext = viewModel;
        }

        private void Senha_Completed(object sender, EventArgs e) {
            if (BindingContext is LoginViewModel vm && vm.LoginCommand.CanExecute(null)) {
                vm.LoginCommand.Execute(null);
            }
        }
    }
}