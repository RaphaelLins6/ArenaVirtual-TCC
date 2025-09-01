using SQLite;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

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

        // Campos de suporte (backing fields)
        private string nome = string.Empty;
        private string email = string.Empty;
        private string senhaHash = string.Empty;
        private TipoPerfil perfil;
        private string imagemPath = string.Empty;
        private string localizacao = string.Empty;
        private string telefone = string.Empty;
        private string linkRedeSocial = string.Empty;
        private DateTime? dataNascimento;
        private GeneroEnum? genero;
        private string nomeEmpresa = string.Empty;
        private string cnpj = string.Empty;
        private double? peso;
        private double? altura;
        private string faixaOrcamentoPatrocinio = string.Empty;
        private int? timeId;

        // Novos campos de suporte para IsSynced e UpdatedAt para evitar recursão
        private bool isSynced;
        private DateTime updatedAt;

        // Propriedades usando SetProperty
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Nome {
            get => nome;
            set => SetProperty(ref nome, value);
        }

        public string Email {
            get => email;
            set => SetProperty(ref email, value);
        }

        public string SenhaHash {
            get => senhaHash;
            set => SetProperty(ref senhaHash, value);
        }

        public TipoPerfil Perfil {
            get => perfil;
            set => SetProperty(ref perfil, value);
        }

        public string ImagemPath {
            get => imagemPath;
            set => SetProperty(ref imagemPath, value);
        }

        public string Localizacao {
            get => localizacao;
            set => SetProperty(ref localizacao, value);
        }

        public string Telefone {
            get => telefone;
            set => SetProperty(ref telefone, value);
        }

        public string LinkRedeSocial {
            get => linkRedeSocial;
            set => SetProperty(ref linkRedeSocial, value);
        }

        public DateTime? DataNascimento {
            get => dataNascimento;
            set => SetProperty(ref dataNascimento, value);
        }

        public GeneroEnum? Genero {
            get => genero;
            set => SetProperty(ref genero, value);
        }

        public string NomeEmpresa {
            get => nomeEmpresa;
            set => SetProperty(ref nomeEmpresa, value);
        }

        public string CNPJ {
            get => cnpj;
            set => SetProperty(ref cnpj, value);
        }

        public double? Peso {
            get => peso;
            set => SetProperty(ref peso, value);
        }

        public double? Altura {
            get => altura;
            set => SetProperty(ref altura, value);
        }

        public string FaixaOrcamentoPatrocinio {
            get => faixaOrcamentoPatrocinio;
            set => SetProperty(ref faixaOrcamentoPatrocinio, value);
        }

        public int? TimeId {
            get => timeId;
            set => SetProperty(ref timeId, value);
        }

        // Propriedades para sincronização. Elas não chamam SetProperty.
        public bool IsSynced {
            get => isSynced;
            set => isSynced = value;
        }

        public DateTime UpdatedAt {
            get => updatedAt;
            set => updatedAt = value;
        }

        public Usuario() { }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string? propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(backingField, value))
                return false;

            backingField = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            // Atualize os campos de suporte diretamente para evitar recursão
            this.isSynced = false;
            this.updatedAt = DateTime.UtcNow;

            return true;
        }
    }
}