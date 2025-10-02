using ArenaVirtual.Models;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ArenaVirtual.ViewModels {

    // Helper para agrupar as estatísticas por atleta
    public partial class AtletaEstatisticaItem : ObservableObject {

        // Atleta/Usuário
        public Usuario Atleta { get; }
        public Time TimeDoAtleta { get; }

        public AtletaEstatisticaItem(Usuario atleta, Time time) {
            Atleta = atleta;
            TimeDoAtleta = time;
        }

        // Propriedades para input do Árbitro
        [ObservableProperty]
        private int _pontos;

        [ObservableProperty]
        private int _rebotes;

        [ObservableProperty]
        private int _assistencias;

        // Adicionar o restante das propriedades do EstatisticaPartida que o árbitro precisa inserir
        [ObservableProperty]
        private int _roubos;

        [ObservableProperty]
        private int _bloqueios;

        [ObservableProperty]
        private int _faltas;

        [ObservableProperty]
        private int _turnovers;

        // Se a tela for simplificada, o árbitro só insere Pontos.
        // Se for detalhada, ele insere Arremessos 2/3 e Lances Livres. 
        // Vamos manter o básico (Pontos, Rebotes, Assistências) por enquanto.
    }

    // O ViewModel principal para a tela de lançamento
    // Implementa IQueryAttributable para receber o ID do jogo via navegação
    public partial class LancamentoEstatisticaViewModel : ObservableObject, IQueryAttributable {

        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        private Jogo? _jogo;

        // Lista de atletas para exibir na tela e coletar estatísticas
        public ObservableCollection<AtletaEstatisticaItem> EstatisticasTimeA { get; } = new();
        public ObservableCollection<AtletaEstatisticaItem> EstatisticasTimeB { get; } = new();

        public LancamentoEstatisticaViewModel(DatabaseService databaseService) {
            _databaseService = databaseService;
        }

        // Método para receber o ID do jogo
        public async void ApplyQueryAttributes(IDictionary<string, object> query) {
            if (query.TryGetValue("JogoId", out object? jogoIdObject) && jogoIdObject is int jogoId) {
                await LoadJogoDataAsync(jogoId);
            }
        }

        private async Task LoadJogoDataAsync(int jogoId) {
            // 1. Obter o Jogo
            var jogo = await _databaseService.GetTable<Jogo>().Where(j => j.Id == jogoId).FirstOrDefaultAsync();
            if (jogo == null) {
                // TODO: Adicionar tratamento de erro (ex: mostrar alerta e voltar)
                return;
            }
            Jogo = jogo;

            // 2. Obter os Times (assumindo que já existem métodos para buscar por ID)
            var timeA = await _databaseService.GetTimeByIdAsync(Jogo.TimeAId);
            var timeB = await _databaseService.GetTimeByIdAsync(Jogo.TimeBId);

            if (timeA == null || timeB == null) return;

            // 3. Obter os Atletas (Assumindo que TimeClientAppId está no Atleta/Usuário)
            // Você precisa de um método no DatabaseService que liste os Usuários por TimeId/TimeClientAppId
            // Como você tem `GetMembrosByTimeClientAppIdAsync(Guid timeClientAppId)`, vamos assumir um ClientAppId.
            // Para simplificar, vou usar o ID inteiro do Time, mas talvez você precise ajustar o `GetMembros`

            var atletasA = await _databaseService.GetMembrosByTimeClientAppIdAsync(timeA.ClientAppId);
            var atletasB = await _databaseService.GetMembrosByTimeClientAppIdAsync(timeB.ClientAppId);

            // 4. Preencher a lista de Estatísticas
            EstatisticasTimeA.Clear();
            foreach (var atleta in atletasA) {
                EstatisticasTimeA.Add(new AtletaEstatisticaItem(atleta, timeA));
            }

            EstatisticasTimeB.Clear();
            foreach (var atleta in atletasB) {
                EstatisticasTimeB.Add(new AtletaEstatisticaItem(atleta, timeB));
            }
        }


        [RelayCommand]
        private async Task SalvarEstatisticasAsync() {
            if (Jogo == null) return;

            // 1. Validação (Ex: Garante que pelo menos Pontos foram inseridos)
            // ...

            // 2. Preparar todas as estatísticas para inserção
            var todasEstatisticas = new List<EstatisticaPartida>();

            // Coletar dados do Time A
            foreach (var item in EstatisticasTimeA) {
                if (item.Pontos > 0 || item.Rebotes > 0 || item.Assistencias > 0) { // Salvar apenas se houver dados
                    todasEstatisticas.Add(new EstatisticaPartida {
                        JogoId = Jogo.Id,
                        TimeId = Jogo.TimeAId,
                        UsuarioId = item.Atleta.Id, // Assumindo que Atleta.Id é o ID do Usuário
                        Pontos = item.Pontos,
                        Rebotes = item.Rebotes,
                        Assistencias = item.Assistencias,
                        // ... outros campos ...
                    });
                }
            }

            // Coletar dados do Time B
            foreach (var item in EstatisticasTimeB) {
                if (item.Pontos > 0 || item.Rebotes > 0 || item.Assistencias > 0) {
                    todasEstatisticas.Add(new EstatisticaPartida {
                        JogoId = Jogo.Id,
                        TimeId = Jogo.TimeBId,
                        UsuarioId = item.Atleta.Id,
                        Pontos = item.Pontos,
                        Rebotes = item.Rebotes,
                        Assistencias = item.Assistencias,
                        // ... outros campos ...
                    });
                }
            }

            // 3. Inserir no Banco de Dados (usando o método InserirEstatisticaAsync)
            // Você precisará de um método `InsertAll` se o SQLite-net suportar, ou fazer em loop.

            // Loop de inserção (o método InserirEstatisticaAsync é para um item)
            foreach (var estatistica in todasEstatisticas) {
                await _databaseService.InserirEstatisticaAsync(estatistica);
            }


            // 4. Calcular o Placar Final do Jogo
            int placarA = EstatisticasTimeA.Sum(e => e.Pontos);
            int placarB = EstatisticasTimeB.Sum(e => e.Pontos);

            // 5. Atualizar o Jogo com o Placar e o Status "Finalizado"
            Jogo.PlacarTimeAInt = placarA;
            Jogo.PlacarTimeBInt = placarB;
            Jogo.Status = JogoStatus.Finalizado;

            await _databaseService.AtualizarJogoAsync(Jogo);

            // 6. Navegar de volta (ex: para a lista de partidas do árbitro)
            await Shell.Current.DisplayAlert("Sucesso", "Estatísticas e Placar finalizados com sucesso!", "OK");
            await Shell.Current.GoToAsync("..");
        }
    }
}
