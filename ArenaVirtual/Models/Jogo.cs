using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;
using System.Diagnostics;

namespace ArenaVirtual.Models {

    public partial class Jogo : ObservableObject, ISyncable {

        // --- Propriedades Persistentes (salvas no SQLite) ---

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

        // --- Propriedades Ignoradas pelo SQLite (usadas apenas na UI) ---

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

        // --- Outras Propriedades (não persistidas automaticamente) ---
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

        // --- Propriedades Calculadas (para a UI) ---

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
        public bool BotaoArbitroDesabilitado => !BotaoArbitroHabilitado;

        // --- Callbacks automáticos do MVVM Toolkit (CRÍTICO para a persistência) ---

        partial void OnArbitroIdChanged(Guid? value) {
            Debug.WriteLine($"[JOGO MODEL] ArbitroId alterado -> {value}");
            UpdatedAt = DateTime.UtcNow;
            IsSynced = false;
            NotifyArbitroStatusChanged();
        }

        partial void OnNomeArbitroChanged(string value) {
            Debug.WriteLine($"[JOGO MODEL] NomeArbitro alterado -> {value}");
            NotifyArbitroStatusChanged();
        }

        partial void OnIsOrganizadorChanged(bool value) {
            Debug.WriteLine($"[JOGO MODEL] IsOrganizador alterado -> {value}");
            NotifyArbitroStatusChanged();
        }

        // --- Notificação manual para a UI ---

        public void NotifyArbitroStatusChanged() {
            OnPropertyChanged(nameof(TextoBotaoArbitro));
            OnPropertyChanged(nameof(BotaoArbitroHabilitado));
            OnPropertyChanged(nameof(BotaoArbitroDesabilitado));
            OnPropertyChanged(nameof(ArbitroId));
            OnPropertyChanged(nameof(NomeArbitro));

            Debug.WriteLine($"[JOGO MODEL] UI atualizada → Árbitro: {NomeArbitro}, Habilitado: {BotaoArbitroHabilitado}");
        }
    }
}