using ArenaVirtual.ViewModels.CampeonatoPage;
using ArenaVirtual.ViewModels.Arbitro;
using Microsoft.Maui.Controls;
using System;

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

    private async void AceitarTime_Clicked(object sender, EventArgs e) {
        // Usa o BindingContext do botão que é o item da lista (SolicitacaoTimeItemViewModel)
        if (sender is Button button && button.BindingContext is SolicitacaoTimeItemViewModel item) {
            // Chama o comando MVVM definido na ViewModel
            await _viewModel.AceitarTimeCommand.ExecuteAsync(item);
        }
    }

    private async void RecusarTime_Clicked(object sender, EventArgs e) {
        if (sender is Button button && button.BindingContext is SolicitacaoTimeItemViewModel item) {
            await _viewModel.RecusarTimeCommand.ExecuteAsync(item);
        }
    }

    // --- Métodos de Clique para SOLICITAÇÕES DE ÁRBITRO ---

    private async void AceitarArbitro_Clicked(object sender, EventArgs e) {
        // Usa o BindingContext do botão que é o item da lista (SolicitacaoArbitroItemViewModel)
        if (sender is Button button && button.BindingContext is SolicitacaoArbitroItemViewModel item) {
            // Chama o comando MVVM definido na ViewModel
            await _viewModel.AceitarArbitroCommand.ExecuteAsync(item);
        }
    }

    private async void RecusarArbitro_Clicked(object sender, EventArgs e) {
        if (sender is Button button && button.BindingContext is SolicitacaoArbitroItemViewModel item) {
            await _viewModel.RecusarArbitroCommand.ExecuteAsync(item);
        }
    }
}