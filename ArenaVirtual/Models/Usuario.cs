using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ArenaVirtual.Models {
    public enum TipoPerfil {
        Atleta,
        Organizador,
        Arbitro,
        Patrocinador
    }

    public enum GeneroEnum {
        Masculino,
        Feminino,
        Outro
    }

    public partial class Usuario : INotifyPropertyChanged, ISyncable {

        // Campos de Suporte (Backing Fields)
        private string _nome = string.Empty;
        private string _email = string.Empty;
        private string _senhaHash = string.Empty;
        private TipoPerfil _perfil;
        private string _imagemPath = string.Empty;
        private string _localizacao = string.Empty;
        private string _telefone = string.Empty;
        private string _linkRedeSocial = string.Empty;
        private DateTime? _dataNascimento;
        private GeneroEnum? _genero;
        private string _nomeEmpresa = string.Empty;
        private string _cnpj = string.Empty;
        private double? _peso;
        private double? _altura;
        private string _faixaOrcamentoPatrocinio = string.Empty;
        private Guid? _timeClientAppId;
        private bool _isSynced;
        private DateTime _updatedAt;

        // Propriedades usando SetProperty
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public Guid ClientAppId { get; set; } = Guid.NewGuid();

        public int? IdServidor { get; set; }

        public string Nome {
            get => _nome;
            set => SetProperty(ref _nome, value);
        }

        public string Email {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public TipoPerfil Perfil {
            get => _perfil;
            set => SetProperty(ref _perfil, value);
        }

        public string ImagemPath {
            get => _imagemPath;
            set => SetProperty(ref _imagemPath, value);
        }

        public string Localizacao {
            get => _localizacao;
            set => SetProperty(ref _localizacao, value);
        }

        public string Telefone {
            get => _telefone;
            set => SetProperty(ref _telefone, value);
        }

        public string LinkRedeSocial {
            get => _linkRedeSocial;
            set => SetProperty(ref _linkRedeSocial, value);
        }

        public DateTime? DataNascimento {
            get => _dataNascimento;
            set => SetProperty(ref _dataNascimento, value);
        }

        public GeneroEnum? Genero {
            get => _genero;
            set => SetProperty(ref _genero, value);
        }

        public string NomeEmpresa {
            get => _nomeEmpresa;
            set => SetProperty(ref _nomeEmpresa, value);
        }

        public string CNPJ {
            get => _cnpj;
            set => SetProperty(ref _cnpj, value);
        }

        public double? Peso {
            get => _peso;
            set => SetProperty(ref _peso, value);
        }

        public double? Altura {
            get => _altura;
            set => SetProperty(ref _altura, value);
        }

        public string FaixaOrcamentoPatrocinio {
            get => _faixaOrcamentoPatrocinio;
            set => SetProperty(ref _faixaOrcamentoPatrocinio, value);
        }

        // Relacionamento com Time usando a chave universal
        public Guid? TimeClientAppId {
            get => _timeClientAppId;
            set => SetProperty(ref _timeClientAppId, value);
        }

        // As propriedades de sincronização que não usam SetProperty
        public bool IsSynced {
            get => _isSynced;
            set => _isSynced = value;
        }

        public DateTime UpdatedAt {
            get => _updatedAt;
            set => _updatedAt = value;
        }

        // A propriedade SenhaHash não deve ser sincronizada,
        // então ela é gerenciada fora da lógica de SetProperty.
        public string SenhaHash {
            get => _senhaHash;
            set => _senhaHash = value;
        }

        // --- CORREÇÃO DE LÓGICA: SOBRECARGA PARA COMPARAÇÃO POR ID (CRUCIAL) ---

        public override bool Equals(object obj) {
            if (obj is Usuario other) {
                // A comparação é feita pela chave única
                return this.ClientAppId.Equals(other.ClientAppId);
            }
            return false;
        }

        public override int GetHashCode() {
            // Usa o hash da ID única
            return ClientAppId.GetHashCode();
        }

        public Usuario() { }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string? propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(backingField, value))
                return false;

            backingField = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            if (propertyName != nameof(IsSynced) && propertyName != nameof(UpdatedAt) && propertyName != nameof(SenhaHash)) {
                this.IsSynced = false;
                this.UpdatedAt = DateTime.UtcNow;
            }

            return true;
        }
    }
}