using ArenaVirtual.Services;
using ArenaVirtual.ViewModels;

namespace ArenaVirtual.Views {
    public partial class RegisterPage : ContentPage {
        public RegisterPage() {
            InitializeComponent();
            // Obtém o serviço via ServiceProvider do MAUI
            var alertService = App.Current?.Handler?.MauiContext?.Services.GetService<IAlertService>();
            BindingContext = new RegisterViewModel(alertService);
        }

        public RegisterPage(RegisterViewModel viewModel) {
            InitializeComponent();
            BindingContext = viewModel;
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