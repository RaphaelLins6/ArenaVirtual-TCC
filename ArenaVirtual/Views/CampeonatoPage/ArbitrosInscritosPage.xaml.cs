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
        // 1. Obtém o botão que foi clicado
        var button = (Button)sender;

        // 2. Tenta obter o item de dados (SolicitacaoArbitroItemViewModel) do BindingContext do botão
        if (button.BindingContext is SolicitacaoArbitroItemViewModel arbitroViewModel) {
            // 3. Tenta obter o ViewModel da página (ArbitrosInscritosViewModel)
            if (this.BindingContext is ArbitrosInscritosViewModel pageViewModel) {
                // 4. Chama o IAsyncRelayCommand do ViewModel, passando o item como parâmetro
                // Usamos o ExecuteAsync para comandos assíncronos
                await ((IAsyncRelayCommand)pageViewModel.RemoverArbitroCommand).ExecuteAsync(arbitroViewModel);
            }
        }
    }
}