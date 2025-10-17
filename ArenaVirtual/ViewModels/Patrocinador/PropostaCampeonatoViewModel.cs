using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System;
using System.Threading.Tasks;

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
            // ... (restante do método permanece inalterado) ...
            if (!CanSendProposta()) return;

            IsBusy = true;

            try {
                // Concatena valor e mensagem adicional para a propriedade 'Mensagem'
                string mensagemCompleta = $"PROPOSTA: R$ {ValorPatrocinio}\n\nMENSAGEM: {MensagemAdicional}";

                // O método CriarPropostaPatrocinioAsync espera o ID interno (int) do campeonato.
                int result = await _patrocinioService.CriarPropostaPatrocinioAsync(_campeonatoInternalId, mensagemCompleta);

                if (result > 0) {
                    await _alertService.DisplayAlert("Sucesso",
                                                     $"Proposta de Patrocínio de R$ {ValorPatrocinio} para '{NomeCampeonato}' enviada com sucesso!",
                                                     "OK");

                    // Navega de volta para o Dashboard
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

            // CORREÇÃO 3: Adicionar Log de Debug para rastrear a habilitação do botão
            Debug.WriteLine($"[CanExecute] Proposta: Busy={isNotBusy} | Value={hasValue} | ID={hasInternalId}. Resultado: {isNotBusy && hasValue && hasInternalId}");

            // Verifica se o valor do patrocínio é válido (não nulo/vazio) e se o campeonato interno foi carregado.
            return isNotBusy && hasValue && hasInternalId;
        }
    }
}