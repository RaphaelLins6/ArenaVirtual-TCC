using ArenaVirtual.ViewModels.Atleta;

namespace ArenaVirtual.Views.Atleta {
    public partial class EstatisticasPessoaisPage : ContentPage {
        public EstatisticasPessoaisPage(EstatisticasPessoaisViewModel viewModel) {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}