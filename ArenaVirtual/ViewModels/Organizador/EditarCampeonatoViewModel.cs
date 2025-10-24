using ArenaVirtual.Models;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

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

    // Controle de estado para evitar cliques duplos
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SalvarCommand))]
    private bool isBusy;

    public IRelayCommand SalvarCommand { get; }
    public IRelayCommand SelecionarLogoCommand { get; }

    public EditarCampeonatoViewModel(CampeonatoService campeonatoService, SessaoService sessaoService, SyncService syncService, Campeonato campeonato) {
        _campeonatoService = campeonatoService;
        _sessaoService = sessaoService;
        _syncService = syncService;
        Campeonato = campeonato;

        NumeroMaximoEquipesText = Campeonato.NumeroMaximoEquipes.ToString();
        ValorTaxaInscricaoText = Campeonato.ValorTaxaInscricao.ToString();

        SalvarCommand = new AsyncRelayCommand(SalvarAsync, CanExecuteSalvar);
        SelecionarLogoCommand = new AsyncRelayCommand(SelecionarLogoAsync);

        if (!string.IsNullOrEmpty(Campeonato.LogoUrl) && File.Exists(Campeonato.LogoUrl)) {
            LogoImageSource = ImageSource.FromFile(Campeonato.LogoUrl);
        }
    }

    private bool CanExecuteSalvar() => !IsBusy;

    private async Task SalvarAsync() {
        IsBusy = true;
        ValidationMessage = string.Empty;

        if (!int.TryParse(NumeroMaximoEquipesText, out int numeroEquipes)) {
            ValidationMessage = "Número máximo de equipes deve ser um número válido.";
            IsBusy = false;
            return;
        }

        if (!decimal.TryParse(ValorTaxaInscricaoText, out decimal valorTaxa)) {
            ValidationMessage = "Valor da taxa de inscrição deve ser um número válido.";
            IsBusy = false;
            return;
        }

        Campeonato? campeonatoAAtualizar = await _campeonatoService.ObterPorClientAppIdAsync(Campeonato.ClientAppId);

        if (campeonatoAAtualizar == null) {
            await Application.Current.MainPage.DisplayAlert("Erro", "Falha ao encontrar o campeonato no banco de dados.", "OK");
            IsBusy = false;
            return;
        }

        campeonatoAAtualizar.Nome = Campeonato.Nome;
        campeonatoAAtualizar.Local = Campeonato.Local;
        campeonatoAAtualizar.NomeOrganizador = Campeonato.NomeOrganizador;
        campeonatoAAtualizar.EmailOrganizador = Campeonato.EmailOrganizador;
        campeonatoAAtualizar.TelefoneOrganizador = Campeonato.TelefoneOrganizador;
        campeonatoAAtualizar.FormatoCampeonato = Campeonato.FormatoCampeonato;
        campeonatoAAtualizar.LocaisDosJogos = Campeonato.LocaisDosJogos;
        campeonatoAAtualizar.DataInicio = Campeonato.DataInicio;
        campeonatoAAtualizar.DataFim = Campeonato.DataFim;
        campeonatoAAtualizar.HaveraPremiacao = Campeonato.HaveraPremiacao;
        campeonatoAAtualizar.LogoUrl = Campeonato.LogoUrl;
        campeonatoAAtualizar.NumeroMaximoEquipes = numeroEquipes;
        campeonatoAAtualizar.ValorTaxaInscricao = valorTaxa;

        try {
            var usuarioLogado = _sessaoService.GetUsuarioAtual();

            if (usuarioLogado?.IdServidor.HasValue == true) {
                campeonatoAAtualizar.OrganizadorId = usuarioLogado.IdServidor.Value;
            }

            int result = await _campeonatoService.AtualizarAsync(campeonatoAAtualizar);

            if (result > 0) {
                await Application.Current.MainPage.DisplayAlert("Sucesso", "Campeonato atualizado com sucesso!", "OK");
            } else {
                await Application.Current.MainPage.DisplayAlert("Atenção", "O campeonato não foi atualizado. Nenhuma alteração persistiu (0 linhas afetadas).", "OK");
            }

            await Shell.Current.GoToAsync("..");

        } catch (Exception ex) {
            //Debug.WriteLine($"[EditarCampeonatoViewModel] Erro ao salvar campeonato: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Erro", $"Falha ao salvar o campeonato: {ex.Message}", "OK");
        } finally {
            IsBusy = false;
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
            //Debug.WriteLine($"Erro ao selecionar logo: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Erro", $"Falha ao selecionar imagem: {ex.Message}", "OK");
        }
    }
}