using ArenaVirtual.ViewModels;
using System;
using System.Threading.Tasks;

namespace ArenaVirtual.Views {
    public partial class LoginPage : ContentPage {
        public LoginPage(LoginViewModel viewModel) {
            InitializeComponent();
            BindingContext = viewModel;
        }

        private async void OnLoginEnterPressed(object sender, EventArgs e) {
            // Este método será chamado quando o usuário pressionar Enter no campo Senha.
            // Executa o comando de login do ViewModel.
            if (BindingContext is LoginViewModel vm && vm.LoginCommand.CanExecute(null)) {
                await vm.LoginCommand.ExecuteAsync(null);
            }
        }
    }
}
