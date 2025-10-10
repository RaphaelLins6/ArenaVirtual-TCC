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
    private readonly CampeonatoService _campeonatoService;
    private readonly SessaoService _sessaoService;
    private readonly SyncService _syncService;

    [ObservableProperty]
    private Campeonato campeonato;

    [ObservableProperty]
    private ImageSource? logoImageSource;

    [ObservableProperty]
    private string? numeroMaximoEquipesText;

    [ObservableProperty]
    private string? valorTaxaInscricaoText;

    [ObservableProperty]
    private string? validationMessage;

    // A propriedade FormatoCampeonatoSelecionado foi removida!
    // O Picker agora faz binding direto com Campeonato.FormatoCampeonato.

    public IRelayCommand SalvarCommand { get; }
    public IRelayCommand SelecionarLogoCommand { get; }

    public EditarCampeonatoViewModel(CampeonatoService campeonatoService, SessaoService sessaoService, SyncService syncService, Campeonato campeonato) {
        _campeonatoService = campeonatoService;
        _sessaoService = sessaoService;
        _syncService = syncService;
        Campeonato = campeonato;

        // Inicializa as propriedades de texto (Entries) com os dados existentes
        NumeroMaximoEquipesText = Campeonato.NumeroMaximoEquipes.ToString();
        ValorTaxaInscricaoText = Campeonato.ValorTaxaInscricao.ToString();

        // Não precisamos inicializar o FormatoCampeonato, pois ele está no objeto Campeonato

        SalvarCommand = new AsyncRelayCommand(SalvarAsync);
        SelecionarLogoCommand = new AsyncRelayCommand(SelecionarLogoAsync);

        if (!string.IsNullOrEmpty(Campeonato.LogoUrl) && File.Exists(Campeonato.LogoUrl)) {
            LogoImageSource = ImageSource.FromFile(Campeonato.LogoUrl);
        }
    }

    private async Task SalvarAsync() {
        ValidationMessage = string.Empty;

        if (!int.TryParse(NumeroMaximoEquipesText, out int numeroEquipes)) {
            ValidationMessage = "Número máximo de equipes deve ser um número válido.";
            return;
        }

        if (!decimal.TryParse(ValorTaxaInscricaoText, out decimal valorTaxa)) {
            ValidationMessage = "Valor da taxa de inscrição deve ser um número válido.";
            return;
        }

        // A alteração no FormatoCampeonato é feita automaticamente pelo Picker
        // devido ao binding direto com Campeonato.FormatoCampeonato.

        Campeonato.NumeroMaximoEquipes = numeroEquipes;
        Campeonato.ValorTaxaInscricao = valorTaxa;

        try {
            var usuarioLogado = _sessaoService.GetUsuarioAtual();
            if (usuarioLogado == null) {
                Debug.WriteLine("[EditarCampeonatoViewModel] Nenhum usuário logado encontrado. Não foi possível salvar o campeonato.");
                ValidationMessage = "Nenhum usuário logado. Por favor, faça login novamente.";
                return;
            }

            // Lógica de sincronização do ID do organizador
            if (!usuarioLogado.IdServidor.HasValue) {
                ValidationMessage = "Sincronizando seu perfil para salvar o campeonato. Por favor, aguarde...";
                await _syncService.SyncAsync(new Progress<string>());

                usuarioLogado = _sessaoService.GetUsuarioAtual();
                if (usuarioLogado?.IdServidor.HasValue == true) {
                    Campeonato.OrganizadorId = usuarioLogado.IdServidor.Value;
                } else {
                    ValidationMessage = "Não foi possível sincronizar o perfil. Tente novamente.";
                    return;
                }
            } else {
                Campeonato.OrganizadorId = usuarioLogado.IdServidor.Value;
            }

            await _campeonatoService.AtualizarAsync(Campeonato);

            await Shell.Current.GoToAsync("..");
        } catch (Exception ex) {
            Debug.WriteLine($"[EditarCampeonatoViewModel] Erro ao salvar campeonato: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Erro", $"Falha ao salvar o campeonato: {ex.Message}", "OK");
        }
    }

    private async Task SelecionarLogoAsync() {
        try {
            var result = await FilePicker.Default.PickAsync(new PickOptions {
                PickerTitle = "Selecione o logo do campeonato",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null) {
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