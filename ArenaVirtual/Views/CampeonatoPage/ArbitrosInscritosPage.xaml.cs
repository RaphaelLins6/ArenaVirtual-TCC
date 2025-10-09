using CommunityToolkit.Mvvm.Input;
using ArenaVirtual.ViewModels.CampeonatoPage;
using ArenaVirtual.ViewModels.Arbitro;

namespace ArenaVirtual.Views.CampeonatoPage;

public partial class ArbitrosInscritosPage : ContentPage {
    public ArbitrosInscritosPage(ViewModels.CampeonatoPage.ArbitrosInscritosViewModel viewModel) { 
        InitializeComponent();
        this.BindingContext = viewModel;
    }
    private async void RemoverArbitro_Clicked(object sender, EventArgs e) {
        var button = (Button)sender;

        if (button.BindingContext is SolicitacaoArbitroItemViewModel arbitroViewModel) {
            if (this.BindingContext is ArbitrosInscritosViewModel pageViewModel) {
                await ((IAsyncRelayCommand)pageViewModel.RemoverArbitroCommand).ExecuteAsync(arbitroViewModel);
            }
        }
    }
}