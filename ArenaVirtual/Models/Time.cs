using SQLite;
using System.ComponentModel; // Adicione este using
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices; // Adicione este using

namespace ArenaVirtual.Models {
    // Altere a declaração da classe para incluir INotifyPropertyChanged e ISyncable
    public partial class Time : INotifyPropertyChanged, ISyncable {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        private string nome = string.Empty;
        [NotNull, MaxLength(100)]
        public string Nome {
            get => nome;
            set => SetProperty(ref nome, value);
        }

        private string? logoUrl;
        [MaxLength(255)]
        public string? LogoUrl {
            get => logoUrl;
            set => SetProperty(ref logoUrl, value);
        }

        private int campeonatoId;
        public int CampeonatoId {
            get => campeonatoId;
            set => SetProperty(ref campeonatoId, value);
        }

        private string? descricao;
        [MaxLength(500)]
        public string? Descricao {
            get => descricao;
            set => SetProperty(ref descricao, value);
        }

        // DataCriacao deve ser inicializada no construtor, ou a propriedade em si
        public DateTime DataCriacao { get; set; }

        private string? regiao;
        [MaxLength(50)]
        public string? Regiao {
            get => regiao;
            set => SetProperty(ref regiao, value);
        }

        private int pontuacaoTotal;
        public int PontuacaoTotal {
            get => pontuacaoTotal;
            set => SetProperty(ref pontuacaoTotal, value);
        }

        private int vitorias;
        public int Vitorias {
            get => vitorias;
            set => SetProperty(ref vitorias, value);
        }

        private int derrotas;
        public int Derrotas {
            get => derrotas;
            set => SetProperty(ref derrotas, value);
        }

        private int empates;
        public int Empates {
            get => empates;
            set => SetProperty(ref empates, value);
        }

        private int? capitaoId;
        [ForeignKey("CapitaoId")]
        public int? CapitaoId {
            get => capitaoId;
            set => SetProperty(ref capitaoId, value);
        }

        // Propriedades de sincronização (já estão aqui, mas precisam da lógica de SetProperty para serem atualizadas)
        public bool IsSynced { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Construtor para inicializar as propriedades de sincronização e DataCriacao
        public Time() {
            IsSynced = false;
            UpdatedAt = DateTime.UtcNow;
            DataCriacao = DateTime.Now; // Ou DateTime.UtcNow, dependendo da sua preferência
        }

        // Implementação do PropertyChangedEventHandler
        public event PropertyChangedEventHandler? PropertyChanged;

        // Método auxiliar para definir propriedades e disparar eventos
        protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string? propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(backingField, value))
                return false;

            backingField = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            // Lógica de sincronização: Se a propriedade foi alterada, marque o objeto para sincronização
            this.IsSynced = false;
            this.UpdatedAt = DateTime.UtcNow;
            return true;
        }
    }
}