using ArenaVirtual.Models;
using ArenaVirtual.Services;
using MvvmHelpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

namespace ArenaVirtual.ViewModels
{
    public partial class HomeViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private readonly ObservableCollection<Campeonato> _campeonatos;
        public ObservableCollection<Campeonato> Campeonatos { get; set; }

        private ObservableCollection<Campeonato> _favoritos = [];
        public ObservableCollection<Campeonato> Favoritos
        {
            get => _favoritos;
            set
            {
                _favoritos = value;
                OnPropertyChanged(nameof(Favoritos));
            }
        }

        public ICommand FavoritarCommand { get; }
        private readonly CampeonatoService _campeonatoService;
        private readonly DatabaseService _databaseService;

        public HomeViewModel(DatabaseService databaseService)
        {
            _campeonatos = [];
            Campeonatos = _campeonatos;
            _databaseService = databaseService;

            FavoritarCommand = new Command<object>(
                async obj =>
                {
                    if (obj is Campeonato campeonato)
                        await FavoritarAsync(campeonato);
                });

            _campeonatoService = new CampeonatoService(databaseService);

            Task.Run(async () =>
            {
                await _databaseService.InitializeAsync();
                await CarregarCampeonatos();
            });
        }

        public async Task CarregarCampeonatos()
        {
            IsBusy = true;
            try
            {
                var campeonatos = await _databaseService.ListarCampeonatosAsync() ?? [];
                _campeonatos.Clear();
                foreach (var c in campeonatos)
                {
                    _campeonatos.Add(c);
                }
                OnPropertyChanged(nameof(Campeonatos));
                AtualizarFavoritos();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao carregar campeonatos: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void AtualizarFavoritos()
        {
            Favoritos = new ObservableCollection<Campeonato>(_campeonatos.Where(c => c.EhFavorito));
        }

        private async Task FavoritarAsync(Campeonato campeonato)
        {
            if (campeonato == null) return;

            campeonato.EhFavorito = !campeonato.EhFavorito;
            await _databaseService.AtualizarCampeonatoAsync(campeonato);
            AtualizarFavoritos();
        }
    }
}
