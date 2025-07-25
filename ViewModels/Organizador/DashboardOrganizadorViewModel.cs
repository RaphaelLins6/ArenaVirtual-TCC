using ArenaVirtual.Models;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Text.Json;
using ArenaVirtual.Views.Organizador;

namespace ArenaVirtual.ViewModels.Organizador {
    public partial class DashboardOrganizadorViewModel : ObservableObject {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        private ObservableCollection<Campeonato> campeonatos = [];

        public DashboardOrganizadorViewModel(DatabaseService databaseService) {
            _databaseService = databaseService;
            CarregarCampeonatosCommand = new AsyncRelayCommand(CarregarCampeonatos);
            AdicionarCampeonatoCommand = new AsyncRelayCommand<Campeonato>(AdicionarCampeonatoAsync);
            EditarCampeonatoCommand = new AsyncRelayCommand<Campeonato>(NavegarParaEditarCampeonatoAsync);
            RemoverCampeonatoCommand = new AsyncRelayCommand<Campeonato>(RemoverCampeonatoAsync);
            _ = CarregarCampeonatos();
        }

        public IAsyncRelayCommand CarregarCampeonatosCommand { get; }
        public IAsyncRelayCommand<Campeonato> AdicionarCampeonatoCommand { get; }
        public IAsyncRelayCommand<Campeonato> EditarCampeonatoCommand { get; }
        public IAsyncRelayCommand<Campeonato> RemoverCampeonatoCommand { get; }

        private async Task CarregarCampeonatos() {
            var lista = await _databaseService.ListarCampeonatosAsync();
            if (App.CurrentUser != null)
                lista = [.. lista.Where(c => c.OrganizadorId == App.CurrentUser.Id)];
            Campeonatos = new ObservableCollection<Campeonato>(lista);
        }

        private async Task AdicionarCampeonatoAsync(Campeonato campeonato) {
            if (App.CurrentUser != null && App.CurrentUser.Perfil == TipoPerfil.Organizador)
                campeonato.OrganizadorId = App.CurrentUser.Id;
            await _databaseService.InserirCampeonatoAsync(campeonato);
            await CarregarCampeonatos();
        }

        private async Task NavegarParaEditarCampeonatoAsync(Campeonato campeonato) {
            // Serializa o objeto para passar como parâmetro
            var campeonatoJson = JsonSerializer.Serialize(campeonato);
            await Shell.Current.GoToAsync($"{nameof(EditarCampeonatoPage)}?campeonato={Uri.EscapeDataString(campeonatoJson)}");
        }

        private async Task RemoverCampeonatoAsync(Campeonato campeonato) {
            await _databaseService.DeletarCampeonatoAsync(campeonato);
            await CarregarCampeonatos();
        }
    }
}