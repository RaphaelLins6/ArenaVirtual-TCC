using ArenaVirtual.ViewModels;
using ArenaVirtual.Services;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection; // Necessário para GetRequiredService

namespace ArenaVirtual.Views {
    public partial class HomePage : ContentPage {
        public HomePage() {
            InitializeComponent();

            // Obtenha o service provider do contexto atual.
            var serviceProvider = App.Current?.Handler?.MauiContext?.Services;
            if (serviceProvider == null) {
                Debug.WriteLine("[HomePage] Service provider não encontrado.");
                return;
            }

            // Obtenha os serviços necessários.
            var databaseService = serviceProvider.GetRequiredService<DatabaseService>();
            var syncService = serviceProvider.GetRequiredService<SyncService>(); // Obtenha o SyncService

            // Atribua o BindingContext, passando ambos os serviços.
            BindingContext = new HomeViewModel(databaseService, syncService);

            Debug.WriteLine($"[HomePage] BindingContext atribuído: {BindingContext?.GetType().Name ?? "Nulo"}");
        }

        protected override async void OnAppearing() {
            base.OnAppearing();
            if (BindingContext is HomeViewModel vm) {
                // Chame o novo método OnAppearingAsync que já dispara a sincronização e carrega os dados.
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