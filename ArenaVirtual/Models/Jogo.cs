using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;
using System.Diagnostics;

namespace ArenaVirtual.Models {

    public partial class Jogo : ObservableObject, ISyncable {

        // --- Propriedades Reativas Geradas (MVVM Toolkit) ---

        // Chave Primária Local
        [ObservableProperty]
        [property: PrimaryKey, AutoIncrement]
        private int id;

        // ID de sincronização (deve ser a implementação de ISyncable.ClientAppId)
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

        // O tipo JogoStatus deve estar definido em outro arquivo no namespace ArenaVirtual.Models
        [ObservableProperty]
        private JogoStatus status;

        [ObservableProperty]
        private int placarTimeAInt;

        [ObservableProperty]
        private int placarTimeBInt;

        // Propriedades Ignoradas pelo SQLite (apenas para exibição/lógica)
        [ObservableProperty]
        [property: Ignore]
        private Time? timeA;

        [ObservableProperty]
        [property: Ignore]
        private Time? timeB;

        [ObservableProperty]
        [property: Ignore] // Nome do árbitro é carregado do usuário, não armazenado aqui.
        private string nomeArbitro = string.Empty;

        [ObservableProperty]
        [property: Ignore]
        private bool isOrganizador;

        // --- Propriedades POCO/Interfaces ---
        public int IdServidor { get; set; }
        public int Rodada { get; set; }
        public string NomeCampeonato { get; set; } = string.Empty;
        public bool IsSynced { get; set; } // Propriedade da interface ISyncable
        public DateTime UpdatedAt { get; set; } // Propriedade da interface ISyncable


        // --- Construtor ---

        public Jogo() {
            this.ClientAppId = Guid.NewGuid(); 
            this.IsSynced = false;
            this.UpdatedAt = DateTime.UtcNow;

            this.Status = JogoStatus.Agendado;
            this.IsOrganizador = false;
        }


        // --- PROPRIEDADES CALCULADAS E CALLBACKS ---

        private bool ArbitroAtribuido => ArbitroId.HasValue && ArbitroId.Value != Guid.Empty;

        [Ignore]
        public bool BotaoArbitroHabilitado => IsOrganizador;


        [Ignore]
        public string TextoBotaoArbitro => !IsOrganizador
                ? (ArbitroAtribuido && !string.IsNullOrEmpty(NomeArbitro) ? $"Árbitro: {NomeArbitro}" : "Detalhes")
                : (ArbitroAtribuido
            ? $"Árbitro: {(!string.IsNullOrEmpty(NomeArbitro) ? NomeArbitro : "Anexado")}"
            : "Anexar Árbitros");


        [Ignore]
        public bool BotaoArbitroDesabilitado => !BotaoArbitroHabilitado;


        // --- CALLBACKS DO MVVM TOOLKIT ---

        partial void OnArbitroIdChanged(Guid? value) {
            Debug.WriteLine($"[JOGO MODEL] ArbitroId alterado para: {value}. Chamando Notificação.");
            NotifyArbitroStatusChanged();
        }

        partial void OnNomeArbitroChanged(string value) {
            Debug.WriteLine($"[JOGO MODEL] NomeArbitro alterado para: {value}. Chamando Notificação.");
            NotifyArbitroStatusChanged();
        }

        partial void OnIsOrganizadorChanged(bool value) {
            Debug.WriteLine($"[JOGO MODEL] IsOrganizador alterado para: {value}. Chamando Notificação.");
            NotifyArbitroStatusChanged();
        }

        // --- MÉTODO DE NOTIFICAÇÃO PÚBLICO ---
        public void NotifyArbitroStatusChanged() {
            // Notificar as propriedades calculadas.
            OnPropertyChanged(nameof(TextoBotaoArbitro));
            OnPropertyChanged(nameof(BotaoArbitroHabilitado));
            OnPropertyChanged(nameof(BotaoArbitroDesabilitado));

            // Notificar as propriedades base (útil para debug ou caso algum binding direto exista).
            OnPropertyChanged(nameof(ArbitroId));
            OnPropertyChanged(nameof(NomeArbitro));

            Debug.WriteLine($"[JOGO MODEL] Notificações de Status de Árbitro disparadas. Texto Botão: {TextoBotaoArbitro}");
        }
    }
}