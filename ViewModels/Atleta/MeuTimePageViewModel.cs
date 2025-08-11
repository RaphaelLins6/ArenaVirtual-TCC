using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ArenaVirtual.Services;
using ArenaVirtual.Views.Atleta;

namespace ArenaVirtual.ViewModels.Atleta;

public partial class MeuTimePageViewModel : INotifyPropertyChanged {
    public class TimeModel {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string Logo { get; set; }
        public ObservableCollection<MembroModel> Membros { get; set; } = [];
    }

    public class MembroModel {
        public string Nome { get; set; }
        public string Foto { get; set; }
    }

    private TimeModel _time = new TimeModel();
    public TimeModel Time {
        get => _time;
        set {
            if (_time != value) {
                _time = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(VinculadoATime));
                OnPropertyChanged(nameof(NaoVinculadoATime));
            }
        }
    }

    public bool VinculadoATime => SessaoService.Instancia.GetUsuarioAtual()?.TimeId != null;
    public bool NaoVinculadoATime => !VinculadoATime;

    public ICommand CriarMeuTimeCommand { get; }
    public ICommand EntrarTimeCommand { get; }
    public ICommand GerenciarTimeCommand { get; }

    private readonly TimeService _timeService;
    private readonly UsuarioService _usuarioService; 

    public MeuTimePageViewModel(TimeService timeService, UsuarioService usuarioService) {
        _timeService = timeService;
        _usuarioService = usuarioService; 

        GerenciarTimeCommand = new Command(async () => await Shell.Current.GoToAsync("GerenciarTimePage"));
        CriarMeuTimeCommand = new Command(async () => await Shell.Current.Navigation.PushAsync(new CriarTimePage(_timeService)));
        EntrarTimeCommand = new Command(async () => await Shell.Current.GoToAsync("ProcurarTimePage"));

        _ = LoadData();
    }

    public async Task LoadData() {
        var usuarioAtual = SessaoService.Instancia.GetUsuarioAtual();

        if (usuarioAtual != null && usuarioAtual.TimeId != null) {
            var timeDoUsuario = await _timeService.ObterPorIdAsync(usuarioAtual.TimeId.Value);

            if (timeDoUsuario != null) {
                this.Time = new MeuTimePageViewModel.TimeModel {
                    Id = timeDoUsuario.Id,
                    Nome = timeDoUsuario.Nome,
                    Descricao = timeDoUsuario.Descricao,
                    Logo = timeDoUsuario.LogoUrl,
                    Membros = new ObservableCollection<MembroModel>()
                };

                var membros = await _usuarioService.ListarMembrosDoTimeAsync(timeDoUsuario.Id);
                foreach (var membro in membros) {
                    this.Time.Membros.Add(new MembroModel { Nome = membro.Nome, Foto = membro.ImagemPath });
                }
            }
        } else {
            this.Time = new MeuTimePageViewModel.TimeModel();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
