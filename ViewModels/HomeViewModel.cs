using ArenaVirtual.Models;
using ArenaVirtual.Services;
using MvvmHelpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Diagnostics;

namespace ArenaVirtual.ViewModels {
    public partial class HomeViewModel : BaseViewModel, INotifyPropertyChanged {
        private readonly ObservableCollection<Campeonato> _campeonatos;
        public ObservableCollection<Campeonato> Campeonatos { get; set; }
        public ICommand FavoritarCommand { get; }
        private readonly CampeonatoService _campeonatoService;
        private readonly DatabaseService _databaseService;

        public HomeViewModel(DatabaseService databaseService) {
            _campeonatos = [];
            Campeonatos = _campeonatos;
            _databaseService = databaseService;

            FavoritarCommand = new Command<object>(
                async obj => {
                    if (obj is Campeonato campeonato)
                        await FavoritarAsync(campeonato, databaseService);
                });

            _campeonatoService = new CampeonatoService(databaseService);

            // Chame CarregarCampeonatos() se desejar carregamento automático
            // _ = CarregarCampeonatos();
        }

        public async Task CarregarCampeonatos() {
            IsBusy = true;
            try {
                var campeonatos = await _databaseService.ListarCampeonatosAsync() ?? [];
                _campeonatos.Clear();
                foreach (var c in campeonatos) {
                    _campeonatos.Add(c);
                }
                OnPropertyChanged(nameof(Campeonatos));
            } catch (Exception ex) {
                Debug.WriteLine($"Erro ao carregar campeonatos: {ex.Message}");
            } finally {
                IsBusy = false;
            }
        }

        private static async Task FavoritarAsync(Campeonato campeonato, DatabaseService databaseService) {
            if (campeonato == null) return;
            campeonato.EhFavorito = true;
            await databaseService.AtualizarCampeonatoAsync(campeonato);
        }
    }
}