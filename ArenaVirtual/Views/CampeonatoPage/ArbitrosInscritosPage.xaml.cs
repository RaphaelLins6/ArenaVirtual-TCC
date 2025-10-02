namespace ArenaVirtual.Views.CampeonatoPage;

public partial class ArbitrosInscritosPage : ContentPage {
    public ArbitrosInscritosPage(ViewModels.CampeonatoPage.ArbitrosInscritosViewModel viewModel) { 
        InitializeComponent();
        this.BindingContext = viewModel;
    }
}