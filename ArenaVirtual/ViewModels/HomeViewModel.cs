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

        private ObservableCollection<Campeonato> _favoritos = new ObservableCollection<Campeonato>();
        public ObservableCollection<Campeonato> Favoritos {
            get => _favoritos;
            set {
                _favoritos = value;
                OnPropertyChanged(nameof(Favoritos));
            }
        }

        // Sinalizador para o primeiro carregamento da página
        private bool _isFirstLoad = true;

        public ICommand FavoritarCommand { get; }
        public ICommand VerCampeonatoCommand { get; }
        private readonly DatabaseService _databaseService;
        private readonly SyncService _syncService;

        // Usamos um SemaphoreSlim para garantir que apenas uma execução
        // de OnAppearingAsync ocorra por vez.
        private readonly SemaphoreSlim _syncSemaphore = new SemaphoreSlim(1, 1);

        public HomeViewModel(DatabaseService databaseService, SyncService syncService) {
            _campeonatos = new ObservableCollection<Campeonato>();
            Campeonatos = _campeonatos;
            _databaseService = databaseService;
            _syncService = syncService;

            FavoritarCommand = new Command<object>(
                async obj => {
                    if (obj is Campeonato campeonato)
                        await FavoritarAsync(campeonato);
                });

            VerCampeonatoCommand = new Command<Campeonato>(async (campeonato) => {
                await VerCampeonatoAsync(campeonato);
            });
        }

        public async Task OnAppearingAsync() {
            Debug.WriteLine("[HomeViewModel] OnAppearingAsync chamado.");

            // Tenta adquirir o 'lock' de forma assíncrona.
            await _syncSemaphore.WaitAsync();
            try {
                // Sincroniza e carrega tudo apenas no primeiro carregamento.
                if (_isFirstLoad) {
                    _isFirstLoad = false;
                    IsBusy = true;
                    await _syncService.SyncAsync(new Progress<string>());
                    await CarregarTodosCampeonatos();
                } else {
                    // Nas chamadas subsequentes, apenas atualize os favoritos para ser mais eficiente.
                    await CarregarFavoritos();
                }
            } catch (Exception ex) {
                Debug.WriteLine($"[HomeViewModel] Erro em OnAppearingAsync: {ex.Message}");
            } finally {
                IsBusy = false;
                // Libera o 'lock' para que a próxima chamada possa ser executada.
                _syncSemaphore.Release();
            }
        }

        private async Task CarregarTodosCampeonatos() {
            try {
                var usuarioAtual = SessaoService.Instancia.GetUsuarioAtual();
                if (usuarioAtual == null) return;

                var todosCampeonatos = await _databaseService.ListarCampeonatosAsync() ?? new List<Campeonato>();
                var favoritosDoUsuario = await _databaseService.ListarFavoritosPorUsuarioAsync(usuarioAtual.Id);

                var idsFavoritos = new HashSet<int>(favoritosDoUsuario.Select(f => f.CampeonatoId));

                MainThread.BeginInvokeOnMainThread(() => {
                    Favoritos.Clear();
                    _campeonatos.Clear();

                    foreach (var c in todosCampeonatos) {
                        c.EhFavorito = idsFavoritos.Contains(c.Id);
                        _campeonatos.Add(c);
                        if (c.EhFavorito)
                            Favoritos.Add(c);
                    }
                });
            } catch (Exception ex) {
                Debug.WriteLine($"Erro ao carregar campeonatos: {ex.Message}");
            }
        }

        // Novo método para carregar apenas os favoritos, otimizando o reaparecimento da tela
        private async Task CarregarFavoritos() {
            try {
                var usuarioAtual = SessaoService.Instancia.GetUsuarioAtual();
                if (usuarioAtual == null) return;

                var favoritosDoUsuario = await _databaseService.ListarFavoritosPorUsuarioAsync(usuarioAtual.Id);
                var idsFavoritos = new HashSet<int>(favoritosDoUsuario.Select(f => f.CampeonatoId));

                MainThread.BeginInvokeOnMainThread(() => {
                    Favoritos.Clear();
                    foreach (var c in _campeonatos) {
                        c.EhFavorito = idsFavoritos.Contains(c.Id);
                        if (c.EhFavorito)
                            Favoritos.Add(c);
                    }
                });
            } catch (Exception ex) {
                Debug.WriteLine($"Erro ao carregar favoritos: {ex.Message}");
            }
        }

        private async Task FavoritarAsync(Campeonato campeonato) {
            if (campeonato == null) return;

            var usuarioAtual = SessaoService.Instancia.GetUsuarioAtual();
            if (usuarioAtual == null) return;

            campeonato.EhFavorito = !campeonato.EhFavorito;

            // Oculta/exibe o favorito da lista de favoritos imediatamente para dar feedback instantâneo
            if (campeonato.EhFavorito) {
                // Adiciona o favorito na lista
                var favorito = new UsuarioCampeonatoFavorito {
                    UsuarioId = usuarioAtual.Id,
                    CampeonatoId = campeonato.Id
                };
                await _databaseService.InserirFavoritoAsync(favorito);
                Favoritos.Add(campeonato);
            } else {
                // Remove o favorito da lista
                var favoritoExistente = (await _databaseService.ListarFavoritosPorUsuarioAsync(usuarioAtual.Id))
                    .FirstOrDefault(f => f.CampeonatoId == campeonato.Id);
                if (favoritoExistente != null) {
                    await _databaseService.DeletarFavoritoAsync(favoritoExistente);
                }
                Favoritos.Remove(campeonato);
            }

            // Reordena a lista de favoritos
            var tempFavoritos = Favoritos.OrderBy(c => c.Nome).ToList();
            MainThread.BeginInvokeOnMainThread(() => {
                Favoritos.Clear();
                foreach (var fav in tempFavoritos) {
                    Favoritos.Add(fav);
                }
            });
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