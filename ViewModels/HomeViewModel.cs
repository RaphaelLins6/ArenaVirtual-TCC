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
                var usuarioAtual = SessaoService.Instancia.GetUsuarioAtual();
                if (usuarioAtual == null) return;

                var todosCampeonatos = await _databaseService.ListarCampeonatosAsync() ?? [];
                var favoritosDoUsuario = await _databaseService.ListarFavoritosPorUsuarioAsync(usuarioAtual.Id);

                var idsFavoritos = new HashSet<int>(favoritosDoUsuario.Select(f => f.CampeonatoId));

                Favoritos.Clear();
                _campeonatos.Clear();

                foreach (var c in todosCampeonatos) {
                    c.EhFavorito = idsFavoritos.Contains(c.Id);
                    _campeonatos.Add(c);
                    if (c.EhFavorito)
                        Favoritos.Add(c);
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
            if (campeonato == null) return;

            var usuarioAtual = SessaoService.Instancia.GetUsuarioAtual();
            if (usuarioAtual == null) return;

            campeonato.EhFavorito = !campeonato.EhFavorito;

            if (campeonato.EhFavorito) {
                var favorito = new UsuarioCampeonatoFavorito {
                    UsuarioId = usuarioAtual.Id,
                    CampeonatoId = campeonato.Id
                };
                await _databaseService.InserirFavoritoAsync(favorito);
            } else {
                var favoritoExistente = (await _databaseService.ListarFavoritosPorUsuarioAsync(usuarioAtual.Id))
                    .FirstOrDefault(f => f.CampeonatoId == campeonato.Id);
                if (favoritoExistente != null) {
                    await _databaseService.DeletarFavoritoAsync(favoritoExistente);
                }
            }

            await CarregarCampeonatos();
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
