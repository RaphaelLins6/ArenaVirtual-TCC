using System.Collections.ObjectModel;
using ArenaVirtual.Models;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ArenaVirtual.ViewModels.CampeonatoPage {
    public partial class TimesCadastradosViewModel : ObservableObject, IQueryAttributable {
        private readonly CampeonatoService _campeonatoService;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasTimes))]
        private ObservableCollection<Time> times;

        private int _campeonatoId;

        public bool HasTimes => Times?.Count > 0;

        public TimesCadastradosViewModel(CampeonatoService campeonatoService) {
            _campeonatoService = campeonatoService;
            Times = new ObservableCollection<Time>();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query) {
            if (query.ContainsKey("CampeonatoId")) {
                _campeonatoId = (int)query["CampeonatoId"];

                MainThread.BeginInvokeOnMainThread(async () => await LoadTimesAsync());
            }
        }

        [RelayCommand]
        private async Task LoadTimesAsync() {
            try {
                Times.Clear();

                var timesAceitos = await _campeonatoService.GetTimesAceitos(_campeonatoId);

                foreach (var time in timesAceitos) {
                    Times.Add(time);
                }
            } catch (Exception ex) {
                Console.WriteLine($"Erro ao carregar times: {ex.Message}");
            }
        }
    }
}