using SQLite;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ArenaVirtual.Models {
    public class Jogo : INotifyPropertyChanged {

        // Propriedades privadas para o backing field
        private int id;
        private Guid clientAppId;
        private int timeAId;
        private int timeBId;
        private Time timeA;
        private Time timeB;
        private DateTime dataHora;
        private int campeonatoId;
        private int arbitroId;
        private string local = string.Empty;
        private string placarA = string.Empty;
        private string placarB = string.Empty;

        [PrimaryKey, AutoIncrement]
        public int Id {
            get => id;
            set => SetProperty(ref id, value);
        }

        public Guid ClientAppId {
            get => clientAppId;
            set => SetProperty(ref clientAppId, value);
        }

        // Propriedades para as chaves estrangeiras
        public int TimeAId {
            get => timeAId;
            set => SetProperty(ref timeAId, value);
        }
        public int TimeBId {
            get => timeBId;
            set => SetProperty(ref timeBId, value);
        }

        // Propriedades de navegação para a interface de usuário
        [Ignore] // Ignore estas propriedades no SQLite, pois elas são para a UI
        public Time TimeA {
            get => timeA;
            set => SetProperty(ref timeA, value);
        }

        [Ignore] // Ignore estas propriedades no SQLite, pois elas são para a UI
        public Time TimeB {
            get => timeB;
            set => SetProperty(ref timeB, value);
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

        // Adicione as propriedades de placar
        public string PlacarA {
            get => placarA;
            set => SetProperty(ref placarA, value);
        }

        public string PlacarB {
            get => placarB;
            set => SetProperty(ref placarB, value);
        }

        public bool IsSynced { get; set; }
        public DateTime UpdatedAt { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string? propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(backingField, value))
                return false;

            backingField = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}