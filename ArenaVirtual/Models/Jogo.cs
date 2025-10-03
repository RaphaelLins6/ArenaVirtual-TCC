using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;

namespace ArenaVirtual.Models {

    public partial class Jogo : ObservableObject, ISyncable {

        // --- Backing Fields (Campos Privados) ---
        private int id;
        private Guid clientAppId;
        private int timeAId;
        private int timeBId;
        private Time? timeA;
        private Time? timeB;
        private DateTime dataHora;
        private int campeonatoId;
        private int arbitroId;
        private string local = string.Empty;
        private string placarA = string.Empty;
        private string placarB = string.Empty;
        private JogoStatus status;
        private int placarTimeAInt;
        private int placarTimeBInt;
        private bool isOrganizador;

        // Campos que não usam SetProperty (POCO)
        public int Rodada { get; set; }
        public string NomeCampeonato { get; set; } = string.Empty;
        public bool IsSynced { get; set; }
        public DateTime UpdatedAt { get; set; }

        // --- Construtor ---

        public Jogo() {
            this.Status = JogoStatus.Agendado;
            this.IsSynced = false;
            this.UpdatedAt = DateTime.UtcNow;
            this.isOrganizador = false;
        }

        // --- Propriedades Públicas (Mapeamento SQLite e MVVM) ---

        // ATRIBUTOS DO SQLITE APLICADOS AQUI
        [PrimaryKey, AutoIncrement]
        public int Id {
            get => id;
            set => SetProperty(ref id, value);
        }

        public Guid ClientAppId {
            get => clientAppId;
            set => SetProperty(ref clientAppId, value);
        }

        public int TimeAId {
            get => timeAId;
            set => SetProperty(ref timeAId, value);
        }

        public int TimeBId {
            get => timeBId;
            set => SetProperty(ref timeBId, value);
        }

        public DateTime DataHora {
            get => dataHora;
            set => SetProperty(ref dataHora, value);
        }

        public int CampeonatoId {
            get => campeonatoId;
            set => SetProperty(ref campeonatoId, value);
        }

        public int ArbitroId {
            get => arbitroId;
            set => SetProperty(ref arbitroId, value);
        }

        public string Local {
            get => local;
            set => SetProperty(ref local, value);
        }

        public JogoStatus Status {
            get => status;
            set => SetProperty(ref status, value);
        }

        public int PlacarTimeAInt {
            get => placarTimeAInt;
            set => SetProperty(ref placarTimeAInt, value);
        }

        public int PlacarTimeBInt {
            get => placarTimeBInt;
            set => SetProperty(ref placarTimeBInt, value);
        }

        public string PlacarA {
            get => placarA;
            set => SetProperty(ref placarA, value);
        }

        public string PlacarB {
            get => placarB;
            set => SetProperty(ref placarB, value);
        }

        // --- Propriedades de Contexto/Relacionamento (Ignoradas pelo SQLite) ---

        // ATRIBUTOS DO SQLITE APLICADOS AQUI
        [Ignore]
        public bool IsOrganizador {
            get => isOrganizador;
            set => SetProperty(ref isOrganizador, value);
        }

        [Ignore]
        public Time? TimeA {
            get => timeA;
            set => SetProperty(ref timeA, value);
        }

        [Ignore]
        public Time? TimeB {
            get => timeB;
            set => SetProperty(ref timeB, value);
        }
    }
}