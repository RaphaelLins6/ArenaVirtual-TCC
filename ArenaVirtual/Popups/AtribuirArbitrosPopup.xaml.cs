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

public partial class AtribuirArbitrosPopup : ContentPage, INotifyPropertyChanged {

    public event EventHandler<Usuario> ArbitroAnexado;

    public event PropertyChangedEventHandler PropertyChanged;

    // Campos privados
    private readonly Campeonato _campeonato;
    private readonly Jogo _jogo;
    private readonly IAlertService _alertService;
    private readonly DatabaseService _databaseService;
    private readonly SyncService _syncService;
    private readonly UsuarioService _usuarioService;

    // Propriedade para o jogo atual (usada no construtor e binding)
    public Jogo JogoAtual { get; private set; }

    // Propriedades para Binding
    public ObservableCollection<Usuario> ArbitrosDisponiveis { get; set; }

    private Usuario _arbitroSelecionado;
    public Usuario ArbitroSelecionado {
        get => _arbitroSelecionado;
        set {
            if (_arbitroSelecionado != value) {
                _arbitroSelecionado = value;
                OnPropertyChanged();
                // Log de debug para verificar quando a propriedade é setada
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

    // Construtor com injeção de dependência
    public AtribuirArbitrosPopup(Campeonato campeonato, Jogo jogo,
                IAlertService alertService, DatabaseService databaseService,
                SyncService syncService,
                UsuarioService usuarioService) {
        InitializeComponent();

        _campeonato = campeonato;
        _jogo = jogo;
        _alertService = alertService;
        _databaseService = databaseService;
        _syncService = syncService;
        _usuarioService = usuarioService;

        JogoAtual = jogo;
        ArbitrosDisponiveis = new ObservableCollection<Usuario>();

        this.BindingContext = this;

        // Inicia o carregamento dos árbitros
        CarregarArbitrosAsync(_campeonato.ClientAppId);
    }

    // --- Lógica de Seleção via evento do RadioButton ---
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

    // --- Lógica de Dados ---

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

                    // =======================================================
                    // PRÉ-SELEÇÃO: Usa HasValue e Value (Correto para Guid?)
                    // =======================================================
                    if (JogoAtual.ArbitroId.HasValue && JogoAtual.ArbitroId.Value != Guid.Empty) {

                        // Compara JogoAtual.ArbitroId.Value (Guid) com ArbitrosDisponiveis.ClientAppId (Guid)
                        var arbitroAtual = ArbitrosDisponiveis.FirstOrDefault(a => a.ClientAppId == JogoAtual.ArbitroId.Value);

                        if (arbitroAtual != null) {
                            ArbitroSelecionado = arbitroAtual;
                            Debug.WriteLine($"[DEBUG-PRESELECAO] Árbitro '{arbitroAtual.Nome}' pré-selecionado no Pop-up.");
                        }
                    }
                    // =======================================================
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
        await Navigation.PopModalAsync();
    }

    private async void ConfirmarAtribuicao_Clicked(object sender, EventArgs e) {
        Debug.WriteLine($"[DEBUG-CONFIRM] CONFIRMAR CLICADO. ArbitroSelecionado FINAL: {ArbitroSelecionado?.Nome ?? "NULL"}");

        if (ArbitroSelecionado == null) {
            await _alertService.DisplayAlert("Atenção", "Selecione um árbitro antes de confirmar.", "OK");
            return;
        }

        try {
            // --- PONTO DE DEBUG A: VALORES ANTES DE ATRIBUIR ---
            Debug.WriteLine($"[A] POPUP (Antes Atrib.): Jogo.Id: {_jogo.Id} | ArbitroId anterior: {_jogo.ArbitroId} | IsSynced: {_jogo.IsSynced}");

            // 1. ATRIBUIÇÃO DOS DADOS NO MODELO
            _jogo.ArbitroId = ArbitroSelecionado.ClientAppId;
            _jogo.NomeArbitro = ArbitroSelecionado.Nome;

            // 2. MARCAÇÃO PARA SINCRONIZAÇÃO E ATUALIZAÇÃO DA UI
            _jogo.UpdatedAt = DateTime.UtcNow;
            _jogo.IsSynced = false;
            _jogo.NotifyArbitroStatusChanged(); // Notifica as mudanças (texto do botão/nome)

            // --- PONTO DE DEBUG B: VALORES ANTES DE CHAMAR O SALVAMENTO ---
            Debug.WriteLine($"[B] POPUP (Antes Salvar): Jogo.Id: {_jogo.Id} | ArbitroId NOVO: {_jogo.ArbitroId} | IsSynced NOVO: {_jogo.IsSynced}");

            // 3. PERSISTÊNCIA LOCAL
            int resultadoUpdate = await _databaseService.SalvarJogoAsync(_jogo);

            // --- PONTO DE DEBUG C: RESULTADO DO SALVAMENTO LOCAL ---
            Debug.WriteLine($"[C] POPUP (Após Salvar): Resultado Salvar (InsertOrReplace): {resultadoUpdate}.");

            // 4. Feedback Visual (ANTES DE FECHAR A POP-UP)
            await _alertService.DisplayAlert("Sucesso", $"Árbitro {ArbitroSelecionado.Nome} atribuído com sucesso.", "OK");

            // 5. Fechamento da Pop-up (Usando GoToAsync com parâmetro)
            // Isso notifica o CampeonatoDetailViewModel para recarregar a tabela.
            await Shell.Current.GoToAsync("..", new Dictionary<string, object> {
                { "jogoAtualizado", true }
            });

            // 6. SINCRONIZAÇÃO REMOTA (Disparado em segundo plano)
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

    // --- Implementação INotifyPropertyChanged ---

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}