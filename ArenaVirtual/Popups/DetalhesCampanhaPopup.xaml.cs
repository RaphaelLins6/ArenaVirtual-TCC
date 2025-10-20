using ArenaVirtual.Models;
using ArenaVirtual.Services;
using ArenaVirtual.ViewModels.Patrocinador;
using System;
using System.ComponentModel;
using System.Threading.Tasks; // Necessário para Task
using System.Globalization;
using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace ArenaVirtual.Popups;

// A interface INotifyPropertyChanged é necessária para a propriedade IsBusy
public partial class DetalhesCampanhaPopup : ContentPage, INotifyPropertyChanged {
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    // Propriedade IsBusy (Mantida)
    private bool _isBusy;
    public bool IsBusy {
        get => _isBusy;
        set {
            if (_isBusy != value) {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }
    }

    private readonly CampanhaAtivaViewModel _campanhaVM;
    private readonly DatabaseService _databaseService;
    private readonly IAlertService _alertService;

    public DetalhesCampanhaPopup(CampanhaAtivaViewModel campanhaVM, DatabaseService databaseService, IAlertService alertService) {
        InitializeComponent();

        _campanhaVM = campanhaVM;
        _databaseService = databaseService;
        _alertService = alertService;

        // O BindingContext desta página será o ViewModel CampanhaAtivaViewModel
        BindingContext = _campanhaVM;
        System.Diagnostics.Debug.WriteLine("[POPUP] Construtor DetalhesCampanhaPopup finalizado."); // ?? Ponto de Log 1 ??
    }

    protected override async void OnAppearing() {
        base.OnAppearing();
        System.Diagnostics.Debug.WriteLine("[POPUP] OnAppearing iniciado.");
        await CarregarDadosCampanhaAsync();
    }

    private async Task CarregarDadosCampanhaAsync() {
        System.Diagnostics.Debug.WriteLine("[POPUP-ASYNC-1] CarregarDadosCampanhaAsync iniciado.");

        if (_campanhaVM.CampanhaId <= 0) return;

        // 1. Obter o modelo completo (CampanhaPatrocinio) do banco de dados para os campos que não estão no VM
        IsBusy = true; // Inicia o indicador de carregamento
        System.Diagnostics.Debug.WriteLine("[POPUP-ASYNC-2] IsBusy = True.");

        CampanhaPatrocinio? campanhaModel = null;
        try {
            System.Diagnostics.Debug.WriteLine("[POPUP-ASYNC-3] Chamando GetCampanhaByIdAsync...");
            campanhaModel = await _databaseService.GetCampanhaByIdAsync(_campanhaVM.CampanhaId);
            System.Diagnostics.Debug.WriteLine($"[POPUP-ASYNC-4] GetCampanhaByIdAsync retornado. Modelo é nulo? {campanhaModel == null}");

        } catch (Exception ex) {
            // Bloco de tratamento de erro do banco de dados
            System.Diagnostics.Debug.WriteLine($"[POPUP-ASYNC-ERROR] Erro ao carregar dados: {ex.Message}");
            await _alertService.DisplayAlert("Erro de Carregamento", "Não foi possível buscar os dados completos da campanha.", "OK");
            return; // Garante que não tentará usar um modelo nulo se houver erro
        } finally {
            // Garante que o indicador DESLIGA após a operação, mesmo em caso de erro
            IsBusy = false;
            System.Diagnostics.Debug.WriteLine("[POPUP-ASYNC-5] IsBusy = False (Finally).");
        }

        if (campanhaModel != null) {
            System.Diagnostics.Debug.WriteLine("[POPUP-ASYNC-6] Atribuindo dados aos controles.");

            // Supondo que você adicionará um campo "ValorProposta" (decimal) no seu CampanhaPatrocinio Model
            // Exemplo para o ValorProposta:
            // ValorPropostaEntry.Text = campanhaModel.ValorProposta.ToString("F2", CultureInfo.InvariantCulture);
            ValorPropostaEntry.Text = "5000,00"; // Usando um valor mock para o teste

            // Datas
            DataInicioPicker.Date = campanhaModel.Inicio;
            DataFimPicker.Date = campanhaModel.Fim;
        }

        // Se a campanha estiver finalizada, desabilita a edição e exclusão
        if (_campanhaVM.Status == "Finalizada") {
            System.Diagnostics.Debug.WriteLine("[POPUP-ASYNC-7] Desabilitando controles (Status: Finalizada).");
            ValorPropostaEntry.IsEnabled = false;
            DataInicioPicker.IsEnabled = false;
            DataFimPicker.IsEnabled = false;
            ExcluirCampanhaButton.IsVisible = false; // Não permite excluir campanhas antigas
        }

        System.Diagnostics.Debug.WriteLine("[POPUP-ASYNC-8] CarregarDadosCampanhaAsync finalizado com sucesso.");
    }

    private async void Cancelar_Clicked(object sender, EventArgs e) {
        await Navigation.PopModalAsync();
    }

    private async void Salvar_Clicked(object sender, EventArgs e) {
        if (IsBusy) return;

        IsBusy = true;

        try {
            // 1. Obter o modelo completo
            var campanhaModel = await _databaseService.GetCampanhaByIdAsync(_campanhaVM.CampanhaId);

            if (campanhaModel == null) {
                await _alertService.DisplayAlert("Erro", "Campanha não encontrada para salvar.", "OK");
                return;
            }

            // 2. Atualizar modelo com os valores dos controles
            campanhaModel.Inicio = DataInicioPicker.Date;
            campanhaModel.Fim = DataFimPicker.Date;
            // Converter ValorProposta (Ajuste conforme seu modelo final)
            // decimal.TryParse(ValorPropostaEntry.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out decimal valor);
            // campanhaModel.ValorProposta = valor; 

            // 3. Salvar no banco de dados
            await _databaseService.AtualizarCampanhaPatrocinioAsync(campanhaModel);

            // 4. Fechar e notificar
            await _alertService.DisplayAlert("Sucesso", "Campanha atualizada com sucesso!", "OK");

            // Notificar o Dashboard para recarregar a lista (Melhor prática)
            MessagingCenter.Send(this, "CampanhaAtualizada");

            await Navigation.PopModalAsync();

        } catch (Exception ex) {
            Debug.WriteLine($"[DETALHES POPUP] Erro ao salvar campanha: {ex.Message}");
            await _alertService.DisplayAlert("Erro", $"Ocorreu um erro ao salvar a campanha: {ex.Message}", "OK");
        } finally {
            IsBusy = false;
        }
    }

    private async void ExcluirCampanha_Clicked(object sender, EventArgs e) {
        if (IsBusy) return;

        bool confirmacao = await _alertService.DisplayAlert(
            "Confirmação",
            $"Tem certeza que deseja excluir a campanha '{_campanhaVM.NomeCampanha}'?",
            "Sim, Excluir",
            "Cancelar");

        if (!confirmacao) return;

        IsBusy = true;
        try {
            // 1. Precisamos obter o objeto completo para deletar
            var campanhaModel = await _databaseService.GetCampanhaByIdAsync(_campanhaVM.CampanhaId);

            if (campanhaModel == null) {
                await _alertService.DisplayAlert("Erro", "Campanha não encontrada para exclusão.", "OK");
                return;
            }

            // 2. Deletar no banco de dados
            await _databaseService.DeletarCampanhaPatrocinioAsync(campanhaModel);

            // 3. Fechar e notificar
            await _alertService.DisplayAlert("Sucesso", "Campanha excluída com sucesso!", "OK");

            // Notificar o Dashboard para recarregar a lista
            MessagingCenter.Send(this, "CampanhaAtualizada");

            await Navigation.PopModalAsync();

        } catch (Exception ex) {
            Debug.WriteLine($"[DETALHES POPUP] Erro ao excluir campanha: {ex.Message}");
            await _alertService.DisplayAlert("Erro", $"Ocorreu um erro ao excluir a campanha: {ex.Message}", "OK");
        } finally {
            IsBusy = false;
        }
    }
}