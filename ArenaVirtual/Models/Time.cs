using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace ArenaVirtual.Models {
    public partial class Time : INotifyPropertyChanged, ISyncable {

        private string nome = string.Empty;
        private string? logoUrl;
        private int? campeonatoId;
        private string? descricao;
        private string? regiao;
        private int pontuacaoTotal;
        private int vitorias;
        private int derrotas;
        private int empates;
        private int? capitaoId;
        private Guid? capitaoClientAppId;
        public Guid? CapitaoClientAppId {
            get => capitaoClientAppId;
            set => SetProperty(ref capitaoClientAppId, value);
        }
        private DateTime dataCriacao;
        private Guid clientAppId; 
        private Guid campeonatoClientAppId;
        private bool isSynced;
        private DateTime updatedAt;

        [PrimaryKey, AutoIncrement]
        [JsonIgnore]
        public int Id { get; set; }

        [NotNull, Unique]
        public Guid ClientAppId {
            get => clientAppId;
            set => SetProperty(ref clientAppId, value);
        }

        [NotNull, MaxLength(100)]
        public string Nome {
            get => nome;
            set => SetProperty(ref nome, value);
        }

        [MaxLength(255)]
        public string? LogoUrl {
            get => logoUrl;
            set => SetProperty(ref logoUrl, value);
        }

        public int? CampeonatoId {
            get => campeonatoId;
            set => SetProperty(ref campeonatoId, value);
        }

        [JsonIgnore]
        public Guid CampeonatoClientAppId {
            get => campeonatoClientAppId;
            set => SetProperty(ref campeonatoClientAppId, value);
        }


        [MaxLength(500)]
        public string? Descricao {
            get => descricao;
            set => SetProperty(ref descricao, value);
        }

        public DateTime DataCriacao {
            get => dataCriacao;
            set => SetProperty(ref dataCriacao, value);
        }

        [MaxLength(50)]
        public string? Regiao {
            get => regiao;
            set => SetProperty(ref regiao, value);
        }

        public int PontuacaoTotal {
            get => pontuacaoTotal;
            set => SetProperty(ref pontuacaoTotal, value);
        }

        public int Vitorias {
            get => vitorias;
            set => SetProperty(ref vitorias, value);
        }

        public int Derrotas {
            get => derrotas;
            set => SetProperty(ref derrotas, value);
        }

        public int Empates {
            get => empates;
            set => SetProperty(ref empates, value);
        }

        [ForeignKey("CapitaoId")]
        public int? CapitaoId {
            get => capitaoId;
            set => SetProperty(ref capitaoId, value);
        }

        public bool IsSynced {
            get => isSynced;
            set => isSynced = value;
        }

        public DateTime UpdatedAt {
            get => updatedAt;
            set => updatedAt = value;
        }

        public Time() {
            isSynced = false;
            updatedAt = DateTime.UtcNow;
            DataCriacao = DateTime.Now;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string? propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(backingField, value))
                return false;

            backingField = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            this.isSynced = false;
            this.updatedAt = DateTime.UtcNow;

            return true;
        }
    }
}