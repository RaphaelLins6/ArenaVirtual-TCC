using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace ArenaVirtual.Models {
    public partial class Campeonato : ObservableObject, ISyncable {
        [PrimaryKey, AutoIncrement]
        [JsonIgnore]
        public int Id { get; set; }
        [NotNull, Unique]
        public Guid ClientAppId { get; set; }
        public int? IdServidor { get; set; }

        private string? _nome;
        private string? _local;
        private bool _ehFavorito;
        private DateTime _dataInicio;
        private DateTime _dataFim;
        private int _organizadorId;
        private Guid? _organizadorClientAppId;
        private string? _logoUrl;
        private string? _nomeOrganizador;
        private string? _emailOrganizador;
        private string? _telefoneOrganizador;
        private int _numeroMaximoEquipes;
        private decimal _valorTaxaInscricao;
        private string? _formatoCampeonato;
        private string? _locaisDosJogos;
        private bool _haveraPremiacao;
        private string? _descricao;
        private string? _modalidade;
        private string? _regras;
        private DateTime? _dataTermino;
        private int? _numeroEquipes;
        private string? _bannerUrl;

        public bool IsSynced { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid CapitaoClientAppId { get; set; }

        [NotNull, MaxLength(100)]
        public string? Nome {
            get => _nome;
            set => SetProperty(ref _nome, value);
        }

        [MaxLength(150)]
        public string? Local {
            get => _local;
            set => SetProperty(ref _local, value);
        }

        [Ignore]
        public bool EhFavorito {
            get => _ehFavorito;
            set => SetProperty(ref _ehFavorito, value);
        }

        [NotNull]
        public DateTime DataInicio {
            get => _dataInicio;
            set => SetProperty(ref _dataInicio, value);
        }

        [NotNull]
        public DateTime DataFim {
            get => _dataFim;
            set => SetProperty(ref _dataFim, value);
        }

        public int OrganizadorId {
            get => _organizadorId;
            set => SetProperty(ref _organizadorId, value);
        }

        public Guid? OrganizadorClientAppId {
            get => _organizadorClientAppId;
            set => SetProperty(ref _organizadorClientAppId, value);
        }

        [MaxLength(255)]
        public string? LogoUrl {
            get => _logoUrl;
            set => SetProperty(ref _logoUrl, value);
        }

        [MaxLength(255)]
        public string? BannerUrl {
            get => _bannerUrl;
            set => SetProperty(ref _bannerUrl, value);
        }

        [MaxLength(100)]
        public string? NomeOrganizador {
            get => _nomeOrganizador;
            set => SetProperty(ref _nomeOrganizador, value);
        }

        [MaxLength(100)]
        public string? EmailOrganizador {
            get => _emailOrganizador;
            set => SetProperty(ref _emailOrganizador, value);
        }

        [MaxLength(20)]
        public string? TelefoneOrganizador {
            get => _telefoneOrganizador;
            set => SetProperty(ref _telefoneOrganizador, value);
        }

        public int NumeroMaximoEquipes {
            get => _numeroMaximoEquipes;
            set => SetProperty(ref _numeroMaximoEquipes, value);
        }

        public decimal ValorTaxaInscricao {
            get => _valorTaxaInscricao;
            set => SetProperty(ref _valorTaxaInscricao, value);
        }

        [MaxLength(50)]
        public string? FormatoCampeonato {
            get => _formatoCampeonato;
            set => SetProperty(ref _formatoCampeonato, value);
        }

        [MaxLength(255)]
        public string? LocaisDosJogos {
            get => _locaisDosJogos;
            set => SetProperty(ref _locaisDosJogos, value);
        }

        public bool HaveraPremiacao {
            get => _haveraPremiacao;
            set => SetProperty(ref _haveraPremiacao, value);
        }

        [MaxLength(500)]
        public string? Descricao {
            get => _descricao;
            set => SetProperty(ref _descricao, value);
        }

        [MaxLength(50)]
        public string? Modalidade {
            get => _modalidade;
            set => SetProperty(ref _modalidade, value);
        }

        [MaxLength(500)]
        public string? Regras {
            get => _regras;
            set => SetProperty(ref _regras, value);
        }

        public DateTime? DataTermino {
            get => _dataTermino;
            set => SetProperty(ref _dataTermino, value);
        }

        public int? NumeroEquipes {
            get => _numeroEquipes;
            set => SetProperty(ref _numeroEquipes, value);
        }

        public Campeonato() {
            IsSynced = false;
            UpdatedAt = DateTime.UtcNow;
            _nome = string.Empty;
            _local = string.Empty;
            _logoUrl = string.Empty;
            _bannerUrl = string.Empty;
            _nomeOrganizador = string.Empty;
            _emailOrganizador = string.Empty;
            _telefoneOrganizador = string.Empty;
            _formatoCampeonato = string.Empty;
            _locaisDosJogos = string.Empty;
            _dataInicio = DateTime.Now;
            _dataFim = DateTime.Now;
            ClientAppId = Guid.NewGuid();
            _descricao = string.Empty;
            _modalidade = string.Empty;
            _regras = string.Empty;
            _dataTermino = null;
            _numeroEquipes = null;
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
}