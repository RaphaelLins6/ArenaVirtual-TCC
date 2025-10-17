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
        private readonly PatrocinioService _patrocinioService; // NOVO: Serviço de Patrocínio

        [ObservableProperty]
        private ObservableCollection<CampeonatoPatrocinioItemViewModel> campeonatosDisponiveis;

        // NOVO: Construtor com PatrocinioService injetado
        public BuscarCampeonatosViewModel(CampeonatoService campeonatoService, PatrocinioService patrocinioService) {
            _campeonatoService = campeonatoService;
            _patrocinioService = patrocinioService; // Inicializa o serviço
            CampeonatosDisponiveis = new ObservableCollection<CampeonatoPatrocinioItemViewModel>();
        }

        // Método que é chamado ao entrar na página
        public async Task OnAppearingAsync() {
            await CarregarCampeonatosAsync(string.Empty);
        }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        public async Task CarregarCampeonatosAsync(string query) {
            if (IsBusy) return;

            IsBusy = true;

            try {
                var todosCampeonatos = await _campeonatoService.ObterTodosAsync();

                // NOVO: Busca todas as propostas do patrocinador logado
                var propostasPatrocinador = await _patrocinioService.ObterPropostasDoPatrocinadorAsync();

                var campeonatosFiltrados = string.IsNullOrWhiteSpace(query)
                    ? todosCampeonatos
                    : todosCampeonatos.Where(c => c.Nome.ToLower().Contains(query.ToLower())).ToList();

                var novosItens = new List<CampeonatoPatrocinioItemViewModel>();

                foreach (var campeonato in campeonatosFiltrados) {
                    var itemViewModel = new CampeonatoPatrocinioItemViewModel(campeonato);

                    // NOVO: Lógica de verificação de status
                    var proposta = propostasPatrocinador.FirstOrDefault(
                        p => p.CampeonatoId == campeonato.Id); // Assumindo que o ID da proposta usa o 'Id' interno do campeonato.

                    if (proposta != null) {
                        // Sua model PropostaPatrocinio usa 'Aprovada' (bool)
                        if (proposta.Aprovada) {
                            // Aprovada = true -> Patrocínio Aceito
                            itemViewModel.StatusAtual = PatrocinioStatus.Aceito;
                        } else {
                            // Aprovada = false -> Proposta Pendente
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
                Debug.WriteLine($"[CarregarCampeonatosAsync - Patrocinador] ERRO: {ex.Message}");
            } finally {
                IsBusy = false;
            }
        }

        // Método público auxiliar para o code-behind (Opção 1)
        public async Task NavegarParaPropostaPublicAsync(CampeonatoPatrocinioItemViewModel item) {
            // Verifica se o botão está habilitado, caso você tenha removido o binding do Command
            if (item == null || item.StatusAtual != PatrocinioStatus.Disponivel) return;

            Debug.WriteLine($"[Public Clicked] CLIQUE RECEBIDO: {item.Nome}");
            var route = $"PropostaCampeonatoPage?campeonatoId={item.Campeonato.ClientAppId}";
            await Shell.Current.GoToAsync(route);
        }

        // O RelayCommand original (mantido por segurança)
        [RelayCommand]
        public async Task NavegarParaPropostaAsync(CampeonatoPatrocinioItemViewModel item) {
            // Apenas para fins de debug, chame o método público para evitar duplicidade de lógica
            await NavegarParaPropostaPublicAsync(item);
        }
    }
}