using ArenaVirtual.ViewModels.Patrocinador;
using Microsoft.Maui.Controls;

namespace ArenaVirtual.Views.Patrocinador {

    public partial class InscricaoCampeonatoPage : ContentPage {

        public InscricaoCampeonatoPage(InscricaoCampeonatoViewModel viewModel) {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}