using System.Collections.ObjectModel;
using ArenaVirtual.Models;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics; 

namespace ArenaVirtual.ViewModels.CampeonatoPage {
    public partial class TimesCadastradosViewModel : ObservableObject, IQueryAttributable {
        private readonly CampeonatoService _campeonatoService;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasTimes))]
        private ObservableCollection<Time> times;

        private int _campeonatoId;

        private Guid _campeonatoClientAppId;

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

                var campeonato = await _campeonatoService.ObterPorIdAsync(_campeonatoId);
                if (campeonato == null) return;

                _campeonatoClientAppId = campeonato.ClientAppId;
                //Debug.WriteLine($"[DEBUG LOAD] CampeonatoClientAppId carregado: {_campeonatoClientAppId}");

                var timesAceitos = await _campeonatoService.GetTimesAceitos(_campeonatoId);

                foreach (var time in timesAceitos) {
                    Times.Add(time);
                }
            } catch (Exception ex) {
                //Debug.WriteLine($"Erro ao carregar times: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task RemoverTime(Time timeParaRemover) {
            if (timeParaRemover == null) return;

            Guid campClientAppIdParaDelecao = _campeonatoClientAppId;

            if (campClientAppIdParaDelecao == Guid.Empty) {
                //Debug.WriteLine("[DEBUG REMOCAO] ERRO: CampeonatoClientAppId do VM está vazio. Não é possível remover.");
                await Application.Current.MainPage.DisplayAlert("Erro", "Falha ao obter o identificador do campeonato.", "OK");
                return;
            }

            //Debug.WriteLine($"[DEBUG REMOCAO] Tentando remover Time ID: {timeParaRemover.Id}");
            //Debug.WriteLine($"[DEBUG REMOCAO] TimeClientAppId: {timeParaRemover.ClientAppId}");
            //Debug.WriteLine($"[DEBUG REMOCAO] CampeonatoClientAppId **CORRETO** USADO: {campClientAppIdParaDelecao}");

            try {
                await _campeonatoService.RemoverTimeDoCampeonato(
                    _campeonatoId,
                    timeParaRemover.Id,
                    timeParaRemover.ClientAppId,
                    campClientAppIdParaDelecao
                );

                Times.Remove(timeParaRemover);
                //Debug.WriteLine($"Time {timeParaRemover.Nome} removido com sucesso!");

            } catch (Exception ex) {
                //Debug.WriteLine($"Erro ao remover time: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Erro", "Não foi possível remover o time.", "OK");
            }
        }
    }
}