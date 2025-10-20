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
using Microsoft.Maui.ApplicationModel; // Importação essencial para MainThread
using ArenaVirtual.ViewModels.Patrocinio;

namespace ArenaVirtual.ViewModels.CampeonatoPage {
    [QueryProperty(nameof(Campeonato), "Campeonato")]
    public partial class GerenciarSolicitacoesViewModel : ObservableObject {

        private readonly DatabaseService _databaseService;
        private readonly CampeonatoService _campeonatoService;
        private readonly SessaoService _sessaoService;
        private readonly TimeService _timeService;
        private readonly IAlertService _alertService;
        private readonly UsuarioService _usuarioService;
        private readonly PatrocinioService _patrocinioService;

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

        // COLEÇÃO PARA SOLICITAÇÕES DE PATROCÍNIO
        [ObservableProperty]
        private ObservableCollection<SolicitacaoPatrocinioItemViewModel> solicitacoesPatrocinioPendentes;

        // PROPRIEDADES DE CONTROLE DE VISIBILIDADE
        [ObservableProperty]
        private bool isListEmpty;

        [ObservableProperty]
        private bool isTimesListEmpty;

        [ObservableProperty]
        private bool isArbitrosListEmpty;

        [ObservableProperty]
        private bool isPatrociniosListEmpty;

        public GerenciarSolicitacoesViewModel(
            DatabaseService databaseService,
            CampeonatoService campeonatoService,
            SessaoService sessaoService,
            TimeService timeService,
            IAlertService alertService,
            UsuarioService usuarioService,
            PatrocinioService patrocinioService) {
            _databaseService = databaseService;
            _campeonatoService = campeonatoService;
            _sessaoService = sessaoService;
            _timeService = timeService;
            _alertService = alertService;
            _usuarioService = usuarioService;
            _patrocinioService = patrocinioService;

            SolicitacoesPendentes = new ObservableCollection<SolicitacaoTimeItemViewModel>();
            SolicitacoesArbitrosPendentes = new ObservableCollection<SolicitacaoArbitroItemViewModel>();
            SolicitacoesPatrocinioPendentes = new ObservableCollection<SolicitacaoPatrocinioItemViewModel>();
        }

        partial void OnCampeonatoChanged(Campeonato value) {
            if (value != null) {
                _ = LoadAllSolicitacoesAsync();
            }
        }

        private async Task LoadAllSolicitacoesAsync() {
            IsBusy = true;
            try {
                await Task.WhenAll(
                    LoadSolicitacoesTimesInternalAsync(),
                    LoadSolicitacoesArbitrosInternalAsync(),
                    LoadSolicitacoesPatrocinioInternalAsync()
                );
            } catch (Exception ex) {
                Debug.WriteLine($"Erro ao carregar todas as solicitações: {ex.Message}");
                await _alertService.DisplayAlert("Erro de Carregamento", "Ocorreu um erro ao carregar as solicitações.", "OK");
            } finally {
                // Chama as atualizações de visibilidade no final, garantindo que ocorra na MainThread.
                MainThread.BeginInvokeOnMainThread(() => {
                    UpdateTimesListVisibility();
                    UpdateArbitrosListVisibility();
                    UpdatePatrociniosListVisibility();
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

        private void UpdatePatrociniosListVisibility() {
            IsPatrociniosListEmpty = !SolicitacoesPatrocinioPendentes.Any();
            UpdateGeneralListEmptyStatus();
        }

        private void UpdateGeneralListEmptyStatus() {
            // ATUALIZADO para incluir a nova lista
            IsListEmpty = IsTimesListEmpty && IsArbitrosListEmpty && IsPatrociniosListEmpty;
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

        public async Task LoadSolicitacoesPatrocinioInternalAsync() {
            Debug.WriteLine("Iniciando LoadSolicitacoesPatrocinioInternalAsync...");

            if (Campeonato is null) return;

            IEnumerable<PropostaPatrocinio> propostas = null;
            try {
                propostas = await _patrocinioService.ObterPropostasPendentesPorCampeonatoAsync(Campeonato.ClientAppId);
                var propostasList = propostas.ToList();

                // >>> LINHA DE DEBUG 1: CONFIRMA QUANTAS PROPOSTAS FORAM ENCONTRADAS
                Debug.WriteLine($"[DEBUG-PATROCINIO] {propostasList.Count} propostas pendentes encontradas.");

                // 1. Coleta o Patrocinador (Usuario) de cada proposta
                var tarefasPatrocinador = propostasList
                    .Select(p => _usuarioService.ObterUsuarioPorIdAsync(p.PatrocinadorId))
                    .ToList();
                var patrocinadores = await Task.WhenAll(tarefasPatrocinador);

                // >>> LINHA DE DEBUG 2: CONFIRMA O STATUS DE CARREGAMENTO DO PRIMEIRO PATROCINADOR
                if (patrocinadores.Any()) {
                    Debug.WriteLine($"[DEBUG-PATROCINIO] ID do Patrocinador na Proposta 1: {propostasList.First().PatrocinadorId}");
                    Debug.WriteLine($"[DEBUG-PATROCINIO] Patrocinador 1 Carregado: {patrocinadores.First() != null}");
                }

                MainThread.BeginInvokeOnMainThread(() => {
                    SolicitacoesPatrocinioPendentes.Clear();

                    for (int i = 0; i < propostasList.Count; i++) {
                        var proposta = propostasList[i];
                        var patrocinador = patrocinadores[i];
                        if (patrocinador != null) {
                            SolicitacoesPatrocinioPendentes.Add(new SolicitacaoPatrocinioItemViewModel(proposta, patrocinador));
                        }
                    }
                    Debug.WriteLine($"[DEBUG-PATROCINIO] CONTAGEM FINAL NA THREAD PRINCIPAL: {SolicitacoesPatrocinioPendentes.Count}");
                    UpdatePatrociniosListVisibility();
                });

            } catch (Exception ex) {
                Debug.WriteLine($"[ERRO FATAL] Erro ao carregar propostas de Patrocínio: {ex.Message}");
                await _alertService.DisplayAlert("Erro", "Ocorreu um erro ao carregar as propostas de patrocínio.", "OK");
            }
        }

        // --- MÉTODOS DE AÇÃO (ÁRBITROS) ---

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

                // 1. Busca o objeto Time pelo ClientAppId do convite.
                var timeAceito = await _timeService.ObterPorClientAppIdAsync(solicitacaoOriginal.TimeClientAppId);

                if (timeAceito != null) {
                    // 2. Vincula o Time ao Campeonato
                    timeAceito.CampeonatoId = Campeonato.Id;
                    timeAceito.IsSynced = false;
                    timeAceito.UpdatedAt = DateTime.UtcNow;

                    // 3. Atualiza o Time no DB.
                    await _timeService.AtualizarTimeAsync(timeAceito);

                    // 4. REGENERA A TABELA DE JOGOS.
                    await _campeonatoService.RecalcularEGerarJogosAsync(Campeonato.ClientAppId);
                }

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

        // --- MÉTODOS DE AÇÃO (PATROCÍNIO) ---

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        public async Task AceitarPatrocinio(SolicitacaoPatrocinioItemViewModel solicitacaoItem) {
            if (solicitacaoItem is null) return;

            IsBusy = true;
            try {
                var propostaOriginal = solicitacaoItem.PropostaOriginal;
                if (propostaOriginal == null) return;

                propostaOriginal.Aprovada = true; // Define como Aprovada!
                propostaOriginal.IsSynced = false;
                propostaOriginal.UpdatedAt = DateTime.UtcNow;

                // 1. Atualiza a Proposta no DB (agora Aprovada)
                await _patrocinioService.AtualizarPropostaAsync(propostaOriginal);

                // --- Variáveis temporárias para a correção da lógica de data ---
                var dataInicioProposta = propostaOriginal.DataInicio;
                var dataFimProposta = propostaOriginal.DataFim;

                // ⭐️ CORREÇÕES APLICADAS AQUI ⭐️
                var novaCampanha = new CampanhaPatrocinio {
                    PatrocinadorId = propostaOriginal.PatrocinadorId,
                    CampeonatoId = propostaOriginal.CampeonatoId,

                    // 🎯 LINHA ESSENCIAL ADICIONADA: Transferir o valor monetário da Proposta para a Campanha
                    // Assumindo que o campo na Campanha é 'ValorProposta' e na Proposta é 'ValorMonetario'.
                    ValorProposta = propostaOriginal.ValorMonetario,

                    Nome = $"Patrocínio Ativo - {solicitacaoItem.NomePatrocinador}",
                    Inicio = dataInicioProposta == DateTime.MinValue ? DateTime.Now.Date : dataInicioProposta.Date,
                    Fim = dataFimProposta == DateTime.MinValue ? DateTime.Now.AddMonths(1).Date : dataFimProposta.Date,
                };

                // 3. Insere a Campanha no DB.
                // Chamada via PatrocinioService.
                await _patrocinioService.InserirCampanhaAsync(novaCampanha);

                Debug.WriteLine($"[Aceite] Campanha {novaCampanha.Nome} criada e inserida para Patrocinador ID {novaCampanha.PatrocinadorId}.");
                // Fim do bloco de correção ⭐️

                MainThread.BeginInvokeOnMainThread(() => {
                    SolicitacoesPatrocinioPendentes.Remove(solicitacaoItem);
                    UpdatePatrociniosListVisibility();
                });

                await _alertService.DisplayAlert("Sucesso", $"Proposta do Patrocinador {solicitacaoItem.NomePatrocinador} aceita com sucesso!", "OK");

            } catch (Exception ex) {
                Debug.WriteLine($"[AceitarPatrocinio] ERRO: {ex.Message}");
                await _alertService.DisplayAlert("Erro", "Ocorreu um erro ao aceitar a proposta de patrocínio.", "OK");
            } finally {
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        public async Task RecusarPatrocinio(SolicitacaoPatrocinioItemViewModel solicitacaoItem) {
            if (solicitacaoItem is null) return;

            IsBusy = true;
            try {
                var propostaOriginal = solicitacaoItem.PropostaOriginal;
                if (propostaOriginal == null) return;

                // 1. Remove a proposta do DB (recusar = deletar, para limpar a lista pendente)
                await _patrocinioService.DeletarPropostaAsync(propostaOriginal);

                MainThread.BeginInvokeOnMainThread(() => {
                    SolicitacoesPatrocinioPendentes.Remove(solicitacaoItem);
                    UpdatePatrociniosListVisibility();
                });

                await _alertService.DisplayAlert("Sucesso", $"Proposta do Patrocinador {solicitacaoItem.NomePatrocinador} recusada com sucesso!", "OK");

            } catch (Exception ex) {
                Debug.WriteLine($"Erro ao recusar proposta de patrocínio: {ex.Message}");
                await _alertService.DisplayAlert("Erro", "Ocorreu um erro ao recusar a proposta de patrocínio.", "OK");
            } finally {
                IsBusy = false;
            }
        }
    }
}