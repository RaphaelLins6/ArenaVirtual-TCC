using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic; // Necessário para List<T> e .Any()
using System.Linq; // Necessário para o método .Any()
using System.Threading.Tasks; // Necessário para Task
using System; // Necessário para DateTime, Exception e Debug.WriteLine

namespace ArenaVirtual.ViewModels.Patrocinador {

    // ⭐️ ViewModel Simples para a lista ⭐️
    public partial class CampanhaAtivaViewModel : ObservableObject {
        public int CampanhaId { get; set; }
        public string NomeCampanha { get; set; }
        public string NomeCampeonato { get; set; }
        public string LogotipoCampeonatoUrl { get; set; }
        public string DataFim { get; set; }
        public string Status { get; set; } // Ex: "Ativa", "Pendente", "Finalizada", "Dado Antigo/Bug"
    }

    public partial class DashboardPatrocinadorViewModel : ObservableObject {

        // Injeção de dependência e campo privado para o ID da sessão
        private readonly DatabaseService _databaseService;
        // NOTA: Para a correção funcionar, o DatabaseService precisa de um método 
        // ObterTodasCampanhasDoPatrocinadorAsync que não filtre por data.
        private readonly int _patrocinadorIdLogado;

        // ⭐️ Propriedades para o MVVM (Observable Collections) ⭐️
        [ObservableProperty]
        private ObservableCollection<CampanhaAtivaViewModel> campanhasAtivas = new();

        // ⭐️ NOVO: Lista para Inativas/Expiradas ⭐️
        [ObservableProperty]
        private ObservableCollection<CampanhaAtivaViewModel> campanhasInativas = new();

        [ObservableProperty]
        private bool estaOcupado; // Para o ActivityIndicator e RefreshView

        // Propriedade para controle da mensagem de lista vazia (para ATIVAS)
        [ObservableProperty]
        private bool listaVazia = false;

        // ⭐️ NOVO: Flag para lista de inativas vazia ⭐️
        [ObservableProperty]
        private bool listaInativaVazia = false;

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
            _ = LoadCampanhasCommand.ExecuteAsync(null);
        }

        // Comando para Navegar para a Busca de Campeonatos (Para padrinhar)
        [RelayCommand]
        private async Task NavegarParaBuscaCampeonatos() {
            // TODO: Certifique-se de que Shell.Current está disponível (MAUI/Xamarin Forms)
            await Shell.Current.GoToAsync("BuscarCampeonatosPage");
        }

        // ⭐️ MÉTODO LoadCampanhas CORRIGIDO E ATUALIZADO ⭐️
        [RelayCommand]
        private async Task LoadCampanhas() {
            if (EstaOcupado) return;

            if (_patrocinadorIdLogado <= 0) {
                System.Diagnostics.Debug.WriteLine("[DASHBOARD] PatrocinadorId inválido (ID=0). Abortando carregamento.");
                return;
            }

            EstaOcupado = true;
            CampanhasAtivas.Clear();
            CampanhasInativas.Clear(); // ⭐️ Limpa a nova lista ⭐️
            ListaVazia = false;
            ListaInativaVazia = false; // ⭐️ Reseta a nova flag ⭐️

            try {
                // 1. Chamar o serviço de Patrocínio para TODAS as campanhas
                // ⭐️ IMPORTANTE: Seu método ObterTodasCampanhasDoPatrocinadorAsync (novo)
                // deve ser alterado para NÃO FILTRAR por data, se ele estiver filtrando lá. ⭐️
                // Assumindo que este novo método exista e busque tudo.
                var todasCampanhas = await _databaseService.ObterTodasCampanhasDoPatrocinadorAsync(_patrocinadorIdLogado);
                System.Diagnostics.Debug.WriteLine($"[DB Patrocinio] Retornou {todasCampanhas.Count} campanhas totais.");

                foreach (var c in todasCampanhas) {

                    // 2a. BUSCAR O CAMPEONATO (Tratando o ID = 0)
                    // Se o ID for 0, usamos o fallback de imediato para evitar a busca desnecessária.
                    var campeonato = (c.CampeonatoId > 0) ?
                                     await _databaseService.GetCampeonatoByIdAsync(c.CampeonatoId) :
                                     null;

                    // Determina o Status
                    string status;
                    if (campeonato == null || c.CampeonatoId == 0) {
                        status = "Dado Antigo/Bug"; // Tratamento para o ID=0
                    } else if (c.Fim.Date < DateTime.Now.Date) {
                        status = "Finalizada"; // Corrigido para Finalizada (data de fim já passou)
                    } else {
                        status = "Ativa";
                    }

                    // 2b. Mapear
                    var vm = new CampanhaAtivaViewModel {
                        CampanhaId = c.Id,

                        // Correção do Bug Visual: Se ID=0, use o fallback nos nomes
                        NomeCampanha = (status == "Dado Antigo/Bug") ? "Patrocínio - Erro de Mapeamento" : $"Patrocínio - {campeonato?.Nome ?? "Campeonato Desconhecido"}",
                        NomeCampeonato = campeonato?.Nome ?? "Campeonato Não Encontrado",
                        LogotipoCampeonatoUrl = campeonato?.LogoUrl ?? "logo_default.png",
                        DataFim = c.Fim.ToString("dd/MM/yyyy"),
                        Status = status
                    };

                    // 3. Separar por listas
                    if (status == "Ativa") {
                        CampanhasAtivas.Add(vm);
                    } else {
                        // Inclui "Finalizada" e "Dado Antigo/Bug" em Inativas
                        CampanhasInativas.Add(vm);
                    }
                }

                // 4. Atualizar as flags da UI
                ListaVazia = !CampanhasAtivas.Any();
                ListaInativaVazia = !CampanhasInativas.Any(); // ⭐️ Nova flag ⭐️

            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"[PATROCINADOR] Erro ao carregar campanhas: {ex.Message}");
            } finally {
                EstaOcupado = false;
            }
        }
    }
}