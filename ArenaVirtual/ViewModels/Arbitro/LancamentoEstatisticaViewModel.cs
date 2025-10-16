using ArenaVirtual.Models;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

#if NETSTANDARD || NETFRAMEWORK || NETCOREAPP
using IDictionary = System.Collections.Generic.IDictionary<string, object>;
#else
using Microsoft.Maui.Controls; // Para IQueryAttributable
#endif

namespace ArenaVirtual.ViewModels.Arbitro {

    public partial class AtletaEstatisticaItem : ObservableObject {
        // Campos de referência para o Atleta e Time
        public Usuario Atleta { get; }
        public Time TimeDoAtleta { get; }
        public string NomeAtleta => Atleta.Nome;
        public string FotoAtletaUrl => Atleta.ImagemPath;

        public AtletaEstatisticaItem(Usuario atleta, Time time) {
            Atleta = atleta;
            TimeDoAtleta = time;
        }

        // Propriedades para input do Árbitro
        [ObservableProperty] private int pontos;
        [ObservableProperty] private int rebotes;
        [ObservableProperty] private int assistencias;
        [ObservableProperty] private int roubos;
        [ObservableProperty] private int bloqueios;
        [ObservableProperty] private int faltas;
        [ObservableProperty] private int turnovers;

        // Campos detalhados
        [ObservableProperty] private int arremessos2PontosConvertidos;
        [ObservableProperty] private int arremessos2PontosTentados;
        [ObservableProperty] private int arremessos3PontosConvertidos;
        [ObservableProperty] private int arremessos3PontosTentados;
        [ObservableProperty] private int lancesLivresConvertidos;
        [ObservableProperty] private int lancesLivresTentados;

        // Método para gerar o Model de EstatísticaPartida
        public EstatisticaPartida ToEstatisticaPartidaModel(int jogoId) {
            return new EstatisticaPartida {
                JogoId = jogoId,
                TimeId = TimeDoAtleta.Id,
                UsuarioId = Atleta.Id,

                // Campos básicos
                Pontos = Pontos,
                Rebotes = Rebotes,
                Assistencias = Assistencias,
                Roubos = Roubos,
                Bloqueios = Bloqueios,
                Faltas = Faltas,
                Turnovers = Turnovers,

                // Campos detalhados
                Arremessos2PontosConvertidos = Arremessos2PontosConvertidos,
                Arremessos2PontosTentados = Arremessos2PontosTentados,
                Arremessos3PontosConvertidos = Arremessos3PontosConvertidos,
                Arremessos3PontosTentados = Arremessos3PontosTentados,
                LancesLivresConvertidos = LancesLivresConvertidos,
                LancesLivresTentados = LancesLivresTentados,
            };
        }
    }

    public partial class LancamentoEstatisticaViewModel : ObservableObject, IQueryAttributable {

        private readonly DatabaseService _databaseService;

        [ObservableProperty] private int placarTimeA;
        [ObservableProperty] private int placarTimeB;
        [ObservableProperty] private Jogo? jogo;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SalvarEstatisticasCommand))]
        private bool estaOcupado;

        public ObservableCollection<AtletaEstatisticaItem> EstatisticasTimeA { get; } = new();
        public ObservableCollection<AtletaEstatisticaItem> EstatisticasTimeB { get; } = new();

        public LancamentoEstatisticaViewModel(DatabaseService databaseService) {
            _databaseService = databaseService;
        }

        public async void ApplyQueryAttributes(IDictionary query) {
            if (query.TryGetValue("JogoId", out object? jogoIdObject) && jogoIdObject is int jogoId) {
                await LoadJogoDataAsync(jogoId);
            }
        }

        public bool IsNotOcupado => !EstaOcupado;

        [RelayCommand]
        private async Task SalvarEstatisticas() {
            if (EstaOcupado) return;
            EstaOcupado = true;

            try {
                // 1. Atualiza os placares no objeto Jogo em memória
                Jogo.PlacarTimeAInt = PlacarTimeA;
                Jogo.PlacarTimeBInt = PlacarTimeB;

                // 🏆 CORREÇÃO CRÍTICA: Força o Status do Jogo para FINALIZADO (2)
                // Isso garante que o jogo será incluído na Tabela de Classificação
                if (Jogo.Status != JogoStatus.Finalizado) {
                    // Presumindo que o enum JogoStatus.Finalizado está acessível e tem valor 2
                    Jogo.Status = JogoStatus.Finalizado;
                }

                var estatisticasParaSalvar = EstatisticasTimeA
                    .Concat(EstatisticasTimeB)
                    .Select(item => item.ToEstatisticaPartidaModel(Jogo.Id))
                    .ToList();

                // 2. Chama o Service para salvar o Jogo atualizado (com o novo Status) e as Estatísticas
                bool sucesso = await _databaseService.SalvarEstatisticasDoJogoAsync(Jogo, estatisticasParaSalvar);

                if (sucesso) {
                    await Shell.Current.DisplayAlert("Sucesso", "Estatísticas salvas com sucesso!", "OK");
                    var navigationParameters = new ShellNavigationQueryParameters {
                        { "jogoAtualizado", Jogo }
                    };
                    await Shell.Current.GoToAsync("..", navigationParameters);
                } else {
                    await Shell.Current.DisplayAlert("Erro", "Falha ao salvar as estatísticas. Tente novamente.", "OK");
                }

            } catch (Exception ex) {
                Debug.WriteLine($"Erro ao salvar estatísticas: {ex.Message}");
                await Shell.Current.DisplayAlert("Erro", "Ocorreu um erro inesperado ao salvar. Detalhes: " + ex.Message, "OK");
            } finally {
                EstaOcupado = false;
            }
        }

        private async Task LoadJogoDataAsync(int jogoId) {
            if (EstaOcupado) return;

            EstaOcupado = true;
            try {
                // 1. Obter o Jogo
                var jogo = await _databaseService.GetTable<Jogo>().Where(j => j.Id == jogoId).FirstOrDefaultAsync();
                if (jogo == null) {
                    await Shell.Current.DisplayAlert("Erro", "Jogo não encontrado.", "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                // 2. Obter os Times
                var timeA = await _databaseService.GetTimeByIdAsync(jogo.TimeAId);
                var timeB = await _databaseService.GetTimeByIdAsync(jogo.TimeBId);

                if (timeA == null || timeB == null) {
                    await Shell.Current.DisplayAlert("Erro", "Um ou ambos os times não foram encontrados.", "OK");
                    return;
                }

                // Atribuir os objetos Time diretamente ao Jogo
                jogo.TimeA = timeA;
                jogo.TimeB = timeB;

                Jogo = jogo;

                // Inicializa o placar com os valores do jogo (se existirem)
                PlacarTimeA = Jogo.PlacarTimeAInt;
                PlacarTimeB = Jogo.PlacarTimeBInt;


                // 3. Obter os Atletas (Elenco)
                var atletasA = await _databaseService.GetMembrosByTimeClientAppIdAsync(jogo.TimeA.ClientAppId);
                var atletasB = await _databaseService.GetMembrosByTimeClientAppIdAsync(jogo.TimeB.ClientAppId);

                // 4. Obter estatísticas existentes
                var estatisticasSalvas = await _databaseService.GetEstatisticasPorJogoIdAsync(jogo.Id);


                // 5. Preencher a lista de Estatísticas do Time A
                EstatisticasTimeA.Clear();
                foreach (var atleta in atletasA) {
                    var item = new AtletaEstatisticaItem(atleta, timeA);

                    // CRÍTICO: Pré-preencher com estatísticas salvas
                    var statsDoAtleta = estatisticasSalvas.FirstOrDefault(s => s.UsuarioId == atleta.Id);
                    if (statsDoAtleta != null) {
                        // Mapear todos os campos salvos para o ViewModel
                        item.Pontos = statsDoAtleta.Pontos;
                        item.Rebotes = statsDoAtleta.Rebotes;
                        item.Assistencias = statsDoAtleta.Assistencias;
                        item.Roubos = statsDoAtleta.Roubos;
                        item.Bloqueios = statsDoAtleta.Bloqueios;
                        item.Faltas = statsDoAtleta.Faltas;
                        item.Turnovers = statsDoAtleta.Turnovers;
                        item.Arremessos2PontosConvertidos = statsDoAtleta.Arremessos2PontosConvertidos;
                        item.Arremessos2PontosTentados = statsDoAtleta.Arremessos2PontosTentados;
                        item.Arremessos3PontosConvertidos = statsDoAtleta.Arremessos3PontosConvertidos;
                        item.Arremessos3PontosTentados = statsDoAtleta.Arremessos3PontosTentados;
                        item.LancesLivresConvertidos = statsDoAtleta.LancesLivresConvertidos;
                        item.LancesLivresTentados = statsDoAtleta.LancesLivresTentados;

                        Debug.WriteLine($"[DEBUG-RECARGA] Estatísticas do Atleta {atleta.Nome} (Time A) carregadas (Pontos: {item.Pontos})");
                    }

                    EstatisticasTimeA.Add(item);
                }

                // 6. Preencher a lista de Estatísticas do Time B
                EstatisticasTimeB.Clear();
                foreach (var atleta in atletasB) {
                    var item = new AtletaEstatisticaItem(atleta, timeB);

                    // CRÍTICO: Pré-preencher com estatísticas salvas
                    var statsDoAtleta = estatisticasSalvas.FirstOrDefault(s => s.UsuarioId == atleta.Id);
                    if (statsDoAtleta != null) {
                        // Mapear todos os campos salvos para o ViewModel
                        item.Pontos = statsDoAtleta.Pontos;
                        item.Rebotes = statsDoAtleta.Rebotes;
                        item.Assistencias = statsDoAtleta.Assistencias;
                        item.Roubos = statsDoAtleta.Roubos;
                        item.Bloqueios = statsDoAtleta.Bloqueios;
                        item.Faltas = statsDoAtleta.Faltas;
                        item.Turnovers = statsDoAtleta.Turnovers;
                        item.Arremessos2PontosConvertidos = statsDoAtleta.Arremessos2PontosConvertidos;
                        item.Arremessos2PontosTentados = statsDoAtleta.Arremessos2PontosTentados;
                        item.Arremessos3PontosConvertidos = statsDoAtleta.Arremessos3PontosConvertidos;
                        item.Arremessos3PontosTentados = statsDoAtleta.Arremessos3PontosTentados;
                        item.LancesLivresConvertidos = statsDoAtleta.LancesLivresConvertidos;
                        item.LancesLivresTentados = statsDoAtleta.LancesLivresTentados;

                        Debug.WriteLine($"[DEBUG-RECARGA] Estatísticas do Atleta {atleta.Nome} (Time B) carregadas (Pontos: {item.Pontos})");
                    }

                    EstatisticasTimeB.Add(item);
                }

                Debug.WriteLine($"Elenco do Time A carregado: {EstatisticasTimeA.Count} atletas.");
                Debug.WriteLine($"Elenco do Time B carregado: {EstatisticasTimeB.Count} atletas.");
            } catch (Exception ex) {
                Debug.WriteLine($"Erro ao carregar dados do jogo/elencos: {ex.Message}");
                await Shell.Current.DisplayAlert("Erro", "Não foi possível carregar os dados do jogo.", "OK");
            } finally {
                EstaOcupado = false;
            }
        }
    }
}