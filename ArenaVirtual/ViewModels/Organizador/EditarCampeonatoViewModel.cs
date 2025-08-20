using ArenaVirtual.Models;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ArenaVirtual.ViewModels.Organizador;

public partial class EditarCampeonatoViewModel : ObservableObject {
    // A dependência do DatabaseService não é mais estritamente necessária aqui,
    // pois a lógica de atualização foi movida para o CampeonatoService.
    private readonly CampeonatoService _campeonatoService;

    [ObservableProperty]
    private Campeonato campeonato;

    [ObservableProperty]
    private ImageSource? logoImageSource;

    public IRelayCommand SalvarCommand { get; }
    public IRelayCommand SelecionarLogoCommand { get; }

    // O construtor agora recebe o CampeonatoService, que encapsula a lógica de atualização e sincronização.
    public EditarCampeonatoViewModel(CampeonatoService campeonatoService, Campeonato campeonato) {
        _campeonatoService = campeonatoService;
        Campeonato = campeonato;
        SalvarCommand = new RelayCommand(async () => await SalvarAsync());
        SelecionarLogoCommand = new RelayCommand(async () => await SelecionarLogoAsync());

        if (!string.IsNullOrEmpty(Campeonato.LogoUrl) && File.Exists(Campeonato.LogoUrl))
            LogoImageSource = ImageSource.FromFile(Campeonato.LogoUrl);
    }

    private async Task SalvarAsync() {
        // O ViewModel delega a lógica de atualização e sincronização para o serviço.
        // O CampeonatoService.AtualizarAsync já se encarregará de marcar o objeto
        // como não sincronizado e chamar o SyncService.
        await _campeonatoService.AtualizarAsync(Campeonato);

        await Shell.Current.GoToAsync("..");
    }

    private async Task SelecionarLogoAsync() {
        try {
            var result = await FilePicker.Default.PickAsync(new PickOptions {
                PickerTitle = "Selecione o logo do campeonato",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null) {
                // Copia a imagem selecionada para o diretório de dados do app para garantir que ela persista.
                var newFilePath = Path.Combine(FileSystem.AppDataDirectory, result.FileName);
                using (var stream = await result.OpenReadAsync()) {
                    using (var newFileStream = File.OpenWrite(newFilePath)) {
                        await stream.CopyToAsync(newFileStream);
                    }
                }

                LogoImageSource = ImageSource.FromFile(newFilePath);
                Campeonato.LogoUrl = newFilePath;
            }
        } catch (Exception ex) {
            Debug.WriteLine($"Erro ao selecionar logo: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Erro", $"Falha ao selecionar imagem: {ex.Message}", "OK");
        }
    }
}