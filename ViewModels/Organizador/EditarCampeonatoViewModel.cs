using ArenaVirtual.Models;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ArenaVirtual.ViewModels.Organizador;
public partial class EditarCampeonatoViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    private Campeonato campeonato;

    [ObservableProperty]
    private ImageSource? logoImageSource;

    public IRelayCommand SalvarCommand { get; }
    public IRelayCommand SelecionarLogoCommand { get; }

    public EditarCampeonatoViewModel(DatabaseService databaseService, Campeonato campeonato)
    {
        _databaseService = databaseService;
        Campeonato = campeonato;
        SalvarCommand = new RelayCommand(async () => await SalvarAsync());
        SelecionarLogoCommand = new RelayCommand(async () => await SelecionarLogoAsync());

        if (!string.IsNullOrEmpty(Campeonato.LogoUrl))
            LogoImageSource = ImageSource.FromFile(Campeonato.LogoUrl);
    }

    private async Task SalvarAsync()
    {
        await _databaseService.AtualizarCampeonatoAsync(Campeonato);
        await Shell.Current.GoToAsync("..");
    }

    private async Task SelecionarLogoAsync()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Selecione o logo do campeonato",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null)
            {
                LogoImageSource = ImageSource.FromFile(result.FullPath);

                Campeonato.LogoUrl = result.FullPath;
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Erro", $"Falha ao selecionar imagem: {ex.Message}", "OK");
        }
    }
}
