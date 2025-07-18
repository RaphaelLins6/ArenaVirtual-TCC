using SQLite;
using System;
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

    public partial class Usuario : INotifyPropertyChanged {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        private string nome = string.Empty;
        public string Nome {
            get => nome;
            set => SetProperty(ref nome, value);
        }

        private string email = string.Empty;
        public string Email {
            get => email;
            set => SetProperty(ref email, value);
        }

        private string senhaHash = string.Empty;
        public string SenhaHash {
            get => senhaHash;
            set => SetProperty(ref senhaHash, value);
        }

        private TipoPerfil perfil;
        public TipoPerfil Perfil {
            get => perfil;
            set => SetProperty(ref perfil, value);
        }

        private string imagemPath = string.Empty;
        public string ImagemPath {
            get => imagemPath;
            set => SetProperty(ref imagemPath, value);
        }

        private string localizacao = string.Empty;
        public string Localizacao {
            get => localizacao;
            set => SetProperty(ref localizacao, value);
        }

        private string telefone = string.Empty;
        public string Telefone {
            get => telefone;
            set => SetProperty(ref telefone, value);
        }

        private string linkRedeSocial = string.Empty;
        public string LinkRedeSocial {
            get => linkRedeSocial;
            set => SetProperty(ref linkRedeSocial, value);
        }

        private DateTime? dataNascimento;
        public DateTime? DataNascimento {
            get => dataNascimento;
            set => SetProperty(ref dataNascimento, value);
        }

        private GeneroEnum? genero;
        public GeneroEnum? Genero {
            get => genero;
            set => SetProperty(ref genero, value);
        }

        private string nomeEmpresa = string.Empty;
        public string NomeEmpresa {
            get => nomeEmpresa;
            set => SetProperty(ref nomeEmpresa, value);
        }

        private string cnpj = string.Empty;
        public string CNPJ {
            get => cnpj;
            set => SetProperty(ref cnpj, value);
        }

        private string modalidades = string.Empty;
        public string Modalidades {
            get => modalidades;
            set => SetProperty(ref modalidades, value);
        }

        private double? peso;
        public double? Peso {
            get => peso;
            set => SetProperty(ref peso, value);
        }

        private double? altura;
        public double? Altura {
            get => altura;
            set => SetProperty(ref altura, value);
        }

        private string areasInteressePatrocinio = string.Empty;
        public string AreasInteressePatrocinio {
            get => areasInteressePatrocinio;
            set => SetProperty(ref areasInteressePatrocinio, value);
        }

        private string faixaOrcamentoPatrocinio = string.Empty;
        public string FaixaOrcamentoPatrocinio {
            get => faixaOrcamentoPatrocinio;
            set => SetProperty(ref faixaOrcamentoPatrocinio, value);
        }

        public Usuario() { }

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