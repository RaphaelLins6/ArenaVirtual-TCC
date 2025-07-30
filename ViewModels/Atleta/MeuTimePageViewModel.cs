using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ArenaVirtual.Views.Atleta; // Certifique-se de que este using está correto
using ArenaVirtual.Services;
using ArenaVirtual.Models;

namespace ArenaVirtual.ViewModels.Atleta;

public partial class MeuTimePageViewModel : INotifyPropertyChanged {
    public class TimeModel {
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public ObservableCollection<MembroModel> Membros { get; set; } = [];
    }

    public class MembroModel {
        public string Nome { get; set; }
    }

    private TimeModel _time = new TimeModel();
    public TimeModel Time {
        get => _time;
        set {
            _time = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(VinculadoATime));
            OnPropertyChanged(nameof(NaoVinculadoATime));
        }
    }

    public bool VinculadoATime => SessaoService.Instancia.GetUsuarioAtual()?.TimeId != null;
    public bool NaoVinculadoATime => !VinculadoATime;

    public ICommand ProcurarTimesCommand { get; }
    public ICommand CriarMeuTimeCommand { get; }
    public ICommand VerDetalhesTimeCommand { get; }

    private readonly TimeService _timeService;

    public MeuTimePageViewModel(TimeService timeService) { // <--- ALteração aqui
        _timeService = timeService; // <--- ALteração aqui
        ProcurarTimesCommand = new Command(() => Shell.Current.GoToAsync("ProcurarTimePage"));
        CriarMeuTimeCommand = new Command(async () => {
            await Shell.Current.Navigation.PushAsync(new CriarTimePage(_timeService));
        });
        VerDetalhesTimeCommand = new Command(() => Shell.Current.GoToAsync("TimeDetailPage"));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}