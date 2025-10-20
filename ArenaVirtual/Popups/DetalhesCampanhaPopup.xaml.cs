using ArenaVirtual.Models;
using ArenaVirtual.Services;
using ArenaVirtual.ViewModels.Patrocinador;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Globalization;
using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace ArenaVirtual.Popups;

public partial class DetalhesCampanhaPopup : ContentPage, INotifyPropertyChanged {
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private bool _isBusy;
    public bool IsBusy {
        get => _isBusy;
        set {
            if (_isBusy != value) {
                _isBusy = value;
                System.Diagnostics.Debug.WriteLine($"[POPUP-ISBUSY] NOVO VALOR: {_isBusy}");
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

        BindingContext = _campanhaVM;
        System.Diagnostics.Debug.WriteLine("[POPUP] Construtor DetalhesCampanhaPopup finalizado.");
    }

    protected override async void OnAppearing() {
        base.OnAppearing();
        System.Diagnostics.Debug.WriteLine("[POPUP] OnAppearing iniciado.");
        await CarregarDadosCampanhaAsync();
    }

    private async Task CarregarDadosCampanhaAsync() {
        System.Diagnostics.Debug.WriteLine("[POPUP-ASYNC-1] CarregarDadosCampanhaAsync iniciado.");

        if (_campanhaVM.CampanhaId <= 0) return;

        IsBusy = true;
        System.Diagnostics.Debug.WriteLine("[POPUP-ASYNC-2] IsBusy = True.");

        CampanhaPatrocinio? campanhaModel = null;
        try {
            System.Diagnostics.Debug.WriteLine("[POPUP-ASYNC-3] Chamando GetCampanhaByIdAsync...");
            campanhaModel = await _databaseService.GetCampanhaByIdAsync(_campanhaVM.CampanhaId);
            System.Diagnostics.Debug.WriteLine($"[POPUP-ASYNC-4] GetCampanhaByIdAsync retornado. Modelo é nulo? {campanhaModel == null}");

        } catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine($"[POPUP-ASYNC-ERROR] Erro ao carregar dados: {ex.Message}");
            await _alertService.DisplayAlert("Erro de Carregamento", "Não foi possível buscar os dados completos da campanha.", "OK");
            return;
        } finally {
            IsBusy = false;
            System.Diagnostics.Debug.WriteLine("[POPUP-ASYNC-5] IsBusy = False (Finally).");
        }

        if (campanhaModel != null) {
            System.Diagnostics.Debug.WriteLine("[POPUP-ASYNC-6] Atribuindo dados aos controles.");

            ValorPropostaEntry.Text = campanhaModel.ValorProposta.ToString("N2", CultureInfo.CurrentCulture);

            // Datas
            DataInicioPicker.Date = campanhaModel.Inicio;
            DataFimPicker.Date = campanhaModel.Fim;
        }

        if (_campanhaVM.Status == "Finalizada") {
            System.Diagnostics.Debug.WriteLine("[POPUP-ASYNC-7] Desabilitando controles (Status: Finalizada).");
            ValorPropostaEntry.IsEnabled = false;
            DataInicioPicker.IsEnabled = false;
            DataFimPicker.IsEnabled = false;
            ExcluirCampanhaButton.IsVisible = false;
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
            var campanhaModel = await _databaseService.GetCampanhaByIdAsync(_campanhaVM.CampanhaId);

            if (campanhaModel == null) {
                await _alertService.DisplayAlert("Erro", "Campanha não encontrada para salvar.", "OK");
                return;
            }

            campanhaModel.Inicio = DataInicioPicker.Date;
            campanhaModel.Fim = DataFimPicker.Date;

            decimal valor;
            if (decimal.TryParse(ValorPropostaEntry.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out valor)) {
                campanhaModel.ValorProposta = valor;
            } else {
                await _alertService.DisplayAlert("Atenção", "O valor da proposta inserido é inválido.", "OK");
                return;
            }

            await _databaseService.AtualizarCampanhaPatrocinioAsync(campanhaModel);

            await _alertService.DisplayAlert("Sucesso", "Campanha atualizada com sucesso!", "OK");

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
            var campanhaModel = await _databaseService.GetCampanhaByIdAsync(_campanhaVM.CampanhaId);

            if (campanhaModel == null) {
                await _alertService.DisplayAlert("Erro", "Campanha não encontrada para exclusão.", "OK");
                return;
            }

            await _databaseService.DeletarCampanhaPatrocinioAsync(campanhaModel);

            await _alertService.DisplayAlert("Sucesso", "Campanha excluída com sucesso!", "OK");

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