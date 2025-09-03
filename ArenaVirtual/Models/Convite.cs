using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ArenaVirtual.Models {
    public class Convite : ISyncable, INotifyPropertyChanged {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int IdSolicitante { get; set; }

        [Indexed]
        public int TimeId { get; set; }

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

            this.IsSynced = false;
            this.UpdatedAt = DateTime.UtcNow;

            return true;
        }
    }

    public enum StatusConvite {
        Pendente,
        Aceito,
        Recusado
    }
}
