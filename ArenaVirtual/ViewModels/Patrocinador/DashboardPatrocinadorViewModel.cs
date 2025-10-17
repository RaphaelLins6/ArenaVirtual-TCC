using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ArenaVirtual.ViewModels.Patrocinador {

    // ⭐️ ViewModel Simples para a lista ⭐️
    public partial class CampanhaAtivaViewModel : ObservableObject {
        public int CampanhaId { get; set; }
        public string NomeCampanha { get; set; }
        public string NomeCampeonato { get; set; }
        public string LogotipoCampeonatoUrl { get; set; }
        public string DataFim { get; set; }
        public string Status { get; set; } // Ex: "Ativa", "Pendente", "Finalizada"
    }

    public partial class DashboardPatrocinadorViewModel : ObservableObject {

        // Injeção de dependência e campo privado para o ID da sessão
        private readonly DatabaseService _databaseService;
        private readonly int _patrocinadorIdLogado;

        // ⭐️ Propriedades para o MVVM (Observable Collections) ⭐️
        [ObservableProperty]
        private ObservableCollection<CampanhaAtivaViewModel> campanhasAtivas = new();

        [ObservableProperty]
        private bool estaOcupado; // Para o ActivityIndicator e RefreshView

        // Propriedade para controle da mensagem de lista vazia
        [ObservableProperty]
        private bool listaVazia = false;

        // Construtor com Injeção de Dependência e Obtenção do ID da Sessão
        public DashboardPatrocinadorViewModel(DatabaseService databaseService) {
            // Inicializa o serviço injetado
            _databaseService = databaseService;

            // ⭐️ OBTENDO O ID DO PATROCINADOR DIRETO DA SESSÃO ⭐️
            // O Id é um INT, então usamos o operador Elvis (?.) e o coalescing (?? 0) para segurança.
            // NOTA: Você precisa que o SessaoService e o GetUsuarioAtual() existam.
            _patrocinadorIdLogado = SessaoService.Instancia.GetUsuarioAtual()?.Id ?? 0;

            System.Diagnostics.Debug.WriteLine($"[DASHBOARD VM] Patrocinador ID da Sessão: {_patrocinadorIdLogado}");

            // Iniciar o carregamento das campanhas ativas ao criar o VM
            // Usamos Task.Run para não bloquear o construtor, embora ExecuteAsync seja o padrão em MVVM
            _ = LoadCampanhasCommand.ExecuteAsync(null);
        }

        // Comando para Navegar para a Busca de Campeonatos (Para padrinhar)
        [RelayCommand]
        private async Task NavegarParaBuscaCampeonatos() {
            // TODO: Certifique-se de que Shell.Current está disponível (MAUI/Xamarin Forms)
            await Shell.Current.GoToAsync("BuscarCampeonatosPage");
        }

        [RelayCommand]
        private async Task LoadCampanhas() {
            if (EstaOcupado) return;

            if (_patrocinadorIdLogado <= 0) {
                System.Diagnostics.Debug.WriteLine("[DASHBOARD] PatrocinadorId inválido (ID=0). Abortando carregamento.");
                return;
            }

            EstaOcupado = true;
            CampanhasAtivas.Clear(); // Limpa antes de carregar
            ListaVazia = false; // Reset da flag

            try {
                var campanhasDoPatrocinador = await _databaseService.ObterCampanhasDoPatrocinadorAsync(_patrocinadorIdLogado);
                System.Diagnostics.Debug.WriteLine($"[DB Patrocinio] Retornou {campanhasDoPatrocinador.Count} campanhas para o dashboard.");

                var campanhasMapeadas = new List<CampanhaAtivaViewModel>();

                foreach (var c in campanhasDoPatrocinador) {
                    System.Diagnostics.Debug.WriteLine($"[DASHBOARD DEBUG] Buscando Campeonato ID: {c.CampeonatoId}");
                    var campeonato = await _databaseService.GetCampeonatoByIdAsync(c.CampeonatoId);

                    campanhasMapeadas.Add(new CampanhaAtivaViewModel {
                        CampanhaId = c.Id,

                        NomeCampanha = $"Patrocínio - {campeonato?.Nome ?? "Campeonato Desconhecido"}",

                        NomeCampeonato = campeonato?.Nome ?? "Campeonato Não Encontrado",
                        LogotipoCampeonatoUrl = campeonato?.LogoUrl ?? "logo_default.png", // Mapeia a URL para o XAML

                        DataFim = c.Fim.ToString("dd/MM/yyyy"),
                        Status = (c.Fim.Date < DateTime.Now.Date) ? "Finalizada" : "Ativa"
                    });
                }

                foreach (var vm in campanhasMapeadas) {
                    CampanhasAtivas.Add(vm);
                }

                ListaVazia = !CampanhasAtivas.Any();

            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"[PATROCINADOR] Erro ao carregar campanhas: {ex.Message}");
            } finally {
                EstaOcupado = false;
            }
        }

        // Comando para visualizar os detalhes de uma campanha
        [RelayCommand]
        private async Task NavegarParaDetalhesCampanha(CampanhaAtivaViewModel campanha) {
            if (campanha == null) return;
            // TODO: Implementar a navegação (assumindo que DetalheCampanhaPage está registrada)
            await Shell.Current.GoToAsync($"DetalheCampanhaPage?Id={campanha.CampanhaId}");
        }
    }
}