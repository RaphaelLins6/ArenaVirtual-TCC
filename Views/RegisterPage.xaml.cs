using ArenaVirtual.ViewModels;

namespace ArenaVirtual.Views {
    public partial class RegisterPage : ContentPage {
        public RegisterPage() {
            InitializeComponent();
            BindingContext = new RegisterViewModel();
        }
    }
}