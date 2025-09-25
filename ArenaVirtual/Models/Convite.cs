using SQLite;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace ArenaVirtual.Models {

    public enum TipoConvite {
        InscricaoCampeonato = 0,
        ConviteTime = 1
    }

    public enum StatusConvite {
        Pendente,
        Aceito,
        Recusado
    }

    public class Convite : ISyncable, INotifyPropertyChanged {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public Guid ClientAppId { get; set; } = Guid.NewGuid();

        public int? IdServidor { get; set; }

        [Indexed]
        public Guid SolicitanteClientAppId { get; set; }

        [Indexed]
        public Guid TimeClientAppId { get; set; }

        [Indexed]
        public Guid UsuarioClientAppId { get; set; }

        public Guid CampeonatoClientAppId { get; set; }

        public TipoConvite Tipo { get; set; }

        public string? ConvidadoEmail { get; set; }

        // ⚡️ Propriedade renomeada para DataEnvio, para resolver o erro de compilação
        public DateTime DataEnvio { get; set; }
        public DateTime DataCriacao { get; set; }

        public StatusConvite Status { get; set; }

        public Convite() { }

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
}