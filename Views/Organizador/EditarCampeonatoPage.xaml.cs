using ArenaVirtual.Models;
using System.Text.Json;

namespace ArenaVirtual.Views.Organizador;
public partial class EditarCampeonatoPage : ContentPage, IQueryAttributable {
    public Campeonato Campeonato { get; set; }

    public EditarCampeonatoPage() {
        InitializeComponent();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query) {
        if (query.TryGetValue("campeonato", out var campeonatoJson)) {
            Campeonato = JsonSerializer.Deserialize<Campeonato>(campeonatoJson as string);
            // Atualize o BindingContext ou os campos conforme necessário
            BindingContext = Campeonato;
        }
    }
}