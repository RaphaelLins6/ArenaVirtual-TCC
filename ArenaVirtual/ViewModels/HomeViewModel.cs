using ArenaVirtual.Models;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace ArenaVirtual.ViewModels {
    // Usando o construtor primário para a injeção de dependência.
    public partial class HomeViewModel(DatabaseService databaseService, SyncService syncService, ConnectivityService connectivityService) : ObservableObject {

        [ObservableProperty]
        private bool isBusy = false;

        [ObservableProperty]
        private bool isOnline;

        private readonly ObservableCollection<Campeonato> _campeonatos = new();
        public ObservableCollection<Campeonato> Campeonatos => _campeonatos;

        private readonly ObservableCollection<Campeonato> _favoritos = new();
        public ObservableCollection<Campeonato> Favoritos => _favoritos;

        private readonly DatabaseService _databaseService = databaseService;
        private readonly SyncService _syncService = syncService;
        private readonly ConnectivityService _connectivityService = connectivityService;
        private readonly SemaphoreSlim _syncSemaphore = new(1, 1);

        // Construtor sem parâmetros para a visualização em design-time.
        public HomeViewModel() : this(null!, null!, null!) {
            _connectivityService.ConnectivityChanged += OnConnectivityChanged;
            UpdateConnectivityStatus();
        }

        private void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e) {
            UpdateConnectivityStatus();
        }

        public void UpdateConnectivityStatus() {
            IsOnline = _connectivityService.IsConnected;
        }

        [RelayCommand]
        private async Task Favoritar(Campeonato campeonato) {
            if (campeonato == null || IsBusy) return;
            await FavoritarAsync(campeonato);
        }

        [RelayCommand]
        private async Task VerCampeonato(Campeonato campeonato) {
            if (campeonato == null || IsBusy) return;
            await VerCampeonatoAsync(campeonato);
        }

        [RelayCommand]
        private async Task Sincronizar() {
            Debug.WriteLine("[HomeViewModel] Comando Sincronizar acionado.");
            await SincronizarAsync();
        }

        public async Task OnAppearingAsync() {
            Debug.WriteLine("[HomeViewModel] OnAppearingAsync chamado.");
            UpdateConnectivityStatus();
            await _syncSemaphore.WaitAsync();
            try {
                IsBusy = true;
                await _syncService.SyncAsync(new Progress<string>());
                await CarregarTodosCampeonatos();
            } catch (Exception ex) {
                Debug.WriteLine($"[HomeViewModel] Erro em OnAppearingAsync: {ex.Message}");
            } finally {
                IsBusy = false;
                _syncSemaphore.Release();
            }
        }

        private async Task SincronizarAsync() {
            if (!IsOnline) return;
            await _syncSemaphore.WaitAsync();
            try {
                IsBusy = true;
                await _syncService.SyncAsync(new Progress<string>());
                await CarregarTodosCampeonatos();
            } catch (Exception ex) {
                Debug.WriteLine($"[HomeViewModel] Erro na sincronização: {ex.Message}");
            } finally {
                IsBusy = false;
                _syncSemaphore.Release();
            }
        }

        private async Task CarregarTodosCampeonatos() {
            try {
                var usuarioAtual = SessaoService.Instancia.GetUsuarioAtual();

                if (usuarioAtual == null) {
                    Debug.WriteLine("[HomeViewModel] SessaoService.GetUsuarioAtual() retornou NULL em CarregarTodosCampeonatos.");
                    return;
                }

                if (usuarioAtual.ClientAppId == Guid.Empty) {
                    Debug.WriteLine("[HomeViewModel] usuarioAtual.ClientAppId está vazio (Guid.Empty) em CarregarTodosCampeonatos.");
                    return;
                }

                Debug.WriteLine($"[HomeViewModel] Usuário atual OK em CarregarTodosCampeonatos: {usuarioAtual.Nome} | ID: {usuarioAtual.ClientAppId}");

                var todosCampeonatos = await _databaseService.ListarCampeonatosAsync() ?? new List<Campeonato>();
                var favoritosDoUsuario = await _databaseService.ListarFavoritosPorUsuarioAsync(usuarioAtual.ClientAppId);
                var idsFavoritos = new HashSet<Guid>(favoritosDoUsuario.Select(f => f.CampeonatoClientAppId));

                MainThread.BeginInvokeOnMainThread(() => {
                    Favoritos.Clear();
                    _campeonatos.Clear();

                    foreach (var c in todosCampeonatos) {
                        c.EhFavorito = idsFavoritos.Contains(c.ClientAppId);
                        _campeonatos.Add(c);
                        if (c.EhFavorito)
                            Favoritos.Add(c);
                    }
                });
            } catch (Exception ex) {
                Debug.WriteLine($"[HomeViewModel] Erro ao carregar campeonatos: {ex.Message}");
            }
        }


        private async Task CarregarFavoritos() {
            try {
                var usuarioAtual = SessaoService.Instancia.GetUsuarioAtual();
                if (usuarioAtual == null || usuarioAtual.ClientAppId == Guid.Empty) return;

                var favoritosDoUsuario = await _databaseService.ListarFavoritosPorUsuarioAsync(usuarioAtual.ClientAppId);
                var idsFavoritos = new HashSet<Guid>(favoritosDoUsuario.Select(f => f.CampeonatoClientAppId));

                MainThread.BeginInvokeOnMainThread(() => {
                    Favoritos.Clear();
                    foreach (var c in _campeonatos) {
                        c.EhFavorito = idsFavoritos.Contains(c.ClientAppId);
                        if (c.EhFavorito)
                            Favoritos.Add(c);
                    }
                });
            } catch (Exception ex) {
                Debug.WriteLine($"Erro ao carregar favoritos: {ex.Message}");
            }
        }

        private async Task FavoritarAsync(Campeonato campeonato) {
            var usuarioAtual = SessaoService.Instancia.GetUsuarioAtual();
            if (usuarioAtual == null || usuarioAtual.ClientAppId == Guid.Empty) return;

            campeonato.EhFavorito = !campeonato.EhFavorito;

            if (campeonato.EhFavorito) {
                var favorito = new UsuarioCampeonatoFavorito {
                    UsuarioClientAppId = usuarioAtual.ClientAppId,
                    CampeonatoClientAppId = campeonato.ClientAppId
                };
                await _databaseService.InserirFavoritoAsync(favorito);
                Favoritos.Add(campeonato);
            } else {
                var favoritoExistente = (await _databaseService.ListarFavoritosPorUsuarioAsync(usuarioAtual.ClientAppId))
                    .FirstOrDefault(f => f.CampeonatoClientAppId == campeonato.ClientAppId);
                if (favoritoExistente != null) {
                    await _databaseService.DeletarFavoritoAsync(favoritoExistente);
                }
                Favoritos.Remove(campeonato);
            }

            var tempFavoritos = Favoritos.OrderBy(c => c.Nome).ToList();
            MainThread.BeginInvokeOnMainThread(() => {
                Favoritos.Clear();
                foreach (var fav in tempFavoritos) {
                    Favoritos.Add(fav);
                }
            });
        }

        private async Task VerCampeonatoAsync(Campeonato campeonato) {
            Debug.WriteLine($"[HomeViewModel] VerCampeonatoAsync chamado. Campeonato: {campeonato?.Nome ?? "NULO"}, ID: {campeonato?.Id ?? 0}");
            if (campeonato == null) {
                Debug.WriteLine("[DEBUG] VerCampeonatoCommand acionado, mas campeonato é nulo.");
                return;
            }

            // Usando o roteamento do Shell para navegar e passar o objeto
            var navigationParameter = new Dictionary<string, object> {
                { "Campeonato", campeonato }
            };

            // A rota deve ser a mesma que você registrou no AppShell.xaml.cs
            await Shell.Current.GoToAsync("campeonatoDetalhes", navigationParameter);
        }
    }
}