namespace ArenaVirtual.Views.CampeonatoPage;
using ArenaVirtual.ViewModels.CampeonatoPage;

public partial class GerenciarSolicitacoesPage : ContentPage {
    public GerenciarSolicitacoesPage(GerenciarSolicitacoesViewModel viewModel) {
        InitializeComponent();
        BindingContext = viewModel;
    }
}