// EM: ArenaVirtual.ViewModels.CampeonatoPage/GerenciarSolicitacoesViewModel.cs

using ArenaVirtual.Models;
using ArenaVirtual.Services;
using ArenaVirtual.ViewModels.Arbitro; // 1. IMPORTADO O NAMESPACE PARA O ARBITRO VM
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System;
using System.Linq; // Necessário para o método Any() do UpdateIsListEmpty
using System.Collections.Generic; // Necessário para IEnumerable

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

        // Propriedade auxiliar para comandos, garante que IsBusy seja refletido
        private bool IsNotBusy => !IsBusy;

        [ObservableProperty]
        private Campeonato campeonato;

        // COLEÇÃO PARA SOLICITAÇÕES DE TIME
        [ObservableProperty]
        private ObservableCollection<SolicitacaoTimeItemViewModel> solicitacoesPendentes;

        // 2. COLEÇÃO PARA SOLICITAÇÕES DE ÁRBITRO
        [ObservableProperty]
        private ObservableCollection<SolicitacaoArbitroItemViewModel> solicitacoesArbitrosPendentes;

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

            // 3. INICIALIZAÇÃO DAS DUAS COLEÇÕES
            SolicitacoesPendentes = new ObservableCollection<SolicitacaoTimeItemViewModel>();
            SolicitacoesArbitrosPendentes = new ObservableCollection<SolicitacaoArbitroItemViewModel>();
        }

        partial void OnCampeonatoChanged(Campeonato value) {
            if (value != null) {
                // Dispara o carregamento dos dados das duas coleções em paralelo (sem await)
                _ = LoadAllSolicitacoesAsync();
            }
        }

        private async Task LoadAllSolicitacoesAsync() {
            IsBusy = true;
            try {
                // Executa os dois carregamentos em paralelo
                await Task.WhenAll(
                    LoadSolicitacoesTimesInternalAsync(),
                    LoadSolicitacoesArbitrosInternalAsync()
                );
            } catch (Exception ex) {
                // Trata exceções do carregamento (já tratadas internamente, mas bom ter um fallback)
                Debug.WriteLine($"Erro ao carregar todas as solicitações: {ex.Message}");
                await _alertService.DisplayAlert("Erro de Carregamento", "Ocorreu um erro ao carregar as solicitações.", "OK");
            } finally {
                IsBusy = false;
                UpdateIsListEmpty(); // Atualiza após ambos carregarem
            }
        }

        private void UpdateIsListEmpty() {
            // 6. LÓGICA UNIFICADA: Lista vazia se ambas as coleções estiverem vazias
            IsListEmpty = !SolicitacoesPendentes.Any() && !SolicitacoesArbitrosPendentes.Any();
        }

        // A) Método LoadSolicitacoesTimesInternalAsync (para TIMES) - Versão interna
        public async Task LoadSolicitacoesTimesInternalAsync() {
            Debug.WriteLine("Iniciando LoadSolicitacoesTimesInternalAsync (Times)...");

            if (Campeonato is null) return;

            IEnumerable<Convite> convites = null;
            try {
                // Carrega convites de times
                convites = await _databaseService.ObterConvitesPendentesAsync(Campeonato.ClientAppId, TipoConvite.InscricaoCampeonato);

                Debug.WriteLine($"Encontradas {convites?.Count() ?? 0} solicitações de times pendentes.");

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
                        } else {
                            Debug.WriteLine($"Aviso: Time com o ClientAppId '{convite.TimeClientAppId}' não foi encontrado. Ignorando a solicitação.");
                        }
                    }
                });

            } catch (Exception ex) {
                Debug.WriteLine($"Erro ao carregar solicitações de Times: {ex.Message}");
                // Notificação de erro local (sem IsBusy = false aqui, pois o LoadAll fará isso)
                await _alertService.DisplayAlert("Erro", "Ocorreu um erro ao carregar as solicitações de times.", "OK");
            }
        }

        // A') Método público para o comando de Refresh
        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        public async Task LoadSolicitacoesAsync() => await LoadAllSolicitacoesAsync();


        // 4. Método LoadSolicitacoesArbitrosInternalAsync (para ÁRBITROS) - Versão interna
        public async Task LoadSolicitacoesArbitrosInternalAsync() {
            Debug.WriteLine("Iniciando LoadSolicitacoesArbitrosInternalAsync...");

            if (Campeonato is null) return;

            IEnumerable<Convite> convites = null;
            try {
                // Filtra por convites de Árbitro
                convites = await _databaseService.ObterConvitesPendentesAsync(Campeonato.ClientAppId, TipoConvite.InscricaoArbitro);

                Debug.WriteLine($"Encontradas {convites?.Count() ?? 0} solicitações de árbitros pendentes.");

                var convitesList = convites.ToList();

                // CORREÇÃO: Puxando o árbitro pelo UsuarioClientAppId do Convite
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
                        } else {
                            Debug.WriteLine($"Aviso: Árbitro com o UsuarioClientAppId '{convite.UsuarioClientAppId}' não foi encontrado. Ignorando a solicitação.");
                        }
                    }
                });

            } catch (Exception ex) {
                Debug.WriteLine($"Erro ao carregar solicitações de Árbitros: {ex.Message}");
                // Notificação de erro local (sem IsBusy = false aqui, pois o LoadAll fará isso)
                await _alertService.DisplayAlert("Erro", "Ocorreu um erro ao carregar as solicitações de árbitros.", "OK");
            }
        }


        // 5. Método AceitarArbitroAsync
        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        public async Task AceitarArbitroAsync(SolicitacaoArbitroItemViewModel solicitacaoItem) {
            if (solicitacaoItem is null) return;

            IsBusy = true;
            try {
                var solicitacaoOriginal = solicitacaoItem.SolicitacaoOriginal;

                if (solicitacaoOriginal == null) {
                    await _alertService.DisplayAlert("Erro", "Objeto de solicitação inválido.", "OK");
                    return;
                }

                // 1. Atualizar status da solicitação
                solicitacaoOriginal.Status = StatusConvite.Aceito;
                solicitacaoOriginal.IsSynced = false;
                solicitacaoOriginal.UpdatedAt = DateTime.UtcNow;

                await _databaseService.AtualizarConviteAsync(solicitacaoOriginal);

                // TODO: Chamar aqui o método que insere o árbitro no campeonato
                // Exemplo: await _campeonatoService.AdicionarArbitroAsync(Campeonato.ClientAppId, solicitacaoItem.ArbitroSolicitante.ClientAppId);

                // 2. Atualizar a UI
                MainThread.BeginInvokeOnMainThread(() => {
                    SolicitacoesArbitrosPendentes.Remove(solicitacaoItem);
                    UpdateIsListEmpty();
                });

                await _alertService.DisplayAlert("Sucesso", $"O Árbitro {solicitacaoItem.NomeArbitro} foi aceito no campeonato!", "OK");

            } catch (Exception ex) {
                Debug.WriteLine($"[AceitarArbitro] ERRO ao aceitar solicitação: {ex.Message}");
                await _alertService.DisplayAlert("Erro", "Ocorreu um erro ao aceitar a solicitação do árbitro.", "OK");
            } finally {
                IsBusy = false;
            }
        }

        // 5. Método RecusarArbitroAsync
        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        public async Task RecusarArbitroAsync(SolicitacaoArbitroItemViewModel solicitacaoItem) {
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
                    UpdateIsListEmpty();
                });

                await _alertService.DisplayAlert("Sucesso", $"Solicitação do árbitro {solicitacaoItem.NomeArbitro} recusada com sucesso!", "OK");

            } catch (Exception ex) {
                Debug.WriteLine($"Erro ao recusar solicitação de árbitro: {ex.Message}");
                await _alertService.DisplayAlert("Erro", "Ocorreu um erro ao recusar a solicitação do árbitro.", "OK");
            } finally {
                IsBusy = false;
            }
        }

        // B) Método AceitarSolicitacaoAsync (para TIMES)
        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        public async Task AceitarSolicitacaoAsync(SolicitacaoTimeItemViewModel solicitacaoItem) {
            if (solicitacaoItem is null) return;

            IsBusy = true;
            Debug.WriteLine($"[AceitarSolicitacao] Iniciando aceitação para o Time ID: {solicitacaoItem.TimeSolicitante.ClientAppId}");

            try {
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

                await _databaseService.AtualizarConviteAsync(solicitacaoOriginal);
                Debug.WriteLine("[AceitarSolicitacao] Status do Convite atualizado para Aceito.");

                // TODO: Chamar aqui o método que insere o time no campeonato
                // Exemplo: await _campeonatoService.InscreverTimeAsync(Campeonato.ClientAppId, solicitacaoItem.TimeSolicitante.ClientAppId);

                // 2. NOTIFICAÇÃO (Se estiver usando Xamarin/MAUI)
                // Usar Injeção de Dependência para o MessagingCenter é melhor, mas mantendo a estrutura:
                // MessagingCenter.Send(this, "TimeAceito", solicitacaoItem.TimeSolicitante); 

                // 3. Atualizar a UI
                MainThread.BeginInvokeOnMainThread(() => {
                    SolicitacoesPendentes.Remove(solicitacaoItem);
                    UpdateIsListEmpty();
                });

                await _alertService.DisplayAlert("Sucesso", $"Solicitação do time {solicitacaoItem.NomeTime} aceita com sucesso!", "OK");

            } catch (Exception ex) {
                Debug.WriteLine($"[AceitarSolicitacao] ERRO ao aceitar solicitação: {ex.Message}");
                await _alertService.DisplayAlert("Erro", "Ocorreu um erro ao aceitar a solicitação.", "OK");
            } finally {
                IsBusy = false;
                Debug.WriteLine("[AceitarSolicitacao] Finalizado.");
            }
        }

        // C) Método RecusarSolicitacaoAsync (para TIMES)
        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        public async Task RecusarSolicitacaoAsync(SolicitacaoTimeItemViewModel solicitacaoItem) {
            if (solicitacaoItem is null) return;

            IsBusy = true;
            try {
                var solicitacaoOriginal = solicitacaoItem.SolicitacaoOriginal;

                if (solicitacaoOriginal == null) return;

                solicitacaoOriginal.Status = StatusConvite.Recusado;
                solicitacaoOriginal.IsSynced = false;
                solicitacaoOriginal.UpdatedAt = DateTime.UtcNow;

                await _databaseService.AtualizarConviteAsync(solicitacaoOriginal);

                // Remova o item da coleção para atualizar a UI
                MainThread.BeginInvokeOnMainThread(() => {
                    SolicitacoesPendentes.Remove(solicitacaoItem);
                    UpdateIsListEmpty();
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