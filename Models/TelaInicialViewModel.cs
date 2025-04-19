using System.Collections.ObjectModel;
using System.ComponentModel;

public class TelaInicialViewModel : INotifyPropertyChanged {
    private ObservableCollection<string> _favoritos = new ObservableCollection<string>();
    public ObservableCollection<string> Favoritos {
        get => _favoritos;
        set {
            _favoritos = value;
            OnPropertyChanged(nameof(Favoritos));
        }
    }

    public TelaInicialViewModel() {
        Favoritos.Add("Campeonato 1");
        Favoritos.Add("Campeonato 2");
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}