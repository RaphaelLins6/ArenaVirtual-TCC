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
    // A única responsabilidade deste pop-up é permitir que o usuário escolha uma imagem
    // e repassar o caminho para a ViewModel.
    // A lógica de salvamento e fechamento agora está na PerfilViewModel.

    private readonly IAlertService _alertService;
    private readonly PerfilViewModel _viewModel;

    // A ViewModel agora é injetada via construtor para que o pop-up possa interagir com ela.
    public AlterarImagemPopup(IAlertService alertService, PerfilViewModel viewModel) {
        InitializeComponent();
        _alertService = alertService;
        _viewModel = viewModel;
        // Define o BindingContext do pop-up para a ViewModel.
        this.BindingContext = _viewModel;
    }

    private async void EscolherImagem_Clicked(object sender, EventArgs e) {
        try {
            var result = await FilePicker.PickAsync(new PickOptions {
                PickerTitle = "Selecione uma imagem",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null && !string.IsNullOrEmpty(result.FullPath)) {
                // Repassa o caminho da nova imagem para a ViewModel
                _viewModel.CaminhoNovaImagemSelecionada = result.FullPath;

                // Atualiza a imagem exibida no pop-up para visualização
                ImagemPerfil.Source = ImageSource.FromFile(result.FullPath);
            }
        } catch (Exception ex) {
            await _alertService.DisplayAlert("Erro", $"Não foi possível escolher a imagem: {ex.Message}", "OK");
        }
    }
}
