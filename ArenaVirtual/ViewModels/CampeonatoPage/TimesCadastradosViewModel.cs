using System.Collections.ObjectModel;
using ArenaVirtual.Models;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Collections.Generic; // Para IDictionary
using System; // Para Exception e Console.WriteLine

namespace ArenaVirtual.ViewModels.CampeonatoPage {
    // Adicionei ": ObservableObject" para garantir o funcionamento do MVVM Toolkit
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

        // 1. Recebe o ID do campeonato
        public void ApplyQueryAttributes(IDictionary<string, object> query) {
            if (query.ContainsKey("CampeonatoId")) {
                _campeonatoId = (int)query["CampeonatoId"];

                // Garante que a operação async rode na thread principal para evitar erros de UI
                MainThread.BeginInvokeOnMainThread(async () => await LoadTimesAsync());
            }
        }

        // 2. Comando para carregar os times aceitos
        [RelayCommand]
        private async Task LoadTimesAsync() {
            try {
                Times.Clear();

                // Chama o serviço que busca os times filtrando pelo StatusInscricao.Aceita no DB
                var timesAceitos = await _campeonatoService.GetTimesAceitos(_campeonatoId);

                foreach (var time in timesAceitos) {
                    Times.Add(time);
                }
            } catch (Exception ex) {
                // Em um app real, aqui você usaria um serviço de alerta ou log.
                Console.WriteLine($"Erro ao carregar times: {ex.Message}");
                // Opcional: Mostrar uma mensagem amigável para o usuário.
            }
        }
    }
}