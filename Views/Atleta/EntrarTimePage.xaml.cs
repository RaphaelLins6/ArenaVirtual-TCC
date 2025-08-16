using ArenaVirtual.ViewModels.Atleta;

namespace ArenaVirtual.Views.Atleta {
    public partial class EntrarTimePage : ContentPage {
        public EntrarTimePage(EntrarTimePageViewModel viewModel) {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}