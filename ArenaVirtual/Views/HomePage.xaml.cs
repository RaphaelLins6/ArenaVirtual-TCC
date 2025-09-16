using ArenaVirtual.ViewModels;
using ArenaVirtual.Services;
using System.Diagnostics;

namespace ArenaVirtual.Views {
    public partial class HomePage : ContentPage {
        public HomePage() {
            InitializeComponent();

            var serviceProvider = App.Current?.Handler?.MauiContext?.Services;
            if (serviceProvider == null) {
                Debug.WriteLine("[HomePage] Service provider não encontrado.");
                return;
            }

            var databaseService = serviceProvider.GetRequiredService<DatabaseService>();
            var syncService = serviceProvider.GetRequiredService<SyncService>();
            var connectivityService = serviceProvider.GetRequiredService<ConnectivityService>();

            BindingContext = new HomeViewModel(databaseService, syncService, connectivityService);

            Debug.WriteLine($"[HomePage] BindingContext atribuído: {BindingContext?.GetType().Name ?? "Nulo"}");
        }

        protected override async void OnAppearing() {
            base.OnAppearing();
            if (BindingContext is HomeViewModel vm) {
                await vm.OnAppearingAsync();
                Debug.WriteLine($"[HomePage] OnAppearing - OnAppearingAsync chamado. Total Campeonatos: {vm.Campeonatos.Count}, Favoritos: {vm.Favoritos.Count}");
            }
        }

        protected override void OnBindingContextChanged() {
            base.OnBindingContextChanged();
            Debug.WriteLine($"[HomePage] BindingContext Changed para: {BindingContext?.GetType().Name ?? "Nulo"}");
        }
    }
}