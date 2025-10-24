using ArenaVirtual.Models;
using ArenaVirtual.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;

namespace ArenaVirtual.ViewModels.Atleta {

    public partial class ProcurarTimesPageViewModel : ObservableObject {

        private readonly TimeService _timeService;

        [ObservableProperty]
        public ObservableCollection<Time> times = [];

        [ObservableProperty]
        public bool isBusy;

        public ProcurarTimesPageViewModel(TimeService timeService) {
            _timeService = timeService;
        }

        [RelayCommand]
        public async Task CarregarTimesAsync() {
            if (IsBusy)
                return;

            try {
                IsBusy = true;
                var lista = await _timeService.ObterTodosAsync();

                MainThread.BeginInvokeOnMainThread(() => {
                    Times.Clear();
                    foreach (var t in lista) {
                        Times.Add(t);
                    }
                });
            } catch (Exception ex) {
                //Debug.WriteLine($"[ProcurarTimesPageViewModel] Erro ao carregar times: {ex.Message}");
            } finally {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task EntrarNoTimeAsync(Time time) {
            if (time == null)
                return;

            try {
                await _timeService.SolicitarEntradaNoTimeAsync(time);
                await Application.Current.MainPage?.DisplayAlert("Sucesso", $"Você entrou no time {time.Nome}", "OK");
                await Shell.Current.GoToAsync("..");
            } catch (Exception ex) {
                //Debug.WriteLine($"[ProcurarTimesPageViewModel] Erro ao entrar no time: {ex.Message}");
                await Application.Current.MainPage?.DisplayAlert("Erro", "Não foi possível entrar no time. Tente novamente.", "OK");
            }
        }
    }
}