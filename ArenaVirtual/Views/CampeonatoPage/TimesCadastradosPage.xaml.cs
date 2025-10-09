using ArenaVirtual.ViewModels.CampeonatoPage;
using ArenaVirtual.Models;

namespace ArenaVirtual.Views.CampeonatoPage;

public partial class TimesCadastradosPage : ContentPage {
    public TimesCadastradosPage(TimesCadastradosViewModel vm) {

        InitializeComponent();

        BindingContext = vm;
    }

    // No arquivo TimesCadastradosPage.cs
    private async void RemoverTime_Clicked(object sender, EventArgs e) {
        var button = sender as Button;
        var timeParaRemover = button?.BindingContext as Time;

        // 1. Garante que o BindingContext é o ViewModel
        var viewModel = BindingContext as TimesCadastradosViewModel;

        if (timeParaRemover != null && viewModel != null) {

            // --- DEBUG XAML 1: Confirma que a lógica do botão foi alcançada ---
            System.Diagnostics.Debug.WriteLine($"[XAML-CLICKED] Botão Remover acionado para: {timeParaRemover.Nome}");

            bool confirmar = await DisplayAlert(
                "Confirmação",
                $"Deseja realmente remover o time '{timeParaRemover.Nome}' do campeonato? Isso removerá também os jogos relacionados.",
                "Sim",
                "Não"
            );

            if (confirmar) {
                try {
                    // Chamada segura do RelayCommand
                    if (viewModel.RemoverTimeCommand.CanExecute(timeParaRemover)) {
                        await viewModel.RemoverTimeCommand.ExecuteAsync(timeParaRemover);
                        System.Diagnostics.Debug.WriteLine("[XAML-CLICKED] Comando de remoção executado com sucesso.");
                    } else {
                        System.Diagnostics.Debug.WriteLine("[XAML-ERRO] Comando RemoverTimeCommand não pode ser executado.");
                    }
                } catch (Exception ex) {
                    // 2. CAPTURA CRÍTICA: Se o RelayCommand falhar ao iniciar, o erro estará aqui.
                    System.Diagnostics.Debug.WriteLine($"[XAML-ERRO CRITICO] Falha ao executar o comando no Code-Behind: {ex.Message}");
                    await DisplayAlert("Erro Crítico", "Ocorreu uma falha ao iniciar a remoção. Verifique o console.", "OK");
                }
            }
        }
    }
}