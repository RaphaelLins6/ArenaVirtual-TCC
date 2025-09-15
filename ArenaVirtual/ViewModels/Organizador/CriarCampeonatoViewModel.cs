using ArenaVirtual.Models;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Input;

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

        // Propriedades corrigidas para lidar com a entrada de texto do XAML
        [ObservableProperty]
        private string numeroMaximoEquipesTexto;

        [ObservableProperty]
        private string valorTaxaInscricaoTexto;

        [ObservableProperty]
        private string formatoCampeonato;

        [ObservableProperty]
        private string locaisDosJogos;

        [ObservableProperty]
        private bool haveraPremiacao;

        [ObservableProperty]
        private ImageSource logoImageSource;

        [ObservableProperty]
        private string mensagemValidacao;

        public IAsyncRelayCommand SalvarCampeonatoCommand { get; }
        public IAsyncRelayCommand SelecionarLogoCommand { get; }

        public CriarCampeonatoViewModel(DatabaseService databaseService) {
            _databaseService = databaseService;

            SalvarCampeonatoCommand = new AsyncRelayCommand(SalvarCampeonatoAsync, CanSalvarCampeonato);
            SelecionarLogoCommand = new AsyncRelayCommand(SelecionarLogoAsync);

            // Reage às mudanças nas propriedades para atualizar o estado do comando
            PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(Nome) || e.PropertyName == nameof(Local) ||
                    e.PropertyName == nameof(NomeOrganizador) || e.PropertyName == nameof(EmailOrganizador) ||
                    e.PropertyName == nameof(DataFim) || e.PropertyName == nameof(DataInicio) ||
                    e.PropertyName == nameof(NumeroMaximoEquipesTexto) || e.PropertyName == nameof(ValorTaxaInscricaoTexto)) {
                    SalvarCampeonatoCommand.NotifyCanExecuteChanged();
                }
            };
        }

        private bool CanSalvarCampeonato() {
            int numeroEquipes;
            decimal valorTaxa;

            bool isValid = !string.IsNullOrWhiteSpace(Nome) &&
                           !string.IsNullOrWhiteSpace(Local) &&
                           !string.IsNullOrWhiteSpace(NomeOrganizador) &&
                           !string.IsNullOrWhiteSpace(EmailOrganizador) &&
                           DataFim >= DataInicio &&
                           int.TryParse(numeroMaximoEquipesTexto, out numeroEquipes) && numeroEquipes > 0 &&
                           decimal.TryParse(valorTaxaInscricaoTexto, NumberStyles.Any, CultureInfo.InvariantCulture, out valorTaxa) && valorTaxa >= 0;

            MensagemValidacao = isValid ? string.Empty : "Preencha todos os campos obrigatórios corretamente.";

            return isValid;
        }

        private async Task SalvarCampeonatoAsync() {
            if (App.CurrentUser == null || App.CurrentUser.Id == 0) {
                MensagemValidacao = "Erro: O usuário organizador não está logado. Por favor, faça login.";
                return;
            }

            if (!int.TryParse(NumeroMaximoEquipesTexto, out int numeroEquipes) ||
                !decimal.TryParse(ValorTaxaInscricaoTexto, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal valorTaxa)) {
                MensagemValidacao = "Erro na conversão dos dados numéricos. Por favor, verifique a entrada.";
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
                NumeroMaximoEquipes = numeroEquipes,
                ValorTaxaInscricao = valorTaxa,
                FormatoCampeonato = FormatoCampeonato,
                LocaisDosJogos = LocaisDosJogos,
                HaveraPremiacao = HaveraPremiacao,
                OrganizadorId = App.CurrentUser.Id
            };

            try {
                await _databaseService.InserirCampeonatoAsync(novoCampeonato);
                MensagemValidacao = "Campeonato criado com sucesso!";
                LimparCampos();
            } catch (Exception ex) {
                Debug.WriteLine($"Erro ao criar campeonato: {ex.Message}");
                MensagemValidacao = $"Falha ao criar campeonato: {ex.Message}";
            }
        }

        private async Task SelecionarLogoAsync() {
            try {
                var result = await FilePicker.PickAsync(new PickOptions {
                    PickerTitle = "Selecione a logo do campeonato",
                    FileTypes = FilePickerFileType.Images
                });

                if (result != null) {
                    LogoUrl = result.FullPath;
                    LogoImageSource = ImageSource.FromFile(result.FullPath);
                }
            } catch (Exception ex) {
                MensagemValidacao = $"Não foi possível selecionar a imagem: {ex.Message}";
            }
        }

        public void LimparCampos() {
            Nome = string.Empty;
            Local = string.Empty;
            DataInicio = DateTime.Today;
            DataFim = DateTime.Today.AddDays(7);
            LogoUrl = string.Empty;
            NomeOrganizador = string.Empty;
            EmailOrganizador = string.Empty;
            TelefoneOrganizador = string.Empty;
            // Limpa as novas propriedades de texto
            NumeroMaximoEquipesTexto = string.Empty;
            ValorTaxaInscricaoTexto = string.Empty;
            FormatoCampeonato = string.Empty;
            LocaisDosJogos = string.Empty;
            HaveraPremiacao = false;
            LogoImageSource = null;
            MensagemValidacao = string.Empty;

            // Garante que o estado do botão seja atualizado após a limpeza
            SalvarCampeonatoCommand.NotifyCanExecuteChanged();
        }
    }
}