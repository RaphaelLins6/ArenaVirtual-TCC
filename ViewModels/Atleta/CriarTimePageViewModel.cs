using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ArenaVirtual.Services;
using ArenaVirtual.Models; 

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
            SelecionarLogoCommand = new Command(async () => await ExecutarSelecionarLogo());

            CarregarDadosUsuario();
        }

        private async Task CriarTime() {
            if (string.IsNullOrWhiteSpace(Nome)) {
                await Application.Current.MainPage.DisplayAlert("Erro", "O nome do time é obrigatório.", "OK");
                return;
            }

            var novoTime = new Time {
                Nome = Nome,
                Descricao = Descricao,
                LogoUrl = LogoImagem,
            };

            var resultado = await _timeService.CriarTimeEAssociarUsuarioAsync(novoTime);

            if (resultado > 0) {
                await Application.Current.MainPage.DisplayAlert("Sucesso", $"Time '{Nome}' criado com sucesso!", "OK");
                await Shell.Current.GoToAsync("..");
            } else {
                await Application.Current.MainPage.DisplayAlert("Erro", "Não foi possível criar o time. Tente novamente.", "OK");
            }
        }


        private async Task ExecutarSelecionarLogo() {
            try {
                var result = await FilePicker.PickAsync(new PickOptions {
                    PickerTitle = "Selecione uma imagem de logo",
                    FileTypes = FilePickerFileType.Images
                });

                if (result != null) {
                    var newFileName = Guid.NewGuid().ToString() + Path.GetExtension(result.FileName);
                    var newFilePath = Path.Combine(FileSystem.AppDataDirectory, newFileName);

                    using (var stream = await result.OpenReadAsync())
                    using (var newStream = File.OpenWrite(newFilePath)) {
                        await stream.CopyToAsync(newStream);
                    }

                    LogoImagem = newFilePath;
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