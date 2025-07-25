using ArenaVirtual.Models;
using ArenaVirtual.Services;
using ArenaVirtual.ViewModels.Organizador;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace ArenaVirtual.Views.Organizador;
public partial class EditarCampeonatoPage : ContentPage, IQueryAttributable
{
    public EditarCampeonatoPage()
    {
        InitializeComponent();
    }

    public Campeonato? Campeonato { get; set; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("campeonato", out var campeonatoJson))
        {
            var json = Uri.UnescapeDataString(campeonatoJson as string);
            Campeonato = JsonSerializer.Deserialize<Campeonato>(json);

            var databaseService = App.Current?.Handler?.MauiContext?.Services?.GetRequiredService<DatabaseService>();
            BindingContext = new EditarCampeonatoViewModel(databaseService, Campeonato);
        }
    }
}