namespace ArenaVirtual.Popups;

using ArenaVirtual.Models;
using ArenaVirtual.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

// 1. A classe precisa implementar INotifyPropertyChanged para que o Binding do XAML funcione
public partial class AtribuirArbitrosPopup : ContentPage, INotifyPropertyChanged {

    // 2. Implementação da interface
    public event PropertyChangedEventHandler PropertyChanged;

    // Campos privados originais (mantidos por segurança)
    private readonly Campeonato _campeonato;
    private readonly Jogo _jogo;
    private readonly IAlertService _alertService;
    private readonly DatabaseService _databaseService;
    private readonly SyncService _syncService;

    // 3. Propriedades públicas para o Data Binding no XAML

    // A propriedade JogoAtual será usada para exibir TimeA e TimeB
    public Jogo JogoAtual { get; private set; }

    // A lista precisa ser ObservableCollection para atualizar automaticamente o CollectionView
    public ObservableCollection<Usuario> ArbitrosDisponiveis { get; set; }

    // Propriedade para controlar o item selecionado na CollectionView
    public Usuario ArbitroSelecionado { get; set; } // Ajuste o tipo se for diferente de Usuario

    // Propriedade para controlar o ActivityIndicator (IsBusy)
    private bool _isBusy;
    public bool IsBusy {
        get => _isBusy;
        set {
            _isBusy = value;
            OnPropertyChanged(); // Notifica a UI sobre a mudança
        }
    }

    public AtribuirArbitrosPopup(Campeonato campeonato, Jogo jogo,
                                 IAlertService alertService, DatabaseService databaseService,
                                 SyncService syncService) {
        InitializeComponent();

        // Atribuições originais (mantidas por segurança)
        _campeonato = campeonato;
        _jogo = jogo;
        _alertService = alertService;
        _databaseService = databaseService;
        _syncService = syncService;

        // ----------------------------------------------
        // CORREÇÃO: Inicializa os dados e define o BindingContext para 'this'
        // ----------------------------------------------
        JogoAtual = jogo;
        ArbitrosDisponiveis = new ObservableCollection<Usuario>();

        // O próprio Code-Behind é o contexto de ligação
        this.BindingContext = this;

        // Inicia o carregamento dos árbitros
        CarregarArbitrosAsync(campeonato.Id);
    }

    // Método para carregar os árbitros (substitua pela sua lógica real)
    private async void CarregarArbitrosAsync(int campeonatoId) {
        IsBusy = true;

        // --- COLAR SUA LÓGICA DE ACESSO AO BANCO DE DADOS AQUI ---
        // Exemplo: var arbitros = await _databaseService.GetArbitrosParaCampeonato(campeonatoId);

        // Simulação de carregamento de dados (remova isto na sua implementação final)
        await Task.Delay(1000); // Simula um atraso de rede/banco
        var arbitros = new List<Usuario>
        {
            new Usuario { Nome = "João Árbitro", Localizacao = "Quadra Central" },
            new Usuario { Nome = "Maria Juíza", Localizacao = "Ginásio Lateral" },
            new Usuario { Nome = "Pedro Bandeirinha", Localizacao = "Campo B" }
        };

        foreach (var a in arbitros) {
            ArbitrosDisponiveis.Add(a);
        }
        // ---------------------------------------------------------

        IsBusy = false;
    }

    private async void Cancelar_Clicked(object sender, EventArgs e) {
        await Application.Current.MainPage.Navigation.PopModalAsync();
    }

    private async void ConfirmarAtribuicao_Clicked(object sender, EventArgs e) {
        if (ArbitroSelecionado == null) {
            // Exemplo de uso do serviço injetado
            await _alertService.DisplayAlert("Atenção", "Selecione um árbitro antes de confirmar.", "OK");
            return;
        }

        // Lógica de atribuição do árbitro ao jogo
        // _jogo.ArbitroId = ArbitroSelecionado.Id;
        // await _databaseService.SaveJogo(_jogo);
        // await _syncService.SyncData();

        await _alertService.DisplayAlert("Sucesso", $"Árbitro {ArbitroSelecionado.Nome} atribuído com sucesso.", "OK");
        await Application.Current.MainPage.Navigation.PopModalAsync();
    }

    // Método auxiliar para notificar a mudança de propriedade
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}