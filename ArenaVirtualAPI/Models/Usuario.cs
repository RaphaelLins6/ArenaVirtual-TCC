using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ArenaVirtualAPI.Models;

public class Usuario : ISyncable {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Nome { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string SenhaHash { get; set; } = string.Empty;

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

    // relacionamento opcional com Time
    public int? TimeId { get; set; }

    // Propriedades de sincronização
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsSynced { get; set; } = false;
}