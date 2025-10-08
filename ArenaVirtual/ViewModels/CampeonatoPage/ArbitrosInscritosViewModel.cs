using ArenaVirtual.Models;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ArenaVirtual.ViewModels.Arbitro;

namespace ArenaVirtual.ViewModels.CampeonatoPage {

    public partial class ArbitrosInscritosViewModel : ObservableObject, IQueryAttributable {

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

        public async void ApplyQueryAttributes(IDictionary<string, object> query) {
            Debug.WriteLine($"[Arbitros VM] ApplyQueryAttributes chamado. Keys: {string.Join(", ", query.Keys)}");

            const string CampeonatoIdKey = "CampeonatoId";

            if (query.TryGetValue(CampeonatoIdKey, out object value)) {

                Debug.WriteLine($"[Arbitros VM] Valor recebido é do Tipo: {value?.GetType().FullName ?? "NULO"}");

                Guid campeonatoId = Guid.Empty;

                if (value is Guid guidId) {
                    campeonatoId = guidId;
                    Debug.WriteLine($"[Arbitros VM] Conversão SUCESSO (Guid direto).");
                } else if (value is string stringId && Guid.TryParse(stringId, out Guid parsedId)) {
                    campeonatoId = parsedId;
                    Debug.WriteLine($"[Arbitros VM] Conversão SUCESSO (String parseada).");
                } else if (value is Campeonato campeonato) {
                    campeonatoId = campeonato.ClientAppId;
                    Debug.WriteLine($"[Arbitros VM] Conversão SUCESSO (Objeto Campeonato).");
                }
                
                _campeonatoClientAppId = campeonatoId;

                if (_campeonatoClientAppId != Guid.Empty) {
                    Debug.WriteLine($"[Arbitros VM] ID Final: {_campeonatoClientAppId}. Chamando LoadArbitrosAsync.");
                    await LoadArbitrosAsync();
                } else {
                    Debug.WriteLine("[Arbitros VM] ERRO: ID do Campeonato não pôde ser atribuída/parseada. Checar tipo logado acima.");
                }

            } else {
                Debug.WriteLine($"[Arbitros VM] ERRO: Nenhuma chave de campeonato ({CampeonatoIdKey}) encontrada na navegação.");
            }
        }

        private string GetConviteDebugString(Convite convite) {
            if (convite == null) return "Convite Nulo";

            var tipo = "Indefinido";
            var status = "Indefinido";

            try {
                tipo = (convite as dynamic).Tipo?.ToString() ?? "Tipo Nulo";
            } catch { }

            try {
                status = (convite as dynamic).StatusInscricao?.ToString() ?? "Status Nulo";
            } catch { }


            return $"Tipo: {tipo} | Status: {status} | User ID: {convite.UsuarioClientAppId}";
        }

        public async Task LoadArbitrosAsync() {
            Debug.WriteLine($"[Arbitros VM] LoadArbitrosAsync INICIADO. ID: {_campeonatoClientAppId}, Busy: {IsBusy}");

            if (_campeonatoClientAppId == Guid.Empty || IsBusy) return;

            IsBusy = true;
            ArbitrosInscritos.Clear();
            IsListEmpty = false;

            try {
                var convitesAceitos = await _databaseService
                     .ObterConvitesAceitosPorCampeonatoAsync(_campeonatoClientAppId);

                Debug.WriteLine($"\n--- INÍCIO DEBUG ÁRBITROS INSCRITOS ---");
                Debug.WriteLine($"[Arbitros VM] Convites aceitos brutos retornados: {convitesAceitos?.Count() ?? 0}");

                if (convitesAceitos == null || !convitesAceitos.Any()) {
                    UpdateIsListEmpty();
                    return;
                }

                Debug.WriteLine($"[Arbitros VM] Detalhes dos Convites:");
                foreach (var convite in convitesAceitos) {
                    Debug.WriteLine($"[DEBUG CONVITE] {GetConviteDebugString(convite)} | Time ID: {convite.TimeClientAppId}");
                }

                var arbitroClientAppIds = convitesAceitos
                    .Where(c => c.TimeClientAppId == Guid.Empty) 
                    .Select(c => c.UsuarioClientAppId).ToList();

                Debug.WriteLine($"[Arbitros VM] Árbitros após filtro (Sem Time): {arbitroClientAppIds?.Count ?? 0}");

                if (arbitroClientAppIds.Any()) {

                    var tarefas = arbitroClientAppIds
                        .Select(id => _usuarioService.ObterUsuarioPorClientAppIdAsync(id))
                        .ToList();

                    var arbitros = await Task.WhenAll(tarefas);

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