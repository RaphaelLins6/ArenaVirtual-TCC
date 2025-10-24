using ArenaVirtual.Models;
using ArenaVirtual.Services;
using ArenaVirtual.ViewModels.Organizador;
using System.Text.Json;
using System.Diagnostics;

namespace ArenaVirtual.Views.Organizador;

public partial class EditarCampeonatoPage : ContentPage, IQueryAttributable {

    private readonly CampeonatoService _campeonatoService;
    private readonly SessaoService _sessaoService;
    private readonly SyncService _syncService; 

    public Campeonato? Campeonato { get; set; } 

    public EditarCampeonatoPage(CampeonatoService campeonatoService, SessaoService sessaoService, SyncService syncService) {
        InitializeComponent();
        _campeonatoService = campeonatoService;
        _sessaoService = sessaoService;
        _syncService = syncService; 
        //Debug.WriteLine("[EditarCampeonatoPage] Construtor chamado com serviços injetados.");
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query) {
        //Debug.WriteLine("[EditarCampeonatoPage] ApplyQueryAttributes chamado.");

        if (query.TryGetValue("campeonato", out var campeonatoJson)) {
            try {
                var json = Uri.UnescapeDataString(campeonatoJson as string);

                Campeonato = JsonSerializer.Deserialize<Campeonato>(json);

                if (Campeonato != null) {
                    BindingContext = new EditarCampeonatoViewModel(_campeonatoService, _sessaoService, _syncService, Campeonato);
                    //Debug.WriteLine($"[EditarCampeonatoPage] BindingContext atribuído para Campeonato ID: {Campeonato.Id}");
                } else {
                    //Debug.WriteLine("[EditarCampeonatoPage] Erro: Campeonato é nulo após desserialização.");
                }
            } catch (JsonException ex) {
                //Debug.WriteLine($"[EditarCampeonatoPage] Erro ao desserializar JSON: {ex.Message}");
            }
        } else {
            //Debug.WriteLine("[EditarCampeonatoPage] Erro: Parâmetro 'campeonato' não encontrado na query.");
        }
    }
}