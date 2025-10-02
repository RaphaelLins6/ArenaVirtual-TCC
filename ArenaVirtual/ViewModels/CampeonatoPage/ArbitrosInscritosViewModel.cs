using ArenaVirtual.Models;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics; // Fundamental para o Debug.WriteLine
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using ArenaVirtual.ViewModels.Arbitro;

namespace ArenaVirtual.ViewModels.CampeonatoPage {

    public class ArbitrosInscritosViewModel : ObservableObject, IQueryAttributable {

        private readonly CampeonatoService _campeonatoService;
        private readonly IAlertService _alertService;
        private readonly UsuarioService _usuarioService;
        private readonly DatabaseService _databaseService;

        private bool _isBusy;
        public bool IsBusy {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }
        private bool IsNotBusy => !IsBusy;

        private Guid _campeonatoClientAppId;

        private ObservableCollection<SolicitacaoArbitroItemViewModel> _arbitrosInscritos;
        public ObservableCollection<SolicitacaoArbitroItemViewModel> ArbitrosInscritos {
            get => _arbitrosInscritos;
            set => SetProperty(ref _arbitrosInscritos, value);
        }

        private bool _isListEmpty;
        public bool IsListEmpty {
            get => _isListEmpty;
            set => SetProperty(ref _isListEmpty, value);
        }

        public IAsyncRelayCommand LoadArbitrosCommand { get; }

        public ArbitrosInscritosViewModel(
            DatabaseService databaseService,
            CampeonatoService campeonatoService,
            IAlertService alertService,
            UsuarioService usuarioService) {
            _databaseService = databaseService;
            _campeonatoService = campeonatoService;
            _alertService = alertService;
            _usuarioService = usuarioService;

            ArbitrosInscritos = new ObservableCollection<SolicitacaoArbitroItemViewModel>();
            LoadArbitrosCommand = new AsyncRelayCommand(LoadArbitrosAsync, () => IsNotBusy);
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query) {
            Debug.WriteLine($"[Arbitros VM] ApplyQueryAttributes chamado. Keys: {string.Join(", ", query.Keys)}");

            if (query.ContainsKey("CampeonatoClientAppId")) {
                // Tenta obter o GUID diretamente
                if (query["CampeonatoClientAppId"] is Guid guidId) {
                    _campeonatoClientAppId = guidId;
                    Debug.WriteLine($"[Arbitros VM] ID Recebido (Guid): {_campeonatoClientAppId}");
                } else if (query["CampeonatoClientAppId"] is Campeonato campeonato) {
                    // Caso tenha sido passado o objeto Campeonato inteiro
                    _campeonatoClientAppId = campeonato.ClientAppId;
                    // 🐛 CORREÇÃO: Trocado _campeamentoClientAppId por _campeonatoClientAppId
                    Debug.WriteLine($"[Arbitros VM] ID Recebido (Campeonato Object): {_campeonatoClientAppId}");
                }

                // Garante que o carregamento aconteça na thread principal
                MainThread.BeginInvokeOnMainThread(async () => {
                    Debug.WriteLine("[Arbitros VM] Chamando LoadArbitrosAsync via ApplyQueryAttributes.");
                    await LoadArbitrosAsync();
                });

            } else {
                Debug.WriteLine("[Arbitros VM] ERRO: Nenhuma chave de campeonato (CampeonatoClientAppId) encontrada na navegação.");
            }
        }

        // NOVO MÉTODO DE DEBUG: Para obter uma string segura do Convite
        private string GetConviteDebugString(Convite convite) {
            if (convite == null) return "Convite Nulo";

            // Tenta obter o valor das propriedades mais prováveis
            var tipo = "Indefinido";
            var status = "Indefinido";

            // Use Reflection ou tente as propriedades mais prováveis para evitar erros de compilação
            // VOU ASSUMIR A PROPRIEDADE 'Tipo' E 'StatusInscricao' (se existir)
            try {
                // Se Tipo existir:
                tipo = (convite as dynamic).Tipo?.ToString() ?? "Tipo Nulo";
            } catch { /* Ignora se a propriedade não existir */ }

            try {
                // Se StatusInscricao existir:
                status = (convite as dynamic).StatusInscricao?.ToString() ?? "Status Nulo";
            } catch { /* Ignora se a propriedade não existir */ }


            return $"Tipo: {tipo} | Status: {status} | User ID: {convite.UsuarioClientAppId}";
        }

        public async Task LoadArbitrosAsync() {
            if (_campeonatoClientAppId == Guid.Empty || IsBusy) return;

            IsBusy = true;
            ArbitrosInscritos.Clear();
            IsListEmpty = false;

            try {
                // 1. Obter todos os convites ACEITOS para o campeonato
                var convitesAceitos = await _databaseService
                     .ObterConvitesAceitosPorCampeonatoAsync(_campeonatoClientAppId);

                Debug.WriteLine($"\n--- INÍCIO DEBUG ÁRBITROS INSCRITOS ---");
                Debug.WriteLine($"[Arbitros VM] Convites aceitos brutos retornados: {convitesAceitos?.Count() ?? 0}");

                if (convitesAceitos == null || !convitesAceitos.Any()) {
                    UpdateIsListEmpty();
                    return;
                }

                // NOVO PASSO: Listar TODOS os convites para ver o valor da propriedade de filtro
                Debug.WriteLine($"[Arbitros VM] Detalhes dos Convites:");
                foreach (var convite in convitesAceitos) {
                    // *** ESTA LINHA IRÁ IMPRIMIR OS DADOS CRUCIAIS! ***
                    Debug.WriteLine($"[DEBUG CONVITE] {GetConviteDebugString(convite)}");
                }

                // 2. Filtra os convites localmente e obtém os IDs dos usuários (árbitros)
                var arbitroClientAppIds = convitesAceitos
                    // MANTENDO A ÚLTIMA CORREÇÃO (c.Tipo), mas se o debug mostrar que está errado, trocaremos.
                    .Where(c => c.Tipo == TipoConvite.InscricaoArbitro)
                    .Select(c => c.UsuarioClientAppId).ToList();

                Debug.WriteLine($"[Arbitros VM] Árbitros após filtro: {arbitroClientAppIds?.Count ?? 0}");

                if (arbitroClientAppIds.Any()) {

                    // 3. Busca detalhes dos Árbitros em paralelo
                    var tarefas = arbitroClientAppIds
                        .Select(id => _usuarioService.ObterUsuarioPorClientAppIdAsync(id))
                        .ToList();

                    var arbitros = await Task.WhenAll(tarefas);

                    // 4. Adicionar à coleção no MainThread
                    MainThread.BeginInvokeOnMainThread(() => {
                        foreach (var arbitro in arbitros.Where(a => a != null)) {
                            ArbitrosInscritos.Add(new SolicitacaoArbitroItemViewModel(arbitro));
                        }
                        UpdateIsListEmpty();
                        Debug.WriteLine($"[Arbitros VM] Total de itens na lista: {ArbitrosInscritos.Count}");
                    });
                } else {
                    UpdateIsListEmpty();
                }

            } catch (Exception ex) {
                Debug.WriteLine($"[Arbitros VM] ERRO FATAL ao carregar árbitros: {ex.Message}");
                await _alertService.DisplayAlert("Erro", "Ocorreu um erro ao carregar a lista de árbitros.", "OK");
            } finally {
                IsBusy = false;
                Debug.WriteLine($"--- FIM DEBUG ÁRBITROS INSCRITOS ---\n");
            }
        }

        private void UpdateIsListEmpty() {
            IsListEmpty = !ArbitrosInscritos.Any();
        }
    }
}