using ArenaVirtual.ViewModels;
using ArenaVirtual.Services;
using System.Diagnostics; // Adicionado para Debug.WriteLine

namespace ArenaVirtual.Views {
    public partial class HomePage : ContentPage {
        public HomePage() {
            InitializeComponent();
            var databaseService = App.Current?.Handler?.MauiContext?.Services?.GetRequiredService<DatabaseService>();
            BindingContext = new ViewModels.HomeViewModel(databaseService!);
            Debug.WriteLine($"[HomePage] BindingContext atribuído: {BindingContext?.GetType().Name ?? "Nulo"}");
        }

        protected override async void OnAppearing() {
            base.OnAppearing();
            if (BindingContext is ArenaVirtual.ViewModels.HomeViewModel vm) {
                await vm.CarregarCampeonatos();
                Debug.WriteLine($"[HomePage] OnAppearing - CarregarCampeonatos chamado. Total Campeonatos: {vm.Campeonatos.Count}, Favoritos: {vm.Favoritos.Count}");
            }
        }

        protected override void OnBindingContextChanged() {
            base.OnBindingContextChanged();
            Debug.WriteLine($"[HomePage] BindingContext Changed para: {BindingContext?.GetType().Name ?? "Nulo"}");
        }
    }
}
