using ArenaVirtual.Models;
using ArenaVirtual.Services;
using System.ComponentModel;
using System.IO;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using CommunityToolkit.Mvvm.Input;
using ArenaVirtual.ViewModels;
using System;
using System.Threading.Tasks;

namespace ArenaVirtual.Popups;

public partial class AlterarImagemPopup : ContentPage {

    private readonly IAlertService _alertService;
    private readonly PerfilViewModel _viewModel;

    public AlterarImagemPopup(IAlertService alertService, PerfilViewModel viewModel) {
        InitializeComponent();
        _alertService = alertService;
        _viewModel = viewModel;
        this.BindingContext = _viewModel;
    }

    private async void EscolherImagem_Clicked(object sender, EventArgs e) {
        try {
            var result = await FilePicker.PickAsync(new PickOptions {
                PickerTitle = "Selecione uma imagem",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null && !string.IsNullOrEmpty(result.FullPath)) {
                _viewModel.CaminhoNovaImagemSelecionada = result.FullPath;

                ImagemPerfil.Source = ImageSource.FromFile(result.FullPath);
            }
        } catch (Exception ex) {
            await _alertService.DisplayAlert("Erro", $"Não foi possível escolher a imagem: {ex.Message}", "OK");
        }
    }
}
