namespace ArenaVirtual.Popups;

using ArenaVirtual.Models;
using ArenaVirtual.Services;
public partial class AtribuirArbitrosPopup : ContentPage {
    
    private readonly Campeonato _campeonato;
    private readonly Jogo _jogo;
    private readonly IAlertService _alertService;
    private readonly DatabaseService _databaseService;
    private readonly SyncService _syncService;
    public AtribuirArbitrosPopup(Campeonato campeonato, Jogo jogo,
                                 IAlertService alertService, DatabaseService databaseService,
                                 SyncService syncService) {
        InitializeComponent();
        _campeonato = campeonato;
        _jogo = jogo;
        _alertService = alertService;
        _databaseService = databaseService;
        _syncService = syncService;
    }

    private async void Cancelar_Clicked(object sender, EventArgs e) {
        await Application.Current.MainPage.Navigation.PopModalAsync();
    }

    private async void ConfirmarAtribuicao_Clicked(object sender, EventArgs e) {
        await Application.Current.MainPage.Navigation.PopModalAsync();
    }
}