using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls; 

namespace ArenaVirtual.ViewModels.Patrocinador {

    [QueryProperty(nameof(CampeonatoId), "campeonatoId")]
    public partial class PropostaCampeonatoViewModel : ObservableObject {

        private readonly PatrocinioService _patrocinioService;
        private readonly CampeonatoService _campeonatoService;
        private readonly IAlertService _alertService;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string campeonatoId = string.Empty; // Recebe a GUID como string

        [ObservableProperty]
        private string nomeCampeonato = "Carregando...";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(EnviarPropostaPatrocinioCommand))]
        private string valorPatrocinio = string.Empty; 

        [ObservableProperty]
        private string mensagemAdicional = string.Empty; 

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(EnviarPropostaPatrocinioCommand))]
        private DateTime dataInicio = DateTime.Now.Date; 

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(EnviarPropostaPatrocinioCommand))]
        private DateTime dataFim = DateTime.Now.Date.AddDays(7); 

        private int _campeonatoInternalId;

        public PropostaCampeonatoViewModel(
          PatrocinioService patrocinioService,
          CampeonatoService campeonatoService,
          IAlertService alertService) {

            _patrocinioService = patrocinioService;
            _campeonatoService = campeonatoService;
            _alertService = alertService;
        }

        partial void OnCampeonatoIdChanged(string value) {
            if (Guid.TryParse(value, out Guid clientAppId)) {
                Task.Run(() => CarregarDetalhesCampeonatoAsync(clientAppId));
            }
        }

        private async Task CarregarDetalhesCampeonatoAsync(Guid clientAppId) {
            IsBusy = true;
            try {
                var campeonato = await _campeonatoService.ObterPorClientAppIdAsync(clientAppId);

                if (campeonato != null) {
                    NomeCampeonato = campeonato.Nome;
                    _campeonatoInternalId = campeonato.Id; // Armazena o ID interno
                } else {
                    NomeCampeonato = "Campeonato Não Encontrado";
                }
            } catch (Exception ex) {
                //Debug.WriteLine($"Erro ao carregar detalhes do campeonato: {ex.Message}");
                NomeCampeonato = "Erro ao Carregar";
            } finally {
                IsBusy = false;
                EnviarPropostaPatrocinioCommand.NotifyCanExecuteChanged();
            }
        }

        [RelayCommand(CanExecute = nameof(CanSendProposta))]
        private async Task EnviarPropostaPatrocinioAsync() {
            if (!CanSendProposta()) return;

            if (DataFim.Date < DataInicio.Date) {
                await _alertService.DisplayAlert("Erro de Data",
                    "A Data Fim deve ser posterior ou igual à Data Início.", "OK");
                return;
            }

            if (!decimal.TryParse(ValorPatrocinio, System.Globalization.NumberStyles.Currency,
                                  System.Globalization.CultureInfo.CurrentCulture, out decimal valorNumerico)) {
                await _alertService.DisplayAlert("Erro de Valor",
                    "O valor do patrocínio não está em um formato numérico válido.", "OK");
                return;
            }

            IsBusy = true;

            try {
                string periodo = $"Período Proposto: {DataInicio:dd/MM/yyyy} a {DataFim:dd/MM/yyyy}";

                string mensagemCompleta = $"PERÍODO: {periodo}\n\nMENSAGEM: {MensagemAdicional}";

                int result = await _patrocinioService.CriarPropostaPatrocinioAsync(
                    _campeonatoInternalId,
                    valorNumerico, 
                    mensagemCompleta
                );

                if (result > 0) {
                    await _alertService.DisplayAlert("Sucesso",
                                        $"Proposta de Patrocínio de R$ {valorNumerico} para '{NomeCampeonato}' enviada com sucesso!",
                                        "OK");

                    await Shell.Current.GoToAsync("..");
                } else {
                    await _alertService.DisplayAlert("Erro", "Falha ao enviar a proposta. Verifique se você está logado.", "OK");
                }
            } catch (Exception ex) {
                //Debug.WriteLine($"[EnviarPropostaPatrocinio] ERRO: {ex.Message}");
                await _alertService.DisplayAlert("Erro", "Ocorreu um erro inesperado ao enviar a proposta.", "OK");
            } finally {
                IsBusy = false;
            }
        }

        private bool CanSendProposta() {
            bool isNotBusy = !IsBusy;
            bool hasValue = !string.IsNullOrWhiteSpace(ValorPatrocinio);
            bool hasInternalId = _campeonatoInternalId > 0;

            bool dataIsFutureOrToday = DataFim.Date >= DateTime.Now.Date;

            //Debug.WriteLine($"[CanExecute] Proposta: Busy={isNotBusy} | Value={hasValue} | ID={hasInternalId} | Future={dataIsFutureOrToday}. Resultado: {isNotBusy && hasValue && hasInternalId && dataIsFutureOrToday}");

            return isNotBusy && hasValue && hasInternalId && dataIsFutureOrToday;
        }
    }
}