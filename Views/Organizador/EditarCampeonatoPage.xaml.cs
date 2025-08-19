using ArenaVirtual.Models;
using ArenaVirtual.Services;
using ArenaVirtual.ViewModels.Organizador;
using System.Text.Json;
using System.Diagnostics;

namespace ArenaVirtual.Views.Organizador;

public partial class EditarCampeonatoPage : ContentPage, IQueryAttributable {
    // Declare apenas o CampeonatoService, pois ele já encapsula a lógica
    // de banco de dados e sincronização.
    private readonly CampeonatoService _campeonatoService;

    // O construtor da Page agora recebe apenas o CampeonatoService
    public EditarCampeonatoPage(CampeonatoService campeonatoService) {
        InitializeComponent();
        _campeonatoService = campeonatoService;
        Debug.WriteLine("[EditarCampeonatoPage] Construtor chamado com serviço injetado.");
    }

    public Campeonato? Campeonato { get; set; }

    public void ApplyQueryAttributes(IDictionary<string, object> query) {
        Debug.WriteLine("[EditarCampeonatoPage] ApplyQueryAttributes chamado.");
        if (query.TryGetValue("campeonato", out var campeonatoJson)) {
            var json = Uri.UnescapeDataString(campeonatoJson as string);
            Campeonato = JsonSerializer.Deserialize<Campeonato>(json);

            if (Campeonato != null) {
                BindingContext = new EditarCampeonatoViewModel(_campeonatoService, Campeonato);
                Debug.WriteLine($"[EditarCampeonatoPage] BindingContext atribuído para Campeonato ID: {Campeonato.Id}");
            } else {
                Debug.WriteLine("[EditarCampeonatoPage] Erro: Campeonato é nulo após deserialização.");
            }
        } else {
            Debug.WriteLine("[EditarCampeonatoPage] Erro: Parâmetro 'campeonato' não encontrado na query.");
        }
    }
}
