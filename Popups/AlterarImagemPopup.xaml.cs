using ArenaVirtual.Models;
using ArenaVirtual.Services;

namespace ArenaVirtual.Popups;

public partial class AlterarImagemPopup : ContentPage {
    private readonly Usuario _usuario;
    private readonly DatabaseService _databaseService; 
    private readonly IAlertService _alertService;
    private string? _novaImagemPath; 

    public event EventHandler<string>? ImagemAtualizada; // Declarado como anulável

    public AlterarImagemPopup(Usuario usuario, IAlertService alertService) {
        InitializeComponent();
        _usuario = usuario;
        _alertService = alertService;

        var serviceProvider = App.Current?.Handler?.MauiContext?.Services;
        if (serviceProvider != null) {
            _databaseService = serviceProvider.GetRequiredService<DatabaseService>();
        } else {
            throw new InvalidOperationException("DatabaseService not registered or app context is null.");
        }

        if (!string.IsNullOrEmpty(_usuario.ImagemPath))
            ImagemPerfil.Source = _usuario.ImagemPath;
    }

    private async void EscolherImagem_Clicked(object sender, EventArgs e) {
        try {
            var resultado = await FilePicker.PickAsync(new PickOptions {
                PickerTitle = "Selecione uma imagem",
                FileTypes = FilePickerFileType.Images
            });

            if (resultado != null) {
                _novaImagemPath = resultado.FullPath;
                ImagemPerfil.Source = _novaImagemPath;
            }
        } catch (Exception ex) {
            await _alertService.DisplayAlert("Erro", $"Não foi possível selecionar a imagem: {ex.Message}", "OK");
        }
    }

    private async void Cancelar_Clicked(object sender, EventArgs e) {
        await Navigation.PopModalAsync();
    }

    private async void Salvar_Clicked(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(_novaImagemPath)) {
            _usuario.ImagemPath = _novaImagemPath;
            await _databaseService.AtualizarUsuarioAsync(_usuario);
            await _alertService.DisplayAlert("Sucesso", "Imagem atualizada!", "OK");
            ImagemAtualizada?.Invoke(this, _usuario.ImagemPath); // Dispara o evento
        }

        await Navigation.PopModalAsync();
    }
}