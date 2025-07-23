using ArenaVirtual.Models;
using ArenaVirtual.Services;
using MvvmHelpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

namespace ArenaVirtual.ViewModels {
    public partial class HomeViewModel : BaseViewModel, INotifyPropertyChanged {
        private readonly ObservableCollection<Campeonato> _campeonatos;
        public ObservableCollection<Campeonato> Campeonatos { get; set; }

        private ObservableCollection<Campeonato> _favoritos = [];
        public ObservableCollection<Campeonato> Favoritos {
            get => _favoritos;
            set {
                _favoritos = value;
                OnPropertyChanged(nameof(Favoritos));
            }
        }

        public ICommand FavoritarCommand { get; }
        public ICommand ParticiparCommand { get; }
        public ICommand VerCampeonatoCommand { get; }
        private readonly DatabaseService _databaseService;

        public HomeViewModel(DatabaseService databaseService) {
            _campeonatos = [];
            Campeonatos = _campeonatos;
            _databaseService = databaseService;

            FavoritarCommand = new Command<object>(
                async obj => {
                    if (obj is Campeonato campeonato)
                        await FavoritarAsync(campeonato);
                });
            
            ParticiparCommand = new Command<Campeonato>(async (campeonato) => {
                await ParticiparAsync(campeonato);
            });

            VerCampeonatoCommand = new Command<Campeonato>(async (campeonato) => {
                await VerCampeonatoAsync(campeonato);
            });

            Task.Run(async () => {
                await _databaseService.InitializeAsync();
                await CarregarCampeonatos();
            });
        }

        public async Task CarregarCampeonatos() {
            if (IsBusy) return;

            IsBusy = true;
            try {
                var todos = await _databaseService.ListarCampeonatosAsync() ?? [];

                // Agrupa por ID para evitar duplicação real
                var unicos = todos
                    .GroupBy(c => c.Id)
                    .Select(g => g.First())
                    .ToList();

                Favoritos = new ObservableCollection<Campeonato>(unicos.Where(c => c.EhFavorito));

                _campeonatos.Clear();
                foreach (var c in unicos.Where(c => !c.EhFavorito)) {
                    _campeonatos.Add(c);
                }

                OnPropertyChanged(nameof(Campeonatos));
                OnPropertyChanged(nameof(Favoritos));
            } catch (Exception ex) {
                Debug.WriteLine($"Erro ao carregar campeonatos: {ex.Message}");
            } finally {
                IsBusy = false;
            }
        }

        private async Task FavoritarAsync(Campeonato campeonato) {
            Debug.WriteLine($"[HomeViewModel] FavoritarAsync chamado. Campeonato: {campeonato?.Nome ?? "NULO"}, ID: {campeonato?.Id ?? 0}");
            if (campeonato == null) return;
            Debug.WriteLine($"[DEBUG] FavoritarAsync acionado para: {campeonato.Nome ?? "N/A"}, ID: {campeonato.Id}");
            campeonato.EhFavorito = !campeonato.EhFavorito;

            await _databaseService.AtualizarCampeonatoAsync(campeonato);

            await CarregarCampeonatos();
        }

        private static async Task ParticiparAsync(Campeonato campeonato) {
            Debug.WriteLine($"[HomeViewModel] ParticiparAsync chamado. Campeonato: {campeonato?.Nome ?? "NULO"}, ID: {campeonato?.Id ?? 0}");
            if (campeonato == null) {
                Debug.WriteLine("[DEBUG] ParticiparCommand acionado, mas campeonato é nulo.");
                return;
            }
            Debug.WriteLine($"[DEBUG] ParticiparCommand acionado para: {campeonato.Nome ?? "N/A"}, ID: {campeonato.Id}");
            await Shell.Current.DisplayAlert("Participar", $"Você clicou em Participar do campeonato: {campeonato.Nome}", "OK");
        }

        private static async Task VerCampeonatoAsync(Campeonato campeonato) {
            Debug.WriteLine($"[HomeViewModel] VerCampeonatoAsync chamado. Campeonato: {campeonato?.Nome ?? "NULO"}, ID: {campeonato?.Id ?? 0}");
            if (campeonato == null) {
                Debug.WriteLine("[DEBUG] VerCampeonatoCommand acionado, mas campeonato é nulo.");
                return;
            }
            Debug.WriteLine($"[DEBUG] VerCampeonatoCommand acionado para: {campeonato.Nome ?? "N/A"}, ID: {campeonato.Id}");
            await Shell.Current.Navigation.PushAsync(new Views.CampeonatoPage.CampeonatoDetailPage(campeonato));
        }
    }
}
