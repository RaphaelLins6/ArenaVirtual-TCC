using SQLite;
using System;

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

    public class Usuario {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SenhaHash { get; set; } = string.Empty; // Armazene o hash da senha
        public TipoPerfil Perfil { get; set; }

        // Campos comuns
        public string ImagemPath { get; set; } = string.Empty;
        public string Localizacao { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string LinkRedeSocial { get; set; } = string.Empty;

        // Específicos
        public DateTime? DataNascimento { get; set; }
        public GeneroEnum? Genero { get; set; }
        public string NomeEmpresa { get; set; } = string.Empty;
        public string CNPJ { get; set; } = string.Empty;
        public string Modalidades { get; set; } = string.Empty;
        public double? Peso { get; set; }
        public double? Altura { get; set; }
        public string AreasInteressePatrocinio { get; set; } = string.Empty;
        public string FaixaOrcamentoPatrocinio { get; set; } = string.Empty;

        public Usuario() { }
    }
}