
namespace ArenaVirtuall.Views;

public partial class Menu : ContentPage
{
    public Menu() {
        InitializeComponent();
    }

    private async void OnInicioClicked(object sender, EventArgs e) {
        // Verifica se a MainPage é um FlyoutPage
        if (Application.Current.MainPage is FlyoutPage flyoutPage &&
            flyoutPage.Detail is NavigationPage navigationPage) {
            // Navega para a página TelaInicial
            await navigationPage.PushAsync(new TelaInicial());

            // Fecha o menu lateral
            flyoutPage.IsPresented = false;
        }
    }

    private async void OnCriarCampeonatoClicked(object sender, EventArgs e) {
        if (Application.Current.MainPage is FlyoutPage flyoutPage &&
            flyoutPage.Detail is NavigationPage navigationPage) {
            // Navega para a página TelaInicial
            await navigationPage.PushAsync(new CriarCampeonato());

            // Fecha o menu lateral
            flyoutPage.IsPresented = false;
        }
    }

    private async void OnAtletaClicked(object sender, EventArgs e) {
        if (Application.Current.MainPage is FlyoutPage flyoutPage &&
            flyoutPage.Detail is NavigationPage navigationPage) {
            // Navega para a página TelaInicial
            await navigationPage.PushAsync(new PaginaAtleta());

            // Fecha o menu lateral
            flyoutPage.IsPresented = false;
        }
    }
}