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

        // Método otimizado com o [RelayCommand] no ViewModel
        private async void OnRegisterEnterPressed(object sender, EventArgs e) {
            if (BindingContext is RegisterViewModel vm) {
                if (vm.RegistrarCommand.CanExecute(null)) {
                    await vm.RegistrarCommand.ExecuteAsync(null);
                }
            }
        }
    }
}
