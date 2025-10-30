using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic; 
using System.Linq; 
using System.Threading.Tasks; 
using System; 

namespace ArenaVirtual.ViewModels.Patrocinador {

    public partial class CampanhaAtivaViewModel : ObservableObject {
        public int CampanhaId { get; set; }
        public string NomeCampanha { get; set; }
        public string NomeCampeonato { get; set; }
        public string LogotipoCampeonatoUrl { get; set; }
        public string DataFim { get; set; }
        public string Status { get; set; } 
    }

    public partial class DashboardPatrocinadorViewModel : ObservableObject {

        private readonly DatabaseService _databaseService;
        private readonly int _patrocinadorIdLogado;

        [ObservableProperty]
        private ObservableCollection<CampanhaAtivaViewModel> campanhasAtivas = new();

        [ObservableProperty]
        private ObservableCollection<CampanhaAtivaViewModel> campanhasInativas = new();

        [ObservableProperty]
        private bool estaOcupado; 

        [ObservableProperty]
        private bool listaVazia = false;

        [ObservableProperty]
        private bool listaInativaVazia = false;

        public DashboardPatrocinadorViewModel(DatabaseService databaseService) {
            _databaseService = databaseService;

            _patrocinadorIdLogado = SessaoService.Instancia.GetUsuarioAtual()?.Id ?? 0;

            //System.Diagnostics.Debug.WriteLine($"[DASHBOARD VM] Patrocinador ID da Sessão: {_patrocinadorIdLogado}");

            _ = LoadCampanhasCommand.ExecuteAsync(null);
        }

        [RelayCommand]
        private async Task NavegarParaBuscaCampeonatos() {
            await Shell.Current.GoToAsync("BuscarCampeonatosPage");
        }

        [RelayCommand]
        private async Task LoadCampanhas() {
            if (EstaOcupado) return;

            if (_patrocinadorIdLogado <= 0) {
                //System.Diagnostics.Debug.WriteLine("[DASHBOARD] PatrocinadorId inválido (ID=0). Abortando carregamento.");
                return;
            }

            EstaOcupado = true;
            CampanhasAtivas.Clear();
            CampanhasInativas.Clear(); 
            ListaVazia = false;
            ListaInativaVazia = false; 

            try {
                var todasCampanhas = await _databaseService.ObterTodasCampanhasDoPatrocinadorAsync(_patrocinadorIdLogado);
                //System.Diagnostics.Debug.WriteLine($"[DB Patrocinio] Retornou {todasCampanhas.Count} campanhas totais.");

                foreach (var c in todasCampanhas) {

                    var campeonato = (c.CampeonatoId > 0) ?
                                     await _databaseService.GetCampeonatoByIdAsync(c.CampeonatoId) :
                                     null;

                    string status;
                    if (campeonato == null || c.CampeonatoId == 0) {
                        status = "Dado Antigo/Bug"; 
                    } else if (c.Fim.Date < DateTime.Now.Date) {
                        status = "Finalizada"; 
                    } else {
                        status = "Ativa";
                    }

                    var vm = new CampanhaAtivaViewModel {
                        CampanhaId = c.Id,

                        NomeCampanha = (status == "Dado Antigo/Bug") ? "Patrocínio - Erro de Mapeamento" : $"Patrocínio - {campeonato?.Nome ?? "Campeonato Desconhecido"}",
                        NomeCampeonato = campeonato?.Nome ?? "Campeonato Não Encontrado",
                        LogotipoCampeonatoUrl = campeonato?.LogoUrl ?? "placeholder.png",
                        DataFim = c.Fim.ToString("dd/MM/yyyy"),
                        Status = status
                    };

                    if (status == "Ativa") {
                        CampanhasAtivas.Add(vm);
                    } else {
                        CampanhasInativas.Add(vm);
                    }
                }

                ListaVazia = !CampanhasAtivas.Any();
                ListaInativaVazia = !CampanhasInativas.Any(); 

            } catch (Exception ex) {
                //System.Diagnostics.Debug.WriteLine($"[PATROCINADOR] Erro ao carregar campanhas: {ex.Message}");
            } finally {
                EstaOcupado = false;
            }
        }
    }
}