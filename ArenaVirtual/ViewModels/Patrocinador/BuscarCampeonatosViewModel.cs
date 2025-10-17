using ArenaVirtual.Models;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ArenaVirtual.ViewModels.Patrocinador {

    public partial class BuscarCampeonatosViewModel : ObservableObject {

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CarregarCampeonatosCommand))]
        private bool isBusy;

        private bool IsNotBusy => !IsBusy;

        private readonly CampeonatoService _campeonatoService;

        [ObservableProperty]
        private ObservableCollection<CampeonatoPatrocinioItemViewModel> campeonatosDisponiveis;

        public BuscarCampeonatosViewModel(CampeonatoService campeonatoService) {
            _campeonatoService = campeonatoService;
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

                var campeonatosFiltrados = string.IsNullOrWhiteSpace(query)
                    ? todosCampeonatos
                    : todosCampeonatos.Where(c => c.Nome.ToLower().Contains(query.ToLower())).ToList();

                var novosItens = new List<CampeonatoPatrocinioItemViewModel>();

                foreach (var campeonato in campeonatosFiltrados) {
                    // **TO-DO:** Adicionar lógica de verificação se o Patrocinador JÁ tem proposta
                    // ou patrocínio ACEITO neste campeonato, e atualizar ButtonText/ButtonColor.
                    novosItens.Add(new CampeonatoPatrocinioItemViewModel(campeonato));
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
            if (item == null) return;
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