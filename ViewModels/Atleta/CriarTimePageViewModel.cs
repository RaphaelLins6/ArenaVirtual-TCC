using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ArenaVirtual.Services;
using ArenaVirtual.Views.Atleta;

namespace ArenaVirtual.ViewModels.Atleta {
    public partial class CriarTimePageViewModel : INotifyPropertyChanged {
        private readonly TimeService _timeService;
        private string _nome;
        private string _descricao;
        private string _logoImagem;
        private string _corUniforme;

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
            _ = await FilePicker.PickAsync(new PickOptions {
                PickerTitle = "Selecione uma imagem de logo",
                FileTypes = FilePickerFileType.Images
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}