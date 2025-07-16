using ArenaVirtual.Services;
using ArenaVirtual.ViewModels;
using Microsoft.Extensions.DependencyInjection; // Adicione este using
using System; // Para EventArgs

namespace ArenaVirtual.Views {
    public partial class RegisterPage : ContentPage {
        private readonly IServiceProvider _serviceProvider; // Adicione esta linha

        public RegisterPage() {
            InitializeComponent();
            // Obtém o serviço via ServiceProvider do MAUI  
            var alertService = App.Current?.Handler?.MauiContext?.Services.GetService<IAlertService>();
            BindingContext = new RegisterViewModel(alertService);
        }

        // Construtor principal para injeção de dependência.
        // Recebe o RegisterViewModel e o IServiceProvider
        public RegisterPage(RegisterViewModel viewModel, IServiceProvider serviceProvider) { // <--- Modifique AQUI
            InitializeComponent();
            BindingContext = viewModel;
            _serviceProvider = serviceProvider; // Armazene o ServiceProvider
        }

        private void OnRegisterEnterPressed(object sender, EventArgs e) {
            if (BindingContext is LoginViewModel vm && vm.LoginCommand.CanExecute(null)) {
                vm.LoginCommand.Execute(null);
            }
        }
    }
}