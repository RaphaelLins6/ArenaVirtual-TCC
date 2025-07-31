using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ArenaVirtual.Services;

namespace ArenaVirtual.ViewModels.Atleta {
    public partial class CriarTimePageViewModel : INotifyPropertyChanged {
        private readonly TimeService _timeService;
        private string _nome;
        private string _descricao;
        private string _logoImagem;
        private string _corUniforme;
        private string _corSecundaria;
        private string _nomeResponsavel;
        private string _telefoneResponsavel;

        public string Nome {
            get => _nome;
            set {
                _nome = value;
                OnPropertyChanged();
            }
        }

        public string Descricao {
            get => _descricao;
            set {
                _descricao = value;
                OnPropertyChanged();
            }
        }

        public string LogoImagem {
            get => _logoImagem;
            set {
                _logoImagem = value;
                OnPropertyChanged();
            }
        }

        public string CorUniforme {
            get => _corUniforme;
            set {
                _corUniforme = value;
                OnPropertyChanged();
            }
        }

        public string CorSecundaria {
            get => _corSecundaria;
            set {
                _corSecundaria = value;
                OnPropertyChanged();
            }
        }

        public string NomeResponsavel {
            get => _nomeResponsavel;
            set {
                _nomeResponsavel = value;
                OnPropertyChanged();
            }
        }

        public string TelefoneResponsavel {
            get => _telefoneResponsavel;
            set {
                _telefoneResponsavel = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> CoresDisponiveis { get; } =
        [
            "Vermelho", "Azul", "Verde", "Amarelo", "Preto", "Branco", "Laranja"
        ];

        public ICommand CriarTimeCommand { get; }
        public ICommand SelecionarLogoCommand { get; }

        public CriarTimePageViewModel(TimeService timeService) {
            _timeService = timeService;
            CriarTimeCommand = new Command(async () => await CriarTime());
            SelecionarLogoCommand = new Command(ExecutarSelecionarLogo);

            CarregarDadosUsuario();
        }

        private async Task CriarTime() {
            if (string.IsNullOrWhiteSpace(Nome)) {
                await Application.Current.MainPage.DisplayAlert("Erro", "O nome do time é obrigatório.", "OK");
                return;
            }

            var resultado = await _timeService.CriarTimeEAssociarUsuarioAsync(Nome, Descricao);

            if (resultado > 0) {
                await Application.Current.MainPage.DisplayAlert("Sucesso", $"Time '{Nome}' criado com sucesso!", "OK");
                await Shell.Current.GoToAsync("..");
            } else {
                await Application.Current.MainPage.DisplayAlert("Erro", "Não foi possível criar o time. Tente novamente.", "OK");
            }
        }

        private async void ExecutarSelecionarLogo() {
            try {
                var result = await FilePicker.PickAsync(new PickOptions {
                    PickerTitle = "Selecione uma imagem de logo",
                    FileTypes = FilePickerFileType.Images
                });

                if (result != null) {
                    LogoImagem = result.FullPath;
                }
            } catch (Exception ex) {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Não foi possível selecionar a imagem: {ex.Message}", "OK");
            }
        }

        private void CarregarDadosUsuario() {
            var usuarioAtual = SessaoService.Instancia.GetUsuarioAtual();
            if (usuarioAtual != null) {
                NomeResponsavel = usuarioAtual.Nome;
                TelefoneResponsavel = usuarioAtual.Telefone;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}