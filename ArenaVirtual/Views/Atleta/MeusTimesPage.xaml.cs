using ArenaVirtual.ViewModels.Atleta;
using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using CommunityToolkit.Mvvm.DependencyInjection; // Pode ser necessário adicionar essa referência
using Microsoft.Extensions.DependencyInjection; // Para usar GetService

namespace ArenaVirtual.Views.Atleta {

    public partial class MeusTimesPage : ContentPage {

        private readonly MeuTimePageViewModel _viewModel;
        private readonly IServiceProvider _serviceProvider; // Adicione o ServiceProvider

        // Construtor ajustado para usar a injeção de dependência
        public MeusTimesPage(MeuTimePageViewModel viewModel, IServiceProvider serviceProvider) {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
            _serviceProvider = serviceProvider;
        }

        // Removida a propriedade Vm, pois o _viewModel privado já cumpre o papel.

        protected override async void OnAppearing() {
            base.OnAppearing();

            // 1. Carregar os dados do Time (Sempre na aba principal)
            await _viewModel.LoadDataAsync();

            // 2. Se o time foi carregado, inicializa a aba Partidas.
            // O acesso ao Time está via _viewModel.Time, não mais via Vm.
            if (_viewModel.Time != null && _viewModel.Time.Id > 0) {

                // Obtém a instância do ViewModel da aba irmã
                var partidasViewModel = _serviceProvider.GetService<PartidasViewModel>();

                if (partidasViewModel != null) {
                    Debug.WriteLine($"[MeusTimesPage.xaml.cs] Compartilhando TimeId: {_viewModel.Time.Id} com PartidasViewModel.");
                    // Chamamos o novo método InitializeAsync
                    await partidasViewModel.InitializeAsync(_viewModel.Time.Id);
                }
            }
        }
    }
}