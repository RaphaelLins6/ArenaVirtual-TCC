
namespace ArenaVirtuall.Views;

public partial class Menu : ContentPage
{
    public Menu() {
        InitializeComponent();
    }

    private async void OnInicioClicked(object sender, EventArgs e) {
        await Navigation.PushAsync(new TelaInicial());
    }

    private async void OnCriarCampeonatoClicked(object sender, EventArgs e) {
        await Navigation.PushAsync(new CriarCampeonato());
    }

    private async void OnAtletaClicked(object sender, EventArgs e) {
        await Navigation.PushAsync(new PaginaAtleta());
    }
}