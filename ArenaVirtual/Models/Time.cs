using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace ArenaVirtual.Models {
    public partial class Time : INotifyPropertyChanged, ISyncable {

        // Campos de suporte privados para todas as propriedades
        private string nome = string.Empty;
        private string? logoUrl;
        private int campeonatoId;
        private string? descricao;
        private string? regiao;
        private int pontuacaoTotal;
        private int vitorias;
        private int derrotas;
        private int empates;
        private int? capitaoId;

        // Campos de suporte para as propriedades de sincronização
        private bool isSynced;
        private DateTime updatedAt;

        [PrimaryKey, AutoIncrement]
        [JsonIgnore]
        public int Id { get; set; }

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

        public int CampeonatoId {
            get => campeonatoId;
            set => SetProperty(ref campeonatoId, value);
        }

        [MaxLength(500)]
        public string? Descricao {
            get => descricao;
            set => SetProperty(ref descricao, value);
        }

        public DateTime DataCriacao { get; set; }

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

        // Propriedades de sincronização que manipulam os campos de suporte diretamente
        public bool IsSynced {
            get => isSynced;
            set => isSynced = value;
        }

        public DateTime UpdatedAt {
            get => updatedAt;
            set => updatedAt = value;
        }

        public Time() {
            // Inicializa os campos de suporte diretamente
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

            // Atualiza os campos de suporte diretamente para evitar recursão
            this.isSynced = false;
            this.updatedAt = DateTime.UtcNow;

            return true;
        }
    }
}