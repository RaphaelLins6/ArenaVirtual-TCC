using ArenaVirtual.Models;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;

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

            CarregarDadosOrganizador();

            PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(Nome) || e.PropertyName == nameof(Local) ||
                    e.PropertyName == nameof(NomeOrganizador) || e.PropertyName == nameof(EmailOrganizador) ||
                    e.PropertyName == nameof(DataFim) || e.PropertyName == nameof(DataInicio) ||
                    e.PropertyName == nameof(NumeroMaximoEquipesTexto) || e.PropertyName == nameof(ValorTaxaInscricaoTexto)) {
                    SalvarCampeonatoCommand.NotifyCanExecuteChanged();
                }
            };
        }

        private void CarregarDadosOrganizador() {
            var usuarioLogado = SessaoService.Instancia.GetUsuarioAtual();

            if (usuarioLogado != null) {
                if (string.IsNullOrWhiteSpace(NomeOrganizador)) {
                    NomeOrganizador = usuarioLogado.Nome ?? string.Empty;
                }
                if (string.IsNullOrWhiteSpace(EmailOrganizador)) {
                    EmailOrganizador = usuarioLogado.Email ?? string.Empty;
                }
                if (string.IsNullOrWhiteSpace(TelefoneOrganizador)) {
                    TelefoneOrganizador = usuarioLogado.Telefone ?? string.Empty;
                }
            }
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

            return isValid;
        }

        private async Task SalvarCampeonatoAsync() {
            if (!CanSalvarCampeonato()) {
                MensagemValidacao = "Preencha todos os campos obrigatórios corretamente.";
                return;
            }

            var usuarioLogado = SessaoService.Instancia.GetUsuarioAtual();
            if (usuarioLogado == null || usuarioLogado.Id == 0) {
                MensagemValidacao = "Erro: O usuário organizador não está logado ou não foi sincronizado. Por favor, tente novamente.";
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
                NumeroMaximoEquipes = int.Parse(NumeroMaximoEquipesTexto),
                ValorTaxaInscricao = decimal.Parse(ValorTaxaInscricaoTexto, CultureInfo.InvariantCulture),
                FormatoCampeonato = FormatoCampeonato,
                LocaisDosJogos = LocaisDosJogos,
                HaveraPremiacao = HaveraPremiacao,
                OrganizadorId = usuarioLogado.Id,
                OrganizadorClientAppId = usuarioLogado.ClientAppId
            };

            try {
                await _databaseService.InserirCampeonatoAsync(novoCampeonato);
                MensagemValidacao = "Campeonato criado com sucesso!";
                LimparCampos();
                await Shell.Current.GoToAsync("..");
            } catch (Exception ex) {
                //Debug.WriteLine($"Erro ao criar campeonato: {ex.Message}");
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

            NumeroMaximoEquipesTexto = string.Empty;
            ValorTaxaInscricaoTexto = string.Empty;
            FormatoCampeonato = string.Empty;
            LocaisDosJogos = string.Empty;
            HaveraPremiacao = false;
            LogoImageSource = null;
            MensagemValidacao = string.Empty;

            CarregarDadosOrganizador();

            SalvarCampeonatoCommand.NotifyCanExecuteChanged();
        }
    }
}