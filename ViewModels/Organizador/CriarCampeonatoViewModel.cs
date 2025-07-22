using ArenaVirtual.Models;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel; 
using MvvmHelpers.Commands;
using MvvmHelpers.Interfaces;
using System.Diagnostics;

namespace ArenaVirtual.ViewModels {
    public partial class CriarCampeonatoViewModel : ObservableObject {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        private string nome;

        [ObservableProperty]
        private string local;

        [ObservableProperty]
        private DateTime dataInicio = DateTime.Today; 

        [ObservableProperty]
        private DateTime dataFim = DateTime.Today.AddDays(7); 

        [ObservableProperty]
        private string logoUrl; 

        [ObservableProperty]
        private string nomeOrganizador; 

        [ObservableProperty]
        private string emailOrganizador; 

        [ObservableProperty]
        private string telefoneOrganizador; 

        [ObservableProperty]
        private int numeroMaximoEquipes; 

        [ObservableProperty]
        private decimal valorTaxaInscricao; 
        [ObservableProperty]
        private string formatoCampeonato; 

        [ObservableProperty]
        private string locaisDosJogos; 

        [ObservableProperty]
        private bool haveraPremiacao; 

        [ObservableProperty]
        private ImageSource logoImageSource; 

        public IAsyncCommand SalvarCampeonatoCommand { get; } 
        public IAsyncCommand SelecionarLogoCommand { get; }

        public CriarCampeonatoViewModel(DatabaseService databaseService) {
            _databaseService = databaseService;
            SalvarCampeonatoCommand = new AsyncCommand(SalvarCampeonato);
            SelecionarLogoCommand = new AsyncCommand(SelecionarLogoAsync);
        }

        private async Task SalvarCampeonato() {
            if (string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(Local) || string.IsNullOrWhiteSpace(NomeOrganizador) || string.IsNullOrWhiteSpace(EmailOrganizador)) {
                await Application.Current.MainPage.DisplayAlert("Erro", "Preencha todos os campos obrigatórios.", "OK");
                return;
            }

            if (DataFim < DataInicio) {
                await Application.Current.MainPage.DisplayAlert("Erro", "A data final não pode ser anterior à data inicial.", "OK");
                return;
            }

            if (NumeroMaximoEquipes <= 0) {
                await Application.Current.MainPage.DisplayAlert("Erro", "O número máximo de equipes deve ser maior que zero.", "OK");
                return;
            }

            if (ValorTaxaInscricao < 0) {
                await Application.Current.MainPage.DisplayAlert("Erro", "O valor da taxa de inscrição não pode ser negativo.", "OK");
                return;
            }

            var novoCampeonato = new Campeonato {
                Nome = Nome,
                Local = Local,
                DataInicio = DataInicio,
                DataFim = DataFim,
                LogoUrl = LogoUrl,
                NomeOrganizador = NomeOrganizador,
                EmailOrganizador = EmailOrganizador,
                TelefoneOrganizador = TelefoneOrganizador,
                NumeroMaximoEquipes = NumeroMaximoEquipes,
                ValorTaxaInscricao = ValorTaxaInscricao,
                FormatoCampeonato = FormatoCampeonato,
                LocaisDosJogos = LocaisDosJogos,
                HaveraPremiacao = HaveraPremiacao,
                OrganizadorId = App.CurrentUser?.Id ?? 0 // Use o ID do usuário logado
            };

            try {
                await _databaseService.InserirCampeonatoAsync(novoCampeonato);
                await Application.Current.MainPage.DisplayAlert("Sucesso", "Campeonato criado com sucesso!", "OK");
                LimparCampos(); 
            } catch (Exception ex) {
                Debug.WriteLine($"Erro ao criar campeonato: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Erro", $"Falha ao criar campeonato: {ex.Message}", "OK");
            }
        }

        private async Task SelecionarLogoAsync() {
            try {
                var result = await FilePicker.PickAsync(new PickOptions {
                    PickerTitle = "Selecione a logo do campeonato",
                    FileTypes = FilePickerFileType.Images
                });

                if (result != null) {
                    LogoUrl = result.FullPath; // Salva o caminho para persistência
                    LogoImageSource = ImageSource.FromFile(result.FullPath); // Atualiza para exibição
                }
            } catch (Exception ex) {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Não foi possível selecionar a imagem: {ex.Message}", "OK");
            }
        }

        public void LimparCampos()
        {
            Nome = string.Empty;
            Local = string.Empty;
            DataInicio = DateTime.Today;
            DataFim = DateTime.Today.AddDays(7);
            LogoUrl = string.Empty;
            NomeOrganizador = string.Empty;
            EmailOrganizador = string.Empty;
            TelefoneOrganizador = string.Empty;
            NumeroMaximoEquipes = 0;
            ValorTaxaInscricao = 0;
            FormatoCampeonato = string.Empty;
            LocaisDosJogos = string.Empty;
            HaveraPremiacao = false;
            LogoImageSource = null;
        }
    }
}