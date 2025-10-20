using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls; // Necessário para Shell.Current.GoToAsync

namespace ArenaVirtual.ViewModels.Patrocinador {

    // Adicionado ObservableObject e atributos para roteamento (QueryProperty)
    [QueryProperty(nameof(CampeonatoId), "campeonatoId")]
    public partial class PropostaCampeonatoViewModel : ObservableObject {

        private readonly PatrocinioService _patrocinioService;
        private readonly CampeonatoService _campeonatoService;
        private readonly IAlertService _alertService;

        [ObservableProperty]
        private bool isBusy;

        // Propriedade para receber o ID do Campeonato via navegação
        [ObservableProperty]
        private string campeonatoId = string.Empty; // Recebe a GUID como string

        [ObservableProperty]
        private string nomeCampeonato = "Carregando...";

        // CORREÇÃO 1: Notifica o comando para reavaliar CanExecute quando o valor do patrocínio muda.
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(EnviarPropostaPatrocinioCommand))]
        private string valorPatrocinio = string.Empty; // Input do usuário para o valor

        [ObservableProperty]
        private string mensagemAdicional = string.Empty; // Input opcional do usuário

        // ⭐️ NOVAS PROPRIEDADES DE DATA ⭐️
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(EnviarPropostaPatrocinioCommand))]
        private DateTime dataInicio = DateTime.Now.Date; // Inicializa com a data atual

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(EnviarPropostaPatrocinioCommand))]
        private DateTime dataFim = DateTime.Now.Date.AddDays(7); // Inicializa com 7 dias no futuro

        private int _campeonatoInternalId;

        public PropostaCampeonatoViewModel(
          PatrocinioService patrocinioService,
          CampeonatoService campeonatoService,
          IAlertService alertService) {

            _patrocinioService = patrocinioService;
            _campeonatoService = campeonatoService;
            _alertService = alertService;
        }

        // Chamado após a propriedade CampeonatoId ser definida via QueryProperty
        partial void OnCampeonatoIdChanged(string value) {
            if (Guid.TryParse(value, out Guid clientAppId)) {
                // Chama o método para buscar o nome do campeonato e seu Id interno
                // Garante que a chamada seja assíncrona
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
                Debug.WriteLine($"Erro ao carregar detalhes do campeonato: {ex.Message}");
                NomeCampeonato = "Erro ao Carregar";
            } finally {
                IsBusy = false;
                // CORREÇÃO 2: Notifica o comando após IsBusy ser false e o ID interno ter sido carregado.
                EnviarPropostaPatrocinioCommand.NotifyCanExecuteChanged();
            }
        }

        // Comando principal para enviar a proposta
        [RelayCommand(CanExecute = nameof(CanSendProposta))]
        private async Task EnviarPropostaPatrocinioAsync() {
            if (!CanSendProposta()) return;

            // ⭐️ VALIDAÇÃO DA DATA ⭐️
            if (DataFim.Date < DataInicio.Date) {
                await _alertService.DisplayAlert("Erro de Data",
                    "A Data Fim deve ser posterior ou igual à Data Início.", "OK");
                return;
            }

            // ⭐️ CORREÇÃO PRINCIPAL: Tenta converter ValorPatrocinio (string) para decimal ⭐️
            if (!decimal.TryParse(ValorPatrocinio, System.Globalization.NumberStyles.Currency,
                                  System.Globalization.CultureInfo.CurrentCulture, out decimal valorNumerico)) {
                await _alertService.DisplayAlert("Erro de Valor",
                    "O valor do patrocínio não está em um formato numérico válido.", "OK");
                return;
            }

            IsBusy = true;

            try {
                // Inclui as datas na mensagem
                string periodo = $"Período Proposto: {DataInicio:dd/MM/yyyy} a {DataFim:dd/MM/yyyy}";

                // Prepara a mensagem completa para o organizador
                string mensagemCompleta = $"PERÍODO: {periodo}\n\nMENSAGEM: {MensagemAdicional}";

                // Chama o Service: (int campeonatoId, decimal valor, string mensagem)
                int result = await _patrocinioService.CriarPropostaPatrocinioAsync(
                    _campeonatoInternalId,
                    valorNumerico, // <--- VARIÁVEL DECIMAL CONVERTIDA
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
                Debug.WriteLine($"[EnviarPropostaPatrocinio] ERRO: {ex.Message}");
                await _alertService.DisplayAlert("Erro", "Ocorreu um erro inesperado ao enviar a proposta.", "OK");
            } finally {
                IsBusy = false;
            }
        }

        private bool CanSendProposta() {
            bool isNotBusy = !IsBusy;
            bool hasValue = !string.IsNullOrWhiteSpace(ValorPatrocinio);
            bool hasInternalId = _campeonatoInternalId > 0;

            // A DataFim deve ser maior ou igual à data atual (para que a proposta seja para o futuro/hoje)
            bool dataIsFutureOrToday = DataFim.Date >= DateTime.Now.Date;

            // CORREÇÃO 3: Adicionar Log de Debug para rastrear a habilitação do botão
            Debug.WriteLine($"[CanExecute] Proposta: Busy={isNotBusy} | Value={hasValue} | ID={hasInternalId} | Future={dataIsFutureOrToday}. Resultado: {isNotBusy && hasValue && hasInternalId && dataIsFutureOrToday}");

            // Verifica se o valor do patrocínio é válido, o campeonato interno foi carregado e a data é válida.
            return isNotBusy && hasValue && hasInternalId && dataIsFutureOrToday;
        }
    }
}