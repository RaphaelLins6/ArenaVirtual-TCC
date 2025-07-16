using ArenaVirtual.ViewModels;

namespace ArenaVirtual.Views {
    public partial class RegisterPage : ContentPage {
        public RegisterPage() {
            InitializeComponent();
            BindingContext = new RegisterViewModel();
        }

        private void OnRegisterEnterPressed(object sender, EventArgs e) {
            if (BindingContext is LoginViewModel vm && vm.LoginCommand.CanExecute(null)) {
                vm.LoginCommand.Execute(null);
            }
        }

        private void OnVoltarClicked(object sender, EventArgs e) {
            Application.Current.MainPage = new LoginPage();
        }
    }
}