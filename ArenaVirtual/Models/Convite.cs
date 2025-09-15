using SQLite;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ArenaVirtual.Models {
    public class Convite : ISyncable, INotifyPropertyChanged {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public Guid ClientAppId { get; set; } = Guid.NewGuid();

        public int? IdServidor { get; set; }

        [Indexed]
        public Guid SolicitanteClientAppId { get; set; }

        [Indexed]
        public Guid TimeClientAppId { get; set; }

        // Propriedade adicionada para corrigir o erro.
        [Indexed]
        public Guid UsuarioClientAppId { get; set; }

        public string? ConvidadoEmail { get; set; }

        public DateTime DataEnvio { get; set; }

        public StatusConvite Status { get; set; }

        private bool _isSynced;
        public bool IsSynced {
            get => _isSynced;
            set => SetProperty(ref _isSynced, value);
        }

        private DateTime _updatedAt;
        public DateTime UpdatedAt {
            get => _updatedAt;
            set => SetProperty(ref _updatedAt, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string? propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(backingField, value)) return false;

            backingField = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            if (propertyName != nameof(IsSynced) && propertyName != nameof(UpdatedAt)) {
                this.IsSynced = false;
                this.UpdatedAt = DateTime.UtcNow;
            }

            return true;
        }
    }

    public enum StatusConvite {
        Pendente,
        Aceito,
        Recusado
    }
}