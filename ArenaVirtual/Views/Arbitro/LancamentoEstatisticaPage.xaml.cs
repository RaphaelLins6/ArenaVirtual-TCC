using ArenaVirtual.ViewModels.Arbitro;
using Microsoft.Maui.Controls;

namespace ArenaVirtual.Views.Arbitro {
    public partial class LancamentoEstatisticaPage : ContentPage {
        public LancamentoEstatisticaPage(LancamentoEstatisticaViewModel viewModel) {
            InitializeComponent();

            BindingContext = viewModel;
        }
    }
}