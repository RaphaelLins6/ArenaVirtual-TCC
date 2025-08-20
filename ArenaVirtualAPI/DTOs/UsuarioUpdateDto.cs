// Dtos/UsuarioUpdateDto.cs
using System.ComponentModel.DataAnnotations;
using ArenaVirtualAPI.Models;

namespace ArenaVirtualAPI.Dtos {
    public class UsuarioUpdateDto {
        [Required]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Nome { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        // opcional: se vier preenchida, atualiza a senha
        public string? NovaSenha { get; set; }

        public TipoPerfil Perfil { get; set; }
        public string ImagemPath { get; set; } = string.Empty;
        public string Localizacao { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string LinkRedeSocial { get; set; } = string.Empty;
        public DateTime? DataNascimento { get; set; }
        public GeneroEnum? Genero { get; set; }
        public string NomeEmpresa { get; set; } = string.Empty;
        public string CNPJ { get; set; } = string.Empty;
        public double? Peso { get; set; }
        public double? Altura { get; set; }
        public string FaixaOrcamentoPatrocinio { get; set; } = string.Empty;
        public int? TimeId { get; set; }
    }
}
