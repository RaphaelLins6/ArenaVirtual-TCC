using ArenaVirtual.Models;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using System.Threading.Tasks;

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
            // CORREÇÃO: Usar a propriedade correta 'TimeClientAppId'
            if (usuarioAtual?.TimeClientAppId != null) {
                // CORREÇÃO: Usar o método de busca por ClientAppId
                _time = await _timeService.ObterPorClientAppIdAsync(usuarioAtual.TimeClientAppId.Value);
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

            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        public async Task UploadLogo() {
            var result = await FilePicker.PickAsync(new PickOptions {
                PickerTitle = "Selecione a logo do time",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null) {
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
                // CORREÇÃO: Usar _time.ClientAppId que é do tipo Guid
                await _timeService.ExcluirTimeAsync(_time.ClientAppId);
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