using ArenaVirtual.ViewModels.CampeonatoPage;
using ArenaVirtual.ViewModels.Arbitro;
using Microsoft.Maui.Controls;
using System;

namespace ArenaVirtual.Views.CampeonatoPage;

public partial class GerenciarSolicitacoesPage : ContentPage {
    private readonly GerenciarSolicitacoesViewModel _viewModel;

    public GerenciarSolicitacoesPage(GerenciarSolicitacoesViewModel viewModel) {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    private async void AceitarTime_Clicked(object sender, EventArgs e) {
        if (sender is Button button && button.BindingContext is SolicitacaoTimeItemViewModel item) {
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
        if (sender is Button button && button.BindingContext is SolicitacaoArbitroItemViewModel item) {
            await _viewModel.AceitarArbitroCommand.ExecuteAsync(item);
        }
    }

    private async void RecusarArbitro_Clicked(object sender, EventArgs e) {
        if (sender is Button button && button.BindingContext is SolicitacaoArbitroItemViewModel item) {
            await _viewModel.RecusarArbitroCommand.ExecuteAsync(item);
        }
    }

    private async void AceitarPatrocinio_Clicked(object sender, EventArgs e) {
        if (sender is Button button && button.BindingContext is ViewModels.Patrocinio.SolicitacaoPatrocinioItemViewModel item) {

            await _viewModel.AceitarPatrocinioCommand.ExecuteAsync(item);
        }
    }

    private async void RecusarPatrocinio_Clicked(object sender, EventArgs e) {
        if (sender is Button button && button.BindingContext is ViewModels.Patrocinio.SolicitacaoPatrocinioItemViewModel item) {

            await _viewModel.RecusarPatrocinioCommand.ExecuteAsync(item);
        }
    }
}