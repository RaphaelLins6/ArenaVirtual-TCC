using ArenaVirtual.Models;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ArenaVirtual.ViewModels.Patrocinador {

    public partial class BuscarCampeonatosViewModel : ObservableObject {

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CarregarCampeonatosCommand))]
        private bool isBusy;

        private bool IsNotBusy => !IsBusy;

        private readonly CampeonatoService _campeonatoService;
        private readonly PatrocinioService _patrocinioService; 

        [ObservableProperty]
        private ObservableCollection<CampeonatoPatrocinioItemViewModel> campeonatosDisponiveis;

        public BuscarCampeonatosViewModel(CampeonatoService campeonatoService, PatrocinioService patrocinioService) {
            _campeonatoService = campeonatoService;
            _patrocinioService = patrocinioService; 
            CampeonatosDisponiveis = new ObservableCollection<CampeonatoPatrocinioItemViewModel>();
        }

        public async Task OnAppearingAsync() {
            await CarregarCampeonatosAsync(string.Empty);
        }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        public async Task CarregarCampeonatosAsync(string query) {
            if (IsBusy) return;

            IsBusy = true;

            try {
                var todosCampeonatos = await _campeonatoService.ObterTodosAsync();

                var propostasPatrocinador = await _patrocinioService.ObterPropostasDoPatrocinadorAsync();

                var campeonatosFiltrados = string.IsNullOrWhiteSpace(query)
                    ? todosCampeonatos
                    : todosCampeonatos.Where(c => c.Nome.ToLower().Contains(query.ToLower())).ToList();

                var novosItens = new List<CampeonatoPatrocinioItemViewModel>();

                foreach (var campeonato in campeonatosFiltrados) {
                    var itemViewModel = new CampeonatoPatrocinioItemViewModel(campeonato);

                    var proposta = propostasPatrocinador.FirstOrDefault(
                        p => p.CampeonatoId == campeonato.Id); 

                    if (proposta != null) {
                        if (proposta.Aprovada) {
                            itemViewModel.StatusAtual = PatrocinioStatus.Aceito;
                        } else {
                            itemViewModel.StatusAtual = PatrocinioStatus.Pendente;
                        }
                    }

                    novosItens.Add(itemViewModel);
                }

                MainThread.BeginInvokeOnMainThread(() => {
                    CampeonatosDisponiveis.Clear();
                    foreach (var item in novosItens) {
                        CampeonatosDisponiveis.Add(item);
                    }
                });

            } catch (Exception ex) {
                //Debug.WriteLine($"[CarregarCampeonatosAsync - Patrocinador] ERRO: {ex.Message}");
            } finally {
                IsBusy = false;
            }
        }

        public async Task NavegarParaPropostaPublicAsync(CampeonatoPatrocinioItemViewModel item) {
            if (item == null || item.StatusAtual != PatrocinioStatus.Disponivel) return;

            //Debug.WriteLine($"[Public Clicked] CLIQUE RECEBIDO: {item.Nome}");
            var route = $"PropostaCampeonatoPage?campeonatoId={item.Campeonato.ClientAppId}";
            await Shell.Current.GoToAsync(route);
        }

        [RelayCommand]
        public async Task NavegarParaPropostaAsync(CampeonatoPatrocinioItemViewModel item) {
            await NavegarParaPropostaPublicAsync(item);
        }
    }
}