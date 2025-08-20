using ArenaVirtual.ViewModels;

namespace ArenaVirtual.Views {
    public partial class RegisterPage : ContentPage {
        private readonly IServiceProvider _serviceProvider;

        // Construtor principal para injeção de dependência.
        public RegisterPage(RegisterViewModel viewModel, IServiceProvider serviceProvider) {
            InitializeComponent();
            BindingContext = viewModel;
            _serviceProvider = serviceProvider;
        }

        private void OnRegisterEnterPressed(object sender, EventArgs e) {
            if (BindingContext is RegisterViewModel vm && vm.RegistrarCommand.CanExecute(null)) {
                vm.RegistrarCommand.Execute(null);
            }
        }
    }
}