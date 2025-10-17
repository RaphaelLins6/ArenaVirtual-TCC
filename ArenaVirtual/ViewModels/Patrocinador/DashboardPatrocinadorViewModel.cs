using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ArenaVirtual.ViewModels.Patrocinador {

    // ⭐️ Criar um ViewModel Simples para a lista (substitui JogoDetalheViewModel) ⭐️
    public partial class CampanhaAtivaViewModel : ObservableObject {
        public int CampanhaId { get; set; }
        public string NomeCampanha { get; set; }
        public string NomeCampeonato { get; set; }
        public string LogotipoCampeonatoUrl { get; set; }
        public string DataFim { get; set; }
        public string Status { get; set; } // Ex: "Ativa", "Pendente", "Finalizada"
    }

    public partial class DashboardPatrocinadorViewModel : ObservableObject {

        // ⭐️ Propriedades para o MVVM (Observable Collections) ⭐️
        [ObservableProperty]
        private ObservableCollection<CampanhaAtivaViewModel> campanhasAtivas = new();

        [ObservableProperty]
        private bool estaOcupado; // Para o ActivityIndicator e RefreshView

        // ⭐️ Comandos para Ações e Carregamento ⭐️

        public DashboardPatrocinadorViewModel() {
            // Iniciar o carregamento das campanhas ativas ao criar o VM
            _ = LoadCampanhasCommand.ExecuteAsync(null);
        }

        // Comando para Navegar para a Criação de Campanha
        [RelayCommand]
        private async Task NavegarParaCriacaoCampanha() {
            // TODO: Implementar a navegação para a página de criação
            await Shell.Current.GoToAsync("CriarCampanhaPage");
        }

        // Comando para Navegar para a Busca de Campeonatos (Para padrinhar)
        [RelayCommand]
        private async Task NavegarParaBuscaCampeonatos() {
            // TODO: Implementar a navegação para a busca de campeonatos
            await Shell.Current.GoToAsync("BuscarCampeonatosPage");
        }

        // Comando para Carregar as Campanhas Ativas (Substitui LoadPartidasCommand)
        [RelayCommand]
        private async Task LoadCampanhas() {
            if (EstaOcupado) return;
            EstaOcupado = true;

            // Simulação de carregamento de dados
            await Task.Delay(1500);

            // TODO: Chamar o serviço de Patrocínio para obter as Campanhas Ativas do usuário logado

            // Exemplo de dados (remover após implementação do serviço real)
            CampanhasAtivas.Clear();
            CampanhasAtivas.Add(new CampanhaAtivaViewModel {
                CampanhaId = 1,
                NomeCampanha = "Patrocínio Copa Arena",
                NomeCampeonato = "Copa Arena Virtual 2025",
                DataFim = "31/12/2025",
                Status = "Ativa",
                LogotipoCampeonatoUrl = "logo_campeonato1.png"
            });
            CampanhasAtivas.Add(new CampanhaAtivaViewModel {
                CampanhaId = 2,
                NomeCampanha = "Divulgação Lançamento",
                NomeCampeonato = "Liga Universitária 2024",
                DataFim = "15/11/2024",
                Status = "Finalizada",
                LogotipoCampeonatoUrl = "logo_campeonato2.png"
            });

            EstaOcupado = false;
        }

        // Comando para visualizar os detalhes de uma campanha
        [RelayCommand]
        private async Task NavegarParaDetalhesCampanha(CampanhaAtivaViewModel campanha) {
            if (campanha == null) return;
            // TODO: Implementar a navegação
            await Shell.Current.GoToAsync($"DetalheCampanhaPage?Id={campanha.CampanhaId}");
        }
    }
}