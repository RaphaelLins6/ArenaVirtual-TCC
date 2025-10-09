using ArenaVirtual.Models;
using ArenaVirtual.Services;
using ArenaVirtual.ViewModels.Arbitro;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;

// NOTA: Certifique-se de que o IAlertService está em um namespace acessível (provavelmente ArenaVirtual.Services)
// E que o MainThread está sendo importado (provavelmente using Microsoft.Maui.Controls; ou usando Microsoft.Maui.ApplicationModel;)

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

        // COLEÇÃO PARA SOLICITAÇÕES DE TIME
        [ObservableProperty]
        private ObservableCollection<SolicitacaoTimeItemViewModel> solicitacoesPendentes;

        // COLEÇÃO PARA SOLICITAÇÕES DE ÁRBITRO
        [ObservableProperty]
        private ObservableCollection<SolicitacaoArbitroItemViewModel> solicitacoesArbitrosPendentes;

        // PROPRIEDADES DE CONTROLE DE VISIBILIDADE
        [ObservableProperty]
        private bool isListEmpty; // Visibilidade Geral

        [ObservableProperty]
        private bool isTimesListEmpty; // Visibilidade da seção Times

        [ObservableProperty]
        private bool isArbitrosListEmpty; // Visibilidade da seção Árbitros

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
            SolicitacoesArbitrosPendentes = new ObservableCollection<SolicitacaoArbitroItemViewModel>();
        }

        partial void OnCampeonatoChanged(Campeonato value) {
            if (value != null) {
                // Ao usar _ = LoadAllSolicitacoesAsync(), garantimos que o Command não aguarda o término
                // e permite a continuação imediata, mas não captura exceções, por isso o try/catch interno é crucial.
                _ = LoadAllSolicitacoesAsync();
            }
        }

        private async Task LoadAllSolicitacoesAsync() {
            IsBusy = true;
            try {
                await Task.WhenAll(
                    LoadSolicitacoesTimesInternalAsync(),
                    LoadSolicitacoesArbitrosInternalAsync()
                );
            } catch (Exception ex) {
                // Este bloco agora só pega erros que impediram o Task.WhenAll de iniciar ou erros muito críticos
                Debug.WriteLine($"Erro ao carregar todas as solicitações: {ex.Message}");
                await _alertService.DisplayAlert("Erro de Carregamento", "Ocorreu um erro ao carregar as solicitações.", "OK");
            } finally {
                // Chama as atualizações de visibilidade no final, garantindo que ocorra na MainThread.
                MainThread.BeginInvokeOnMainThread(() => {
                    UpdateTimesListVisibility();
                    UpdateArbitrosListVisibility();
                    UpdateGeneralListEmptyStatus();
                });
                IsBusy = false;
            }
        }

        // --- MÉTODOS DE CONTROLE DE VISIBILIDADE ---

        private void UpdateTimesListVisibility() {
            IsTimesListEmpty = !SolicitacoesPendentes.Any();
            UpdateGeneralListEmptyStatus();
        }

        private void UpdateArbitrosListVisibility() {
            IsArbitrosListEmpty = !SolicitacoesArbitrosPendentes.Any();
            UpdateGeneralListEmptyStatus();
        }

        private void UpdateGeneralListEmptyStatus() {
            IsListEmpty = IsTimesListEmpty && IsArbitrosListEmpty;
        }

        // --- MÉTODOS DE CARREGAMENTO ---

        public async Task LoadSolicitacoesTimesInternalAsync() {
            Debug.WriteLine("Iniciando LoadSolicitacoesTimesInternalAsync (Times)...");

            if (Campeonato is null) return;

            IEnumerable<Convite> convites = null;
            try {
                convites = await _databaseService.ObterConvitesPendentesAsync(Campeonato.ClientAppId, TipoConvite.InscricaoCampeonato);
                var convitesList = convites.ToList();
                var tarefas = convitesList.Select(c => _timeService.ObterPorClientAppIdAsync(c.TimeClientAppId)).ToList();
                var times = await Task.WhenAll(tarefas);

                MainThread.BeginInvokeOnMainThread(() => {
                    SolicitacoesPendentes.Clear();
                    for (int i = 0; i < convitesList.Count; i++) {
                        var convite = convitesList[i];
                        var timeInscrito = times[i];
                        if (timeInscrito != null) {
                            SolicitacoesPendentes.Add(new SolicitacaoTimeItemViewModel(convite, timeInscrito));
                        }
                    }
                });

            } catch (Exception ex) {
                Debug.WriteLine($"Erro ao carregar solicitações de Times: {ex.Message}");
                await _alertService.DisplayAlert("Erro", "Ocorreu um erro ao carregar as solicitações de times.", "OK");
            }
        }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        public async Task LoadSolicitacoesAsync() => await LoadAllSolicitacoesAsync();


        public async Task LoadSolicitacoesArbitrosInternalAsync() {
            Debug.WriteLine("Iniciando LoadSolicitacoesArbitrosInternalAsync...");

            if (Campeonato is null) return;

            IEnumerable<Convite> convites = null;
            try {
                convites = await _databaseService.ObterConvitesPendentesAsync(Campeonato.ClientAppId, TipoConvite.InscricaoArbitro);
                var convitesList = convites.ToList();

                var tarefas = convitesList
                    .Select(c => _usuarioService.ObterUsuarioPorClientAppIdAsync(c.UsuarioClientAppId))
                    .ToList();
                var arbitros = await Task.WhenAll(tarefas);

                MainThread.BeginInvokeOnMainThread(() => {
                    SolicitacoesArbitrosPendentes.Clear();

                    for (int i = 0; i < convitesList.Count; i++) {
                        var convite = convitesList[i];
                        var arbitroSolicitante = arbitros[i];
                        if (arbitroSolicitante != null) {
                            SolicitacoesArbitrosPendentes.Add(new SolicitacaoArbitroItemViewModel(convite, arbitroSolicitante));
                        }
                    }
                });

            } catch (Exception ex) {
                Debug.WriteLine($"Erro ao carregar solicitações de Árbitros: {ex.Message}");
                await _alertService.DisplayAlert("Erro", "Ocorreu um erro ao carregar as solicitações de árbitros.", "OK");
            }
        }

        // --- MÉTODOS DE AÇÃO (ÁRBITROS) ---

        // CORREÇÃO: Removido 'Command' do nome do método AceitarArbitro
        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        public async Task AceitarArbitro(SolicitacaoArbitroItemViewModel solicitacaoItem) {
            if (solicitacaoItem is null) return;

            IsBusy = true;
            try {
                var solicitacaoOriginal = solicitacaoItem.SolicitacaoOriginal;
                if (solicitacaoOriginal == null) return;

                solicitacaoOriginal.Status = StatusConvite.Aceito;
                solicitacaoOriginal.IsSynced = false;
                solicitacaoOriginal.UpdatedAt = DateTime.UtcNow;

                await _databaseService.AtualizarConviteAsync(solicitacaoOriginal);

                // TODO: Inserir árbitro no campeonato (Adicione a lógica de inserção aqui)

                MainThread.BeginInvokeOnMainThread(() => {
                    SolicitacoesArbitrosPendentes.Remove(solicitacaoItem);
                    UpdateArbitrosListVisibility();
                });

                await _alertService.DisplayAlert("Sucesso", $"O Árbitro {solicitacaoItem.NomeArbitro} foi aceito no campeonato!", "OK");

            } catch (Exception ex) {
                Debug.WriteLine($"[AceitarArbitro] ERRO: {ex.Message}");
                await _alertService.DisplayAlert("Erro", "Ocorreu um erro ao aceitar a solicitação do árbitro.", "OK");
            } finally {
                IsBusy = false;
            }
        }

        // CORREÇÃO: Removido 'Command' do nome do método RecusarArbitro
        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        public async Task RecusarArbitro(SolicitacaoArbitroItemViewModel solicitacaoItem) {
            if (solicitacaoItem is null) return;

            IsBusy = true;
            try {
                var solicitacaoOriginal = solicitacaoItem.SolicitacaoOriginal;
                if (solicitacaoOriginal == null) return;

                solicitacaoOriginal.Status = StatusConvite.Recusado;
                solicitacaoOriginal.IsSynced = false;
                solicitacaoOriginal.UpdatedAt = DateTime.UtcNow;

                await _databaseService.AtualizarConviteAsync(solicitacaoOriginal);

                MainThread.BeginInvokeOnMainThread(() => {
                    SolicitacoesArbitrosPendentes.Remove(solicitacaoItem);
                    UpdateArbitrosListVisibility();
                });

                await _alertService.DisplayAlert("Sucesso", $"Solicitação do árbitro {solicitacaoItem.NomeArbitro} recusada com sucesso!", "OK");

            } catch (Exception ex) {
                Debug.WriteLine($"Erro ao recusar solicitação de árbitro: {ex.Message}");
                await _alertService.DisplayAlert("Erro", "Ocorreu um erro ao recusar a solicitação do árbitro.", "OK");
            } finally {
                IsBusy = false;
            }
        }

        // --- MÉTODOS DE AÇÃO (TIMES) ---

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        public async Task AceitarTime(SolicitacaoTimeItemViewModel solicitacaoItem) {
            if (solicitacaoItem is null) return;

            IsBusy = true;
            try {
                var solicitacaoOriginal = solicitacaoItem.SolicitacaoOriginal;

                if (solicitacaoOriginal == null) return;

                // Atualiza o status do Convite/Solicitação
                solicitacaoOriginal.Status = StatusConvite.Aceito;
                solicitacaoOriginal.IsSynced = false;
                solicitacaoOriginal.UpdatedAt = DateTime.UtcNow;

                await _databaseService.AtualizarConviteAsync(solicitacaoOriginal);

                // -----------------------------------------------------------------
                // CORREÇÃO DOS ERROS DE COMPILAÇÃO E LÓGICA
                // -----------------------------------------------------------------

                // 1. Busca o objeto Time pelo ClientAppId do convite.
                // CORREÇÃO: Utiliza o nome do método que existe no seu TimeService: ObterPorClientAppIdAsync
                var timeAceito = await _timeService.ObterPorClientAppIdAsync(solicitacaoOriginal.TimeClientAppId);

                if (timeAceito != null) {
                    // 2. Vincula o Time ao Campeonato
                    // Nota: 'Campeonato' deve ser uma propriedade acessível no seu ViewModel.
                    timeAceito.CampeonatoId = Campeonato.Id;
                    timeAceito.IsSynced = false;
                    timeAceito.UpdatedAt = DateTime.UtcNow;

                    // 3. Atualiza o Time no DB.
                    // CORREÇÃO: Utiliza o nome do método que existe no seu TimeService: AtualizarTimeAsync
                    await _timeService.AtualizarTimeAsync(timeAceito);

                    // 4. REGENERA A TABELA DE JOGOS.
                    // Nota: Depende da implementação dos novos métodos no CampeonatoService e DatabaseService.
                    await _campeonatoService.RecalcularEGerarJogosAsync(Campeonato.ClientAppId);
                }
                // -----------------------------------------------------------------

                MainThread.BeginInvokeOnMainThread(() => {
                    SolicitacoesPendentes.Remove(solicitacaoItem);
                    UpdateTimesListVisibility();
                });

                await _alertService.DisplayAlert("Sucesso", $"Solicitação do time {solicitacaoItem.NomeTime} aceita com sucesso!", "OK");

            } catch (Exception ex) {
                Debug.WriteLine($"[AceitarSolicitacao] ERRO: {ex.Message}");
                await _alertService.DisplayAlert("Erro", "Ocorreu um erro ao aceitar a solicitação.", "OK");
            } finally {
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        public async Task RecusarTime(SolicitacaoTimeItemViewModel solicitacaoItem) {
            if (solicitacaoItem is null) return;

            IsBusy = true;
            try {
                var solicitacaoOriginal = solicitacaoItem.SolicitacaoOriginal;
                if (solicitacaoOriginal == null) return;

                solicitacaoOriginal.Status = StatusConvite.Recusado;
                solicitacaoOriginal.IsSynced = false;
                solicitacaoOriginal.UpdatedAt = DateTime.UtcNow;

                await _databaseService.AtualizarConviteAsync(solicitacaoOriginal);

                MainThread.BeginInvokeOnMainThread(() => {
                    SolicitacoesPendentes.Remove(solicitacaoItem);
                    UpdateTimesListVisibility();
                });

                await _alertService.DisplayAlert("Sucesso", $"Solicitação do time {solicitacaoItem.NomeTime} recusada com sucesso!", "OK");

            } catch (Exception ex) {
                Debug.WriteLine($"Erro ao recusar solicitação: {ex.Message}");
                await _alertService.DisplayAlert("Erro", "Ocorreu um erro ao recusar a solicitação.", "OK");
            } finally {
                IsBusy = false;
            }
        }
    }
}