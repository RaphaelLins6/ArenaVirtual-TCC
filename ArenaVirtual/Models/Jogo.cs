using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;
using System.Diagnostics;

namespace ArenaVirtual.Models {

    public partial class Jogo : ObservableObject, ISyncable {

        [ObservableProperty]
        [property: PrimaryKey, AutoIncrement]
        private int id;

        [ObservableProperty]
        private Guid clientAppId;

        [ObservableProperty]
        private Guid? arbitroId;

        [ObservableProperty]
        private int timeAId;

        [ObservableProperty]
        private int timeBId;

        [ObservableProperty]
        [property: Ignore]
        private string? timeANome;

        [ObservableProperty]
        [property: Ignore]
        private string? timeBNome;

        [ObservableProperty]
        private DateTime dataHora;

        [ObservableProperty]
        private int campeonatoId;

        [ObservableProperty]
        private Guid campeonatoClientAppId;

        [ObservableProperty]
        private string local = string.Empty;

        [ObservableProperty]
        private string placarA = string.Empty;

        [ObservableProperty]
        private string placarB = string.Empty;

        [ObservableProperty]
        private JogoStatus status;

        [ObservableProperty]
        private int placarTimeAInt;

        [ObservableProperty]
        private int placarTimeBInt;

        [ObservableProperty]
        [property: Ignore]
        private Time? timeA;

        [ObservableProperty]
        [property: Ignore]
        private Time? timeB;

        [ObservableProperty]
        [property: Ignore]
        private Campeonato? campeonato;

        [ObservableProperty]
        [property: Ignore]
        private string nomeArbitro = string.Empty;

        [ObservableProperty]
        [property: Ignore]
        private bool isOrganizador;

        public int IdServidor { get; set; }
        public int Rodada { get; set; }
        public string NomeCampeonato { get; set; } = string.Empty;
        public bool IsSynced { get; set; }
        public DateTime UpdatedAt { get; set; }

        // --- Construtor ---
        public Jogo() {
            ClientAppId = Guid.NewGuid();
            IsSynced = false;
            UpdatedAt = DateTime.UtcNow;
            Status = JogoStatus.Agendado;
            IsOrganizador = false;
        }

        [Ignore]
        public Time? TimeCasa {
            get => TimeA; 
            set => TimeA = value; 
        }

        [Ignore]
        public Time? TimeFora {
            get => TimeB; 
            set => TimeB = value; 
        }

        partial void OnTimeAChanged(Time? value) {
            TimeANome = value?.Nome;
            OnPropertyChanged(nameof(TimeCasa));
        }

        partial void OnTimeBChanged(Time? value) {
            TimeBNome = value?.Nome;
            OnPropertyChanged(nameof(TimeFora));
        }

        [Ignore]
        public bool ArbitroAtribuido => ArbitroId.HasValue && ArbitroId != Guid.Empty;

        [Ignore]
        public bool BotaoArbitroHabilitado => IsOrganizador && Status == JogoStatus.Agendado;

        [Ignore]
        public string TextoBotaoArbitro =>
            (BotaoArbitroHabilitado && ArbitroAtribuido)
                ? $"Árbitro: {(string.IsNullOrEmpty(NomeArbitro) ? "Anexado" : NomeArbitro)}"
                : (ArbitroAtribuido
                    ? $"Árbitro: {(string.IsNullOrEmpty(NomeArbitro) ? "Anexado" : NomeArbitro)}"
                    : "Anexar Árbitros");

        [Ignore]
        public string PlacarParaExibir {
            get {
                if (Status == JogoStatus.Finalizado) {
                    return $"{PlacarTimeAInt} - vs - {PlacarTimeBInt}";
                }
                return "X - vs - Y";
            }
        }

        [Ignore]
        public bool BotaoArbitroDesabilitado => !BotaoArbitroHabilitado;

        partial void OnArbitroIdChanged(Guid? value) {
            //Debug.WriteLine($"[JOGO MODEL] ArbitroId alterado -> {value}");
            UpdatedAt = DateTime.UtcNow;
            IsSynced = false;
            NotifyArbitroStatusChanged();
        }

        partial void OnNomeArbitroChanged(string value) {
            //Debug.WriteLine($"[JOGO MODEL] NomeArbitro alterado -> {value}");
            NotifyArbitroStatusChanged();
        }

        partial void OnIsOrganizadorChanged(bool value) {
            //Debug.WriteLine($"[JOGO MODEL] IsOrganizador alterado -> {value}");
            NotifyArbitroStatusChanged();
        }

        partial void OnStatusChanged(JogoStatus value) {
            OnPropertyChanged(nameof(PlacarParaExibir)); 
        }

        partial void OnPlacarTimeAIntChanged(int value) {
            OnPropertyChanged(nameof(PlacarParaExibir)); 
        }

        partial void OnPlacarTimeBIntChanged(int value) {
            OnPropertyChanged(nameof(PlacarParaExibir)); 
        }

        public void NotifyArbitroStatusChanged() {
            OnPropertyChanged(nameof(TextoBotaoArbitro));
            OnPropertyChanged(nameof(BotaoArbitroHabilitado));
            OnPropertyChanged(nameof(BotaoArbitroDesabilitado));
            OnPropertyChanged(nameof(ArbitroId));
            OnPropertyChanged(nameof(NomeArbitro));

            //Debug.WriteLine($"[JOGO MODEL] UI atualizada → Árbitro: {NomeArbitro}, Habilitado: {BotaoArbitroHabilitado}");
        }
    }
}