using ArenaVirtual.Models;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ArenaVirtual.ViewModels.Organizador;
    public partial class EditarCampeonatoViewModel : ObservableObject {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        private Campeonato campeonato;

        public IRelayCommand SalvarCommand { get; }

        public EditarCampeonatoViewModel(DatabaseService databaseService, Campeonato campeonato) {
            _databaseService = databaseService;
            Campeonato = campeonato;
            SalvarCommand = new RelayCommand(async () => await SalvarAsync());
        }

        private async Task SalvarAsync() {
            await _databaseService.AtualizarCampeonatoAsync(Campeonato);
            await Shell.Current.GoToAsync("..");
        }
    }
