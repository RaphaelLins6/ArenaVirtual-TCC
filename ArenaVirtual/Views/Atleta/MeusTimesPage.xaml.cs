using ArenaVirtual.ViewModels.Atleta;
using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using CommunityToolkit.Mvvm.DependencyInjection; 
using Microsoft.Extensions.DependencyInjection; 

namespace ArenaVirtual.Views.Atleta {

    public partial class MeusTimesPage : ContentPage {

        private readonly MeuTimePageViewModel _viewModel;
        private readonly IServiceProvider _serviceProvider; 

        public MeusTimesPage(MeuTimePageViewModel viewModel, IServiceProvider serviceProvider) {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
            _serviceProvider = serviceProvider;
        }

        protected override async void OnAppearing() {
            base.OnAppearing();

            await _viewModel.LoadDataAsync();
            if (_viewModel.Time != null && _viewModel.Time.Id > 0) {

                var partidasViewModel = _serviceProvider.GetService<PartidasViewModel>();

                if (partidasViewModel != null) {
                    //Debug.WriteLine($"[MeusTimesPage.xaml.cs] Compartilhando TimeId: {_viewModel.Time.Id} com PartidasViewModel.");
                    await partidasViewModel.InitializeAsync(_viewModel.Time.Id);
                }
            }
        }
    }
}