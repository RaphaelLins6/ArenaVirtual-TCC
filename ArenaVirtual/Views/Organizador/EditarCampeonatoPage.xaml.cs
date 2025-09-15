using ArenaVirtual.Models;
using ArenaVirtual.Services;
using ArenaVirtual.ViewModels.Organizador;
using System.Text.Json;
using System.Diagnostics;

namespace ArenaVirtual.Views.Organizador;

public partial class EditarCampeonatoPage : ContentPage, IQueryAttributable {

    private readonly CampeonatoService _campeonatoService;
    private readonly SessaoService _sessaoService;
    private readonly SyncService _syncService; // Adicionando o SyncService

    public Campeonato? Campeonato { get; set; } // Propriedade para armazenar o objeto

    // O construtor agora recebe o SyncService via injeção de dependência
    public EditarCampeonatoPage(CampeonatoService campeonatoService, SessaoService sessaoService, SyncService syncService) {
        InitializeComponent();
        _campeonatoService = campeonatoService;
        _sessaoService = sessaoService;
        _syncService = syncService; // Atribuindo o serviço
        Debug.WriteLine("[EditarCampeonatoPage] Construtor chamado com serviços injetados.");
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query) {
        Debug.WriteLine("[EditarCampeonatoPage] ApplyQueryAttributes chamado.");

        if (query.TryGetValue("campeonato", out var campeonatoJson)) {
            try {
                var json = Uri.UnescapeDataString(campeonatoJson as string);

                // Atribui o valor à propriedade da classe, não a uma variável local
                Campeonato = JsonSerializer.Deserialize<Campeonato>(json);

                if (Campeonato != null) {
                    // Passa o SyncService para o ViewModel
                    BindingContext = new EditarCampeonatoViewModel(_campeonatoService, _sessaoService, _syncService, Campeonato);
                    Debug.WriteLine($"[EditarCampeonatoPage] BindingContext atribuído para Campeonato ID: {Campeonato.Id}");
                } else {
                    Debug.WriteLine("[EditarCampeonatoPage] Erro: Campeonato é nulo após desserialização.");
                }
            } catch (JsonException ex) {
                Debug.WriteLine($"[EditarCampeonatoPage] Erro ao desserializar JSON: {ex.Message}");
            }
        } else {
            Debug.WriteLine("[EditarCampeonatoPage] Erro: Parâmetro 'campeonato' não encontrado na query.");
        }
    }
}