using ArenaVirtual.ViewModels.Patrocinador;
using Microsoft.Maui.Controls;

namespace ArenaVirtual.Views.Patrocinador {

    public partial class PropostaCampeonatoPage : ContentPage {

        public PropostaCampeonatoPage(PropostaCampeonatoViewModel viewModel) {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}