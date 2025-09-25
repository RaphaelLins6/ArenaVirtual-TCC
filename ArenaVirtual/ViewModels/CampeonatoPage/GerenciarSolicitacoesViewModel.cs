using ArenaVirtual.Models;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Maui.Controls;

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
                _ = LoadSolicitacoesAsync();
            }
        }

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
                convites = await _databaseService.ObterSolicitacoesPendentesPorCampeonatoAsync(Campeonato.ClientAppId);

                Debug.WriteLine($"Encontrados {convites?.Count() ?? 0} convites pendentes.");

                var tarefas = convites.Select(c => _timeService.ObterPorClientAppIdAsync(c.TimeClientAppId)).ToList();
                var times = await Task.WhenAll(tarefas);

                MainThread.BeginInvokeOnMainThread(() => {
                    SolicitacoesPendentes.Clear();
                    for (int i = 0; i < convites.Count(); i++) {
                        var convite = convites.ElementAt(i);
                        var timeInscrito = times[i];
                        if (timeInscrito != null) {
                            SolicitacoesPendentes.Add(new SolicitacaoTimeItemViewModel(convite, timeInscrito));
                        } else {
                            Debug.WriteLine($"Aviso: Time com o ClientAppId '{convite.TimeClientAppId}' não foi encontrado. Ignorando o convite.");
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

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        public async Task AceitarSolicitacaoAsync(SolicitacaoTimeItemViewModel solicitacaoItem) {
            IsBusy = true;
            try {
                solicitacaoItem.ConviteOriginal.Status = StatusConvite.Aceito;
                solicitacaoItem.ConviteOriginal.IsSynced = false;
                solicitacaoItem.ConviteOriginal.UpdatedAt = DateTime.UtcNow;
                await _databaseService.AtualizarConviteAsync(solicitacaoItem.ConviteOriginal);

                // Remova o item da coleção para atualizar a UI
                SolicitacoesPendentes.Remove(solicitacaoItem);

                // Atualiza a visibilidade do texto "Nenhuma solicitação pendente."
                IsListEmpty = !SolicitacoesPendentes.Any();

                await _alertService.DisplayAlert("Sucesso", "Solicitação aceita com sucesso!", "OK");

            } catch (Exception ex) {
                Debug.WriteLine($"Erro ao aceitar solicitação: {ex.Message}");
                await _alertService.DisplayAlert("Erro", "Ocorreu um erro ao aceitar a solicitação.", "OK");
            } finally {
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        public async Task RecusarSolicitacaoAsync(SolicitacaoTimeItemViewModel solicitacaoItem) {
            IsBusy = true;
            try {
                solicitacaoItem.ConviteOriginal.Status = StatusConvite.Recusado;
                solicitacaoItem.ConviteOriginal.IsSynced = false;
                solicitacaoItem.ConviteOriginal.UpdatedAt = DateTime.UtcNow;
                await _databaseService.AtualizarConviteAsync(solicitacaoItem.ConviteOriginal);

                // Remova o item da coleção para atualizar a UI
                SolicitacoesPendentes.Remove(solicitacaoItem);

                // Atualiza a visibilidade do texto "Nenhuma solicitação pendente."
                IsListEmpty = !SolicitacoesPendentes.Any();

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