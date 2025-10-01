using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace ArenaVirtual.Models {
    public partial class Jogo : ObservableObject, ISyncable {

        // ... (suas propriedades e campos de apoio) ...
        private int id;
        private Guid clientAppId;
        private int timeAId;
        private int timeBId;
        private Time timeA;
        private Time timeB;
        private DateTime dataHora;
        private int campeonatoId;
        private int arbitroId;
        public int Rodada { get; set; }
        private string local = string.Empty;
        private string placarA = string.Empty;
        private string placarB = string.Empty;
        public string NomeCampeonato { get; set; }

        // Adicione este construtor público sem parâmetros
        public Jogo() {
            // Inicialização padrão das propriedades, se necessário
            this.IsSynced = false;
            this.UpdatedAt = DateTime.UtcNow;
        }

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

        [Ignore]
        public Time TimeA {
            get => timeA;
            set => SetProperty(ref timeA, value);
        }

        [Ignore]
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