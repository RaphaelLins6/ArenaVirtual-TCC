using ArenaVirtual.ViewModels;
using ArenaVirtual.Services;
using System.Diagnostics;

namespace ArenaVirtual.Views {
    public partial class HomePage : ContentPage {
        private readonly ConnectivityService _connectivityService;

        public HomePage() {
            InitializeComponent();

            var serviceProvider = App.Current?.Handler?.MauiContext?.Services;
            if (serviceProvider == null) {
                Debug.WriteLine("[HomePage] Service provider não encontrado.");
                return;
            }

            var databaseService = serviceProvider.GetRequiredService<DatabaseService>();
            var syncService = serviceProvider.GetRequiredService<SyncService>();
            _connectivityService = serviceProvider.GetRequiredService<ConnectivityService>();

            BindingContext = new HomeViewModel(databaseService, syncService, _connectivityService);

            Debug.WriteLine($"[HomePage] BindingContext atribuído: {BindingContext?.GetType().Name ?? "Nulo"}");
        }

        protected override async void OnAppearing() {
            base.OnAppearing();
            if (BindingContext is HomeViewModel vm) {
                // Se inscreve para escutar as mudanças de conectividade
                _connectivityService.ConnectivityChanged += OnConnectivityChanged;
                // Força a atualização do status na entrada da página
                vm.UpdateConnectivityStatus();
                // Carrega os dados mais recentes
                await vm.OnAppearingAsync();
                Debug.WriteLine($"[HomePage] OnAppearing - OnAppearingAsync chamado. Total Campeonatos: {vm.Campeonatos.Count}, Favoritos: {vm.Favoritos.Count}");
            }
        }

        protected override void OnDisappearing() {
            base.OnDisappearing();
            // Remove a inscrição para evitar vazamentos de memória
            _connectivityService.ConnectivityChanged -= OnConnectivityChanged;
        }

        private void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e) {
            // Chama a lógica de atualização no ViewModel sempre que a conectividade muda
            if (BindingContext is HomeViewModel vm) {
                vm.UpdateConnectivityStatus();
            }
        }

        protected override void OnBindingContextChanged() {
            base.OnBindingContextChanged();
            Debug.WriteLine($"[HomePage] BindingContext Changed para: {BindingContext?.GetType().Name ?? "Nulo"}");
        }
    }
}