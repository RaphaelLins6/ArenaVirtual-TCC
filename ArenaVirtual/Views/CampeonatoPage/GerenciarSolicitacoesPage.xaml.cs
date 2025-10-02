using ArenaVirtual.ViewModels.CampeonatoPage;
using ArenaVirtual.ViewModels.Arbitro;

namespace ArenaVirtual.Views.CampeonatoPage;

public partial class GerenciarSolicitacoesPage : ContentPage {
    // É uma boa prática armazenar o ViewModel principal para facilitar o acesso
    private readonly GerenciarSolicitacoesViewModel _viewModel;

    public GerenciarSolicitacoesPage(GerenciarSolicitacoesViewModel viewModel) {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    // --- Métodos de Clique para SOLICITAÇÕES DE TIME ---

    private async void AceitarSolicitacao_Clicked(object sender, EventArgs e) {
        if (sender is Button button && button.BindingContext is SolicitacaoTimeItemViewModel item) {
            // Chamando o método da ViewModel manualmente
            await _viewModel.AceitarSolicitacaoAsync(item);
        }
    }

    private async void RecusarSolicitacao_Clicked(object sender, EventArgs e) {
        if (sender is Button button && button.BindingContext is SolicitacaoTimeItemViewModel item) {
            // Chamando o método da ViewModel manualmente
            await _viewModel.RecusarSolicitacaoAsync(item);
        }
    }

    // --- Métodos de Clique para SOLICITAÇÕES DE ÁRBITRO ---

    private async void AceitarArbitro_Clicked(object sender, EventArgs e) {
        if (sender is Button button && button.BindingContext is SolicitacaoArbitroItemViewModel item) {
            // Chamando o método da ViewModel manualmente
            await _viewModel.AceitarArbitroAsync(item);
        }
    }

    private async void RecusarArbitro_Clicked(object sender, EventArgs e) {
        if (sender is Button button && button.BindingContext is SolicitacaoArbitroItemViewModel item) {
            // Chamando o método da ViewModel manualmente
            await _viewModel.RecusarArbitroAsync(item);
        }
    }
}