namespace ArenaVirtual.Popups;

public partial class AtribuirArbitrosPopup : ContentPage {

    public AtribuirArbitrosPopup(/* ... */) {
        InitializeComponent();
    }

    private async void Cancelar_Clicked(object sender, EventArgs e) {
        await Application.Current.MainPage.Navigation.PopModalAsync();
    }

    private async void ConfirmarAtribuicao_Clicked(object sender, EventArgs e) {
        await Application.Current.MainPage.Navigation.PopModalAsync();
    }
}