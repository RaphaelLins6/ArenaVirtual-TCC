using ArenaVirtual.Models;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ArenaVirtual.ViewModels.Atleta {
    public partial class EditarTimePageViewModel : ObservableObject {

        private readonly TimeService _timeService;
        private readonly UsuarioService _usuarioService;
        private Time _time;

        [ObservableProperty]
        private string nomeTime;

        [ObservableProperty]
        private string descricaoTime;

        [ObservableProperty]
        private ImageSource logoImageSource;

        public EditarTimePageViewModel(TimeService timeService, UsuarioService usuarioService) {
            _timeService = timeService;
            _usuarioService = usuarioService;
        }

        [RelayCommand]
        public async Task LoadDataAsync() {
            var usuarioAtual = SessaoService.Instancia.GetUsuarioAtual();
            if (usuarioAtual?.TimeId != null) {
                _time = await _timeService.ObterPorIdAsync(usuarioAtual.TimeId.Value);
                if (_time != null) {
                    NomeTime = _time.Nome;
                    DescricaoTime = _time.Descricao;
                    LogoImageSource = GetImageSourceFromFile(_time.LogoUrl);
                }
            }
        }

        [RelayCommand]
        public async Task SalvarAlteracoes() {
            if (_time == null) return;

            _time.Nome = NomeTime;
            _time.Descricao = DescricaoTime;

            await _timeService.AtualizarTimeAsync(_time);
            await Shell.Current.DisplayAlert("Sucesso", "Alterações salvas com sucesso!", "OK");

            // Retorna para a página anterior
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        public async Task UploadLogo() {
            var result = await FilePicker.PickAsync(new PickOptions {
                PickerTitle = "Selecione a logo do time",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null) {
                // Lógica para salvar a imagem e atualizar o LogoUrl
                string logoPath = Path.Combine(FileSystem.AppDataDirectory, result.FileName);
                using (var stream = await result.OpenReadAsync())
                using (var fileStream = File.OpenWrite(logoPath)) {
                    await stream.CopyToAsync(fileStream);
                }

                _time.LogoUrl = logoPath;
                LogoImageSource = ImageSource.FromStream(() => File.OpenRead(logoPath));
            }
        }

        [RelayCommand]
        public async Task ExcluirTime() {
            bool confirm = await Shell.Current.DisplayAlert("Atenção", "Tem certeza que deseja excluir o time? Essa ação é irreversível.", "Sim", "Não");
            if (confirm && _time != null) {
                await _timeService.ExcluirTimeAsync(_time.Id);
                await Shell.Current.GoToAsync("//MeusTimesPage");
            }
        }

        private ImageSource GetImageSourceFromFile(string filePath) {
            if (string.IsNullOrEmpty(filePath)) {
                return ImageSource.FromFile("placeholder.png");
            }

            try {
                if (File.Exists(filePath)) {
                    return ImageSource.FromStream(() => File.OpenRead(filePath));
                } else {
                    return ImageSource.FromFile("placeholder.png");
                }
            } catch {
                return ImageSource.FromFile("placeholder.png");
            }
        }
    }
}