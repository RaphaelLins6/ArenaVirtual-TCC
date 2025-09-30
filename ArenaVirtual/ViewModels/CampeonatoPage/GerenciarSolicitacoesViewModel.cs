// EM: ArenaVirtual.ViewModels.CampeonatoPage/GerenciarSolicitacoesViewModel.cs
using ArenaVirtual.Models;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ArenaVirtual.ViewModels.CampeonatoPage {
    [QueryProperty(nameof(Campeonato), "Campeonato")]
    public partial class GerenciarSolicitacoesViewModel : ObservableObject {

        private readonly DatabaseService _databaseService;
        private readonly CampeonatoService _campeonatoService;
        private readonly SessaoService _sessaoService;
        private readonly TimeService _timeService;
        private readonly IAlertService _alertService;
        private readonly UsuarioService _usuarioService;

        [ObservableProperty]
        private bool isBusy;

        // Propriedade calculada para o CanExecute
        private bool IsNotBusy => !IsBusy;

        [ObservableProperty]
        private Campeonato campeonato;

        [ObservableProperty]
        private ObservableCollection<SolicitacaoTimeItemViewModel> solicitacoesPendentes;

        [ObservableProperty]
        private bool isListEmpty;

        public GerenciarSolicitacoesViewModel(
           DatabaseService databaseService,
           CampeonatoService campeonatoService,
           SessaoService sessaoService,
           TimeService timeService,
           IAlertService alertService,
           UsuarioService usuarioService) {
            _databaseService = databaseService;
            _campeonatoService = campeonatoService;
            _sessaoService = sessaoService;
            _timeService = timeService;
            _alertService = alertService;
            _usuarioService = usuarioService;
            SolicitacoesPendentes = new ObservableCollection<SolicitacaoTimeItemViewModel>();
        }

        partial void OnCampeonatoChanged(Campeonato value) {
            if (value != null) {
                // Dispara o carregamento dos dados quando o Campeonato é setado
                _ = LoadSolicitacoesAsync();
            }
        }

        // A) Método LoadSolicitacoesAsync
        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        public async Task LoadSolicitacoesAsync() {
            Debug.WriteLine("Iniciando LoadSolicitacoesAsync...");

            if (Campeonato is null) {
                Debug.WriteLine("Erro: Objeto Campeonato não foi recebido. Não é possível carregar solicitações.");
                return;
            }

            IsBusy = true;
            IEnumerable<Convite> convites = null;
            try {
                // CHAMADA AO MÉTODO DO DATABASE SERVICE (como definido nas correções anteriores)
                convites = await _databaseService.ObterConvitesPendentesAsync(Campeonato.ClientAppId);

                Debug.WriteLine($"Encontradas {convites?.Count() ?? 0} solicitações pendentes.");

                var tarefas = convites.Select(c => _timeService.ObterPorClientAppIdAsync(c.TimeClientAppId)).ToList();
                var times = await Task.WhenAll(tarefas);

                MainThread.BeginInvokeOnMainThread(() => {
                    SolicitacoesPendentes.Clear();

                    var convitesList = convites.ToList();

                    for (int i = 0; i < convitesList.Count; i++) {
                        var convite = convitesList[i];
                        var timeInscrito = times[i];
                        if (timeInscrito != null) {
                            // CORRIGIDO: Construtor SolicitacaoTimeItemViewModel agora aceita Convite. (Resolve CS1503)
                            SolicitacoesPendentes.Add(new SolicitacaoTimeItemViewModel(convite, timeInscrito));
                        } else {
                            Debug.WriteLine($"Aviso: Time com o ClientAppId '{convite.TimeClientAppId}' não foi encontrado. Ignorando a solicitação.");
                        }
                    }
                    IsListEmpty = !SolicitacoesPendentes.Any();
                    Debug.WriteLine($"Adicionadas {SolicitacoesPendentes.Count} solicitações para exibição.");
                });

            } catch (Exception ex) {
                Debug.WriteLine($"Erro ao carregar solicitações: {ex.Message}");
                await _alertService.DisplayAlert("Erro", "Ocorreu um erro ao carregar as solicitações.", "OK");
            } finally {
                IsBusy = false;
            }
        }

        // B) Método AceitarSolicitacaoAsync
        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        public async Task AceitarSolicitacaoAsync(SolicitacaoTimeItemViewModel solicitacaoItem) {
            if (solicitacaoItem is null) return;

            IsBusy = true;
            Debug.WriteLine($"[AceitarSolicitacao] Iniciando aceitação para o Time ID: {solicitacaoItem.TimeSolicitante.ClientAppId}");

            try {
                // CORREÇÃO: SolicitacaoOriginal agora é do tipo Convite. Removido 'as Convite' e o null check de cast.
                var solicitacaoOriginal = solicitacaoItem.SolicitacaoOriginal;

                if (solicitacaoOriginal == null) {
                    Debug.WriteLine("[AceitarSolicitacao] Erro: SolicitacaoOriginal é nula.");
                    await _alertService.DisplayAlert("Erro", "Objeto de solicitação inválido.", "OK");
                    return;
                }

                // 1. Atualizar status da solicitação no banco
                solicitacaoOriginal.Status = StatusConvite.Aceito;
                solicitacaoOriginal.IsSynced = false;
                solicitacaoOriginal.UpdatedAt = DateTime.UtcNow;

                // Usar o método AtualizarConviteAsync
                await _databaseService.AtualizarConviteAsync(solicitacaoOriginal);
                Debug.WriteLine("[AceitarSolicitacao] Status do Convite atualizado para Aceito.");

                // *************** AÇÃO DO PASSO C DEVE ENTRAR AQUI ***************
                // A INSCRIÇÃO REAL DO TIME NO CAMPEONATO ESTÁ FALTANDO (Próximo passo de correção)
                // *******************************************************************

                // 2. NOTIFICAÇÃO (manter)
                MessagingCenter.Send(this, "TimeAceito", solicitacaoItem.TimeSolicitante);
                Debug.WriteLine($"[AceitarSolicitacao] Time {solicitacaoItem.TimeSolicitante.Nome} aceito. Notificando Detail VM.");

                // 3. Atualizar a UI (manter)
                MainThread.BeginInvokeOnMainThread(() => {
                    SolicitacoesPendentes.Remove(solicitacaoItem);
                    IsListEmpty = !SolicitacoesPendentes.Any();
                });

                await _alertService.DisplayAlert("Sucesso", "Solicitação aceita com sucesso!", "OK");

            } catch (Exception ex) {
                Debug.WriteLine($"[AceitarSolicitacao] ERRO ao aceitar solicitação: {ex.Message}");
                await _alertService.DisplayAlert("Erro", "Ocorreu um erro ao aceitar a solicitação.", "OK");
            } finally {
                IsBusy = false;
                Debug.WriteLine("[AceitarSolicitacao] Finalizado.");
            }
        }

        // C) Método RecusarSolicitacaoAsync
        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        public async Task RecusarSolicitacaoAsync(SolicitacaoTimeItemViewModel solicitacaoItem) {
            if (solicitacaoItem is null) return;

            IsBusy = true;
            try {
                // CORREÇÃO: SolicitacaoOriginal agora é do tipo Convite. Removido 'as Convite'.
                var solicitacaoOriginal = solicitacaoItem.SolicitacaoOriginal;

                if (solicitacaoOriginal == null) return;

                // MUDANÇA: Usar StatusConvite.Recusado
                solicitacaoOriginal.Status = StatusConvite.Recusado;
                solicitacaoOriginal.IsSynced = false;
                solicitacaoOriginal.UpdatedAt = DateTime.UtcNow;

                // Usar o método AtualizarConviteAsync
                await _databaseService.AtualizarConviteAsync(solicitacaoOriginal);

                // Remova o item da coleção para atualizar a UI
                MainThread.BeginInvokeOnMainThread(() => {
                    SolicitacoesPendentes.Remove(solicitacaoItem);
                    IsListEmpty = !SolicitacoesPendentes.Any();
                });

                await _alertService.DisplayAlert("Sucesso", "Solicitação recusada com sucesso!", "OK");

            } catch (Exception ex) {
                Debug.WriteLine($"Erro ao recusar solicitação: {ex.Message}");
                await _alertService.DisplayAlert("Erro", "Ocorreu um erro ao recusar a solicitação.", "OK");
            } finally {
                IsBusy = false;
            }
        }
    }
}