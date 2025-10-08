namespace ArenaVirtual.Popups;

using ArenaVirtual.Models;
using ArenaVirtual.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.Maui.Controls;
using System.Diagnostics;

public partial class AtribuirArbitrosPopup : ContentPage, INotifyPropertyChanged, IQueryAttributable {

    public event EventHandler<Usuario> ArbitroAnexado;

    public event PropertyChangedEventHandler PropertyChanged;

    // Campos privados
    private Campeonato _campeonato;
    private Jogo _jogo;

    // Serviços 
    private readonly IAlertService _alertService;
    private readonly DatabaseService _databaseService;
    private readonly SyncService _syncService;
    private readonly UsuarioService _usuarioService;

    // Propriedade para o jogo atual
    public Jogo JogoAtual {
        get => _jogo;
        private set {
            if (_jogo != value) {
                _jogo = value;
                OnPropertyChanged();
            }
        }
    }

    // Propriedades para Binding 
    public ObservableCollection<Usuario> ArbitrosDisponiveis { get; set; }

    private Usuario _arbitroSelecionado;
    public Usuario ArbitroSelecionado {
        get => _arbitroSelecionado;
        set {
            if (_arbitroSelecionado != value) {
                _arbitroSelecionado = value;
                OnPropertyChanged();
                Debug.WriteLine($"[DEBUG-SELEÇÃO] Arbitro Selecionado ATUALIZADO para: {value?.Nome ?? "NULL"}");
            }
        }
    }

    private bool _isBusy;
    public bool IsBusy {
        get => _isBusy;
        set {
            _isBusy = value;
            OnPropertyChanged();
        }
    }

    public AtribuirArbitrosPopup(IAlertService alertService, DatabaseService databaseService,
                                SyncService syncService, UsuarioService usuarioService) {
        InitializeComponent();

        _alertService = alertService;
        _databaseService = databaseService;
        _syncService = syncService;
        _usuarioService = usuarioService;

        ArbitrosDisponiveis = new ObservableCollection<Usuario>();

        this.BindingContext = this;

    }

    public void ApplyQueryAttributes(IDictionary<string, object> query) {
        Debug.WriteLine("[AtribuirArbitrosPopup] ApplyQueryAttributes chamado.");

        if (query.TryGetValue("Campeonato", out object campObj) && campObj is Campeonato campeonato) {
            _campeonato = campeonato;
        }

        if (query.TryGetValue("Jogo", out object jogoObj) && jogoObj is Jogo jogo) {
            JogoAtual = jogo;
        }

        if (_campeonato != null && JogoAtual != null) {
            CarregarArbitrosAsync(_campeonato.ClientAppId);
        } else {
            Debug.WriteLine("[AtribuirArbitrosPopup] Erro: Campeonato ou Jogo é nulo após ApplyQueryAttributes.");
        }
    }
    // --------------------------------------------------------------------------

    // --- Lógica de Seleção via evento do RadioButton  ---
    private void RadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e) {
        if (e.Value) {
            if (sender is RadioButton rb && rb.BindingContext is Usuario usuario) {
                Debug.WriteLine($"[DEBUG-RADIO] RadioButton marcado para: {usuario.Nome}");

                MainThread.BeginInvokeOnMainThread(() => {
                    // Garante que o ArbitroSelecionado seja atualizado corretamente.
                    ArbitroSelecionado = usuario;
                });
            }
        }
    }
    // --------------------------------------------------------------------------

    // --- Lógica de Dados  ---

    // Método para carregar os árbitros inscritos 
    private async void CarregarArbitrosAsync(Guid campeonatoClientAppId) {
        MainThread.BeginInvokeOnMainThread(() => IsBusy = true);
        ArbitrosDisponiveis.Clear();

        try {
            var convitesAceitos = await _databaseService
                                         .ObterConvitesAceitosPorCampeonatoAsync(campeonatoClientAppId);

            if (convitesAceitos == null || !convitesAceitos.Any()) {
                return;
            }

            var arbitroClientAppIds = convitesAceitos
                                     .Where(c => c.Tipo == TipoConvite.InscricaoArbitro)
                                     .Select(c => c.UsuarioClientAppId).ToList();


            if (arbitroClientAppIds.Any()) {
                var tarefas = arbitroClientAppIds
                            .Select(id => _usuarioService.ObterUsuarioPorClientAppIdAsync(id))
                            .ToList();

                var arbitros = await Task.WhenAll(tarefas);

                MainThread.BeginInvokeOnMainThread(() => {
                    foreach (var arbitro in arbitros.Where(a => a != null)) {
                        ArbitrosDisponiveis.Add(arbitro);
                    }

                    // PRÉ-SELEÇÃO
                    if (JogoAtual.ArbitroId.HasValue && JogoAtual.ArbitroId.Value != Guid.Empty) {

                        var arbitroAtual = ArbitrosDisponiveis.FirstOrDefault(a => a.ClientAppId == JogoAtual.ArbitroId.Value);

                        if (arbitroAtual != null) {
                            ArbitroSelecionado = arbitroAtual;
                            Debug.WriteLine($"[DEBUG-PRESELECAO] Árbitro '{arbitroAtual.Nome}' pré-selecionado no Pop-up.");
                        }
                    }
                });
            }

        } catch (Exception ex) {
            Debug.WriteLine($"ERRO ao carregar árbitros: {ex.Message}");
            await _alertService.DisplayAlert("Erro", "Ocorreu um erro ao carregar a lista de árbitros.", "OK");
        } finally {
            MainThread.BeginInvokeOnMainThread(() => IsBusy = false);
        }
    }

    // --- Métodos de Botão ---
    private async void Cancelar_Clicked(object sender, EventArgs e) {
        Debug.WriteLine("[DEBUG-CANCEL] Cancelar clicado. Fechando via Shell GoToAsync('..').");
        await Shell.Current.GoToAsync("..");
    }

    private async void ConfirmarAtribuicao_Clicked(object sender, EventArgs e) {
        Debug.WriteLine($"[DEBUG-CONFIRM] CONFIRMAR CLICADO. ArbitroSelecionado FINAL: {ArbitroSelecionado?.Nome ?? "NULL"}");

        if (ArbitroSelecionado == null) {
            await _alertService.DisplayAlert("Atenção", "Selecione um árbitro antes de confirmar.", "OK");
            return;
        }

        try {
            Debug.WriteLine($"[A] POPUP (Antes Atrib.): Jogo.Id: {JogoAtual.Id} | ArbitroId anterior: {JogoAtual.ArbitroId} | IsSynced: {JogoAtual.IsSynced}");

            JogoAtual.ArbitroId = ArbitroSelecionado.ClientAppId;
            JogoAtual.NomeArbitro = ArbitroSelecionado.Nome;

            JogoAtual.UpdatedAt = DateTime.UtcNow;
            JogoAtual.IsSynced = false;
            JogoAtual.NotifyArbitroStatusChanged();

            Debug.WriteLine($"[B] POPUP (Antes Salvar): Jogo.Id: {JogoAtual.Id} | ArbitroId NOVO: {JogoAtual.ArbitroId} | IsSynced NOVO: {JogoAtual.IsSynced}");

            int resultadoUpdate = await _databaseService.SalvarJogoAsync(JogoAtual);
            Debug.WriteLine($"[C] POPUP (Após Salvar): Resultado Salvar (InsertOrReplace): {resultadoUpdate}.");

            await _alertService.DisplayAlert("Sucesso", $"Árbitro {ArbitroSelecionado.Nome} atribuído com sucesso.", "OK");

            await Shell.Current.GoToAsync("..", new Dictionary<string, object> {
                { "jogoAtualizado", JogoAtual }
            });

            Task.Run(async () => {
                Debug.WriteLine($"[D] POPUP (Sync): Iniciando SyncData em background.");
                await _syncService.SyncData();
                Debug.WriteLine($"[D] POPUP (Sync): SyncData finalizado.");
            });

        } catch (Exception ex) {
            Debug.WriteLine($"[!!! ERRO EXCEPTION !!!] ERRO ao atribuir árbitro: {ex.Message}");
            await _alertService.DisplayAlert("Erro", "Ocorreu um erro ao salvar a atribuição do árbitro.", "OK");
        }
    }

    // --- Implementação INotifyPropertyChanged (MANTIDA) ---
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}