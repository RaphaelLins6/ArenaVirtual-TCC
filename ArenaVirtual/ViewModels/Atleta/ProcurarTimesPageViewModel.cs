using ArenaVirtual.Models;
using ArenaVirtual.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace ArenaVirtual.ViewModels.Atleta {
    public partial class ProcurarTimesPageViewModel : INotifyPropertyChanged {
        private readonly TimeService _timeService;

        public ObservableCollection<Time> Times { get; set; } = [];
        public ICommand EntrarCommand { get; }

        public ProcurarTimesPageViewModel(TimeService timeService) {
            _timeService = timeService;
            EntrarCommand = new Command<Time>(async (time) => await EntrarNoTime(time));
            CarregarTimes();
        }

        private async void CarregarTimes() {
            var lista = await _timeService.ObterTodosAsync();
            Times.Clear();
            foreach (var t in lista)
                Times.Add(t);
        }

        private async Task EntrarNoTime(Time time) {
            await _timeService.AssociarUsuarioAoTimeAsync(time);
            await Application.Current.MainPage?.DisplayAlert("Sucesso", $"Você entrou no time {time.Nome}", "OK");
            await Shell.Current.GoToAsync("..");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}