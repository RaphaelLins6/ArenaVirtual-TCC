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

        [RelayCommand(CanExecute = nameof(IsNotOcupado))]
        private async Task SalvarEstatisticasAsync() {
            if (Jogo == null || EstaOcupado) return;

            EstaOcupado = true;
            try {
                // 1. Prepara todas as estatísticas para inserção
                Func<AtletaEstatisticaItem, bool> hasData = item =>
                    item.Pontos > 0 || item.Rebotes > 0 || item.Assistencias > 0 ||
                    item.Roubos > 0 || item.Bloqueios > 0 || item.Faltas > 0 ||
                    item.Turnovers > 0 || item.Arremessos2PontosConvertidos > 0 ||
                    item.Arremessos2PontosTentados > 0 || item.Arremessos3PontosConvertidos > 0 ||
                    item.Arremessos3PontosTentados > 0 || item.LancesLivresConvertidos > 0 ||
                    item.LancesLivresTentados > 0;

                var estatisticasTimeA = EstatisticasTimeA
                    .Where(hasData)
                    .Select(item => item.ToEstatisticaPartidaModel(Jogo.Id));

                var estatisticasTimeB = EstatisticasTimeB
                    .Where(hasData)
                    .Select(item => item.ToEstatisticaPartidaModel(Jogo.Id));

                var todasEstatisticas = estatisticasTimeA.Concat(estatisticasTimeB).ToList();

                if (!todasEstatisticas.Any()) {
                    await Shell.Current.DisplayAlert("Atenção", "Nenhuma estatística para salvar. Por favor, insira os dados.", "OK");
                    return;
                }

                // 2. Inserir as estatísticas no Banco de Dados
                foreach (var estatistica in todasEstatisticas) {
                    await _databaseService.InserirEstatisticaAsync(estatistica);
                }

                // 3. Calcular e Atualizar o Placar Final do Jogo
                PlacarTimeA = EstatisticasTimeA.Sum(e => e.Pontos);
                PlacarTimeB = EstatisticasTimeB.Sum(e => e.Pontos);

                Jogo.PlacarTimeAInt = PlacarTimeA;
                Jogo.PlacarTimeBInt = PlacarTimeB;
                Jogo.Status = JogoStatus.Finalizado;

                await _databaseService.AtualizarJogoAsync(Jogo);

                // 4. Navegar de volta
                Debug.WriteLine($"Estatísticas salvas. Placar: {PlacarTimeA} x {PlacarTimeB}");
                await Shell.Current.DisplayAlert("Sucesso", "Estatísticas e Placar finalizados com sucesso!", "OK");
                await Shell.Current.GoToAsync("..");
            } catch (Exception ex) {
                Debug.WriteLine($"Erro ao salvar estatísticas: {ex.Message}");
                await Shell.Current.DisplayAlert("Erro", "Ocorreu um erro ao salvar as estatísticas.", "OK");
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

                // 4. Preencher a lista de Estatísticas
                EstatisticasTimeA.Clear();
                foreach (var atleta in atletasA) {
                    EstatisticasTimeA.Add(new AtletaEstatisticaItem(atleta, timeA));
                }

                EstatisticasTimeB.Clear();
                foreach (var atleta in atletasB) {
                    EstatisticasTimeB.Add(new AtletaEstatisticaItem(atleta, timeB));
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